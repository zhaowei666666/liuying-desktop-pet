using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfPoint = System.Windows.Point;

namespace LiuYingPet;

public partial class MainWindow : Window
{
    private const double BaseWidth = 240;
    private const double BaseHeight = 270;
    private const double CloseDistance = 120;

    private readonly SettingsService _settingsService = new();
    private readonly AssetCatalog _assets = new();
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly TimeOnly? _timeOverride;
    private readonly PetSettings _settings;
    private readonly List<Forms.ToolStripMenuItem> _scaleItems = [];

    private Forms.NotifyIcon? _trayIcon;
    private Forms.ToolStripMenuItem? _showItem;
    private Forms.ToolStripMenuItem? _topMostItem;
    private Forms.ToolStripMenuItem? _startupItem;

    private PetPose _currentPose = PetPose.Idle;
    private DateTime _poseChangedAt = DateTime.Now;
    private PetPose? _forcedPose;
    private DateTime _forcedUntil = DateTime.MinValue;
    private DateTime _lastStartleAt = DateTime.MinValue;
    private DateTime _lastTick = DateTime.Now;
    private WpfPoint? _lastCursor;
    private PetPose? _loadedPose;
    private MotionSample _currentMotion = MotionSample.Neutral;
    private double _cursorSpeed;
    private string? _currentSpriteProblem;
    private bool _isExitRequested;

    private bool _isDragging;
    private bool _dragMoved;
    private WpfPoint _dragStartCursor;
    private WpfPoint _dragStartWindow;

    public MainWindow(string[] args)
    {
        InitializeComponent();

        _timeOverride = CommandLineTime.Parse(args);
        _settings = _settingsService.Load();
        _settings.Scale = ClampScale(_settings.Scale);
        _settings.StartWithWindows = StartupService.IsEnabled();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _timer.Tick += Timer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Topmost = _settings.TopMost;
        ApplyScale(_settings.Scale, save: false);
        PlaceInitialWindow();
        CreateTrayIcon();
        ForcePose(PetPose.Wave, TimeSpan.FromSeconds(1.7));
        _timer.Start();
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_isExitRequested)
        {
            SaveWindowPlacement();
            _trayIcon?.Dispose();
            return;
        }

        e.Cancel = true;
        Hide();
        RefreshTrayMenu();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        try
        {
            var now = DateTime.Now;
            var dt = Math.Max(0.001, (now - _lastTick).TotalSeconds);
            _lastTick = now;

            var cursor = GetCursorScreenDip();
            if (_lastCursor is { } last)
            {
                var dx = cursor.X - last.X;
                var dy = cursor.Y - last.Y;
                _cursorSpeed = Math.Sqrt(dx * dx + dy * dy) / dt;
            }

            _lastCursor = cursor;

            if (_isDragging)
            {
                ApplyPose(PetPose.Dragged);
                ApplyAnimation(PetPose.Dragged, dt);
                UpdateStatusBubble(PetPose.Dragged);
                return;
            }

            var pose = SelectPose(cursor, now);
            ApplyPose(pose);
            ApplyAnimation(pose, dt);
            UpdateStatusBubble(pose);
        }
        catch (Exception exception)
        {
            ErrorLogService.Append(exception);
        }
    }

    private PetPose SelectPose(WpfPoint cursor, DateTime now)
    {
        if (_forcedPose is { } forcedPose && now < _forcedUntil)
        {
            return forcedPose;
        }

        _forcedPose = null;

        var center = GetWindowCenter();
        var dx = cursor.X - center.X;
        var dy = cursor.Y - center.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);

        if (_cursorSpeed > 1450 &&
            distance < 190 &&
            (now - _lastStartleAt).TotalSeconds > 1.8)
        {
            _lastStartleAt = now;
            ForcePose(PetPose.Startled, TimeSpan.FromSeconds(1.0));
            return PetPose.Startled;
        }

        if (distance < CloseDistance)
        {
            return PetPose.Happy;
        }

        if (Math.Abs(dx) > 70 || dy < -55)
        {
            return dx < 0 ? PetPose.LookLeft : PetPose.LookRight;
        }

        return GetTimePose(now);
    }

    private PetPose GetTimePose(DateTime now)
    {
        var time = _timeOverride ?? TimeOnly.FromDateTime(now);
        var seconds = (int)_clock.Elapsed.TotalSeconds;

        if (time.Hour < 6)
        {
            return PetPose.DeepNight;
        }

        if (seconds % 10 < 2)
        {
            return PetPose.Blink;
        }

        if (time.Hour < 11)
        {
            return seconds % 18 < 5 ? PetPose.Morning : PetPose.Idle;
        }

        if (time.Hour < 16)
        {
            return seconds % 18 < 5 ? PetPose.Noon : PetPose.Idle;
        }

        if (time.Hour < 21)
        {
            return seconds % 18 < 5 ? PetPose.Evening : PetPose.Idle;
        }

        return seconds % 18 < 6 ? PetPose.Night : PetPose.Idle;
    }

    private void ApplyPose(PetPose pose)
    {
        if (pose == _loadedPose)
        {
            return;
        }

        _currentPose = pose;
        _loadedPose = pose;
        _poseChangedAt = DateTime.Now;

        var image = _assets.TryLoad(pose, out var problem);
        _currentSpriteProblem = string.IsNullOrWhiteSpace(problem) ? null : problem;

        if (image is null)
        {
            SpriteImage.Source = null;
            SpriteImage.Visibility = Visibility.Collapsed;
            FallbackPet.Visibility = Visibility.Visible;
            return;
        }

        SpriteImage.Source = image;
        SpriteImage.Visibility = Visibility.Visible;
        FallbackPet.Visibility = Visibility.Collapsed;
    }

    private void ApplyAnimation(PetPose pose, double dt)
    {
        var elapsedInPose = (DateTime.Now - _poseChangedAt).TotalSeconds;
        var target = KeyframeMotionLibrary.Sample(pose, elapsedInPose);
        var smoothing = 1 - Math.Exp(-Math.Clamp(dt, 0.001, 0.08) * 14);
        _currentMotion = MotionSample.Lerp(_currentMotion, target, smoothing);

        SpriteScale.ScaleX = _currentMotion.ScaleX;
        SpriteScale.ScaleY = _currentMotion.ScaleY;
        SpriteRotate.Angle = _currentMotion.Rotation;
        SpriteTranslate.X = _currentMotion.X;
        SpriteTranslate.Y = _currentMotion.Y;
    }

    private void UpdateStatusBubble(PetPose pose)
    {
        if (_currentSpriteProblem is not null)
        {
            StatusText.Text = $"{_currentSpriteProblem}\n把 PNG 放进 assets 文件夹";
            StatusBubble.Opacity = 0.94;
            return;
        }

        var visibleFor = (DateTime.Now - _poseChangedAt).TotalSeconds;
        var shouldSpeak = pose is PetPose.Wave or PetPose.Happy or PetPose.Startled or PetPose.Dragged
            or PetPose.Morning or PetPose.Noon or PetPose.Evening or PetPose.Night or PetPose.DeepNight
            or PetPose.Clicked;

        if (shouldSpeak && visibleFor < 1.8)
        {
            StatusText.Text = PetPoseText.GetBubble(pose);
            StatusBubble.Opacity = 0.84;
            return;
        }

        StatusBubble.Opacity = 0;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _dragMoved = false;
        _dragStartCursor = GetCursorScreenDip();
        _dragStartWindow = new WpfPoint(Left, Top);
        CaptureMouse();
        ForcePose(PetPose.Dragged, TimeSpan.FromMinutes(5));
        e.Handled = true;
    }

    private void Window_MouseMove(object sender, WpfMouseEventArgs e)
    {
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var cursor = GetCursorScreenDip();
        var dx = cursor.X - _dragStartCursor.X;
        var dy = cursor.Y - _dragStartCursor.Y;

        if (Math.Abs(dx) > 3 || Math.Abs(dy) > 3)
        {
            _dragMoved = true;
        }

        Left = _dragStartWindow.X + dx;
        Top = _dragStartWindow.Y + dy;
        ClampWindowToScreen();
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging)
        {
            return;
        }

        _isDragging = false;
        ReleaseMouseCapture();
        SaveWindowPlacement();

        if (_dragMoved)
        {
            ForcePose(PetPose.Dragged, TimeSpan.FromSeconds(0.5));
        }
        else
        {
            ForcePose(PetPose.Clicked, TimeSpan.FromSeconds(1.5));
        }

        e.Handled = true;
    }

    private void ForcePose(PetPose pose, TimeSpan duration)
    {
        _forcedPose = pose;
        _forcedUntil = DateTime.Now + duration;
    }

    private void ApplyScale(double scale, bool save)
    {
        scale = ClampScale(scale);
        _settings.Scale = scale;
        Width = BaseWidth * scale;
        Height = BaseHeight * scale;
        Stage.LayoutTransform = new ScaleTransform(scale, scale);

        if (save)
        {
            SaveWindowPlacement();
        }

        RefreshTrayMenu();
    }

    private static double ClampScale(double scale)
    {
        if (double.IsNaN(scale) || double.IsInfinity(scale))
        {
            return 1.0;
        }

        return Math.Clamp(scale, 0.75, 1.5);
    }

    private void PlaceInitialWindow()
    {
        if (_settings.Left is { } left && _settings.Top is { } top)
        {
            Left = left;
            Top = top;
            ClampWindowToScreen();
            return;
        }

        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 36;
        Top = workArea.Bottom - Height - 36;
    }

    private void SaveWindowPlacement()
    {
        _settings.Left = Left;
        _settings.Top = Top;
        _settingsService.Save(_settings);
    }

    private void ClampWindowToScreen()
    {
        var minLeft = SystemParameters.VirtualScreenLeft;
        var minTop = SystemParameters.VirtualScreenTop;
        var maxLeft = minLeft + SystemParameters.VirtualScreenWidth - Width;
        var maxTop = minTop + SystemParameters.VirtualScreenHeight - Height;

        Left = Math.Clamp(Left, minLeft, Math.Max(minLeft, maxLeft));
        Top = Math.Clamp(Top, minTop, Math.Max(minTop, maxTop));
    }

    private WpfPoint GetWindowCenter() => new(Left + Width / 2, Top + Height / 2);

    private WpfPoint GetCursorScreenDip()
    {
        if (!GetCursorPos(out var point))
        {
            return GetWindowCenter();
        }

        var screenPoint = new WpfPoint(point.X, point.Y);
        var source = PresentationSource.FromVisual(this);
        return source?.CompositionTarget?.TransformFromDevice.Transform(screenPoint) ?? screenPoint;
    }

    private void CreateTrayIcon()
    {
        if (_trayIcon is not null)
        {
            return;
        }

        var menu = new Forms.ContextMenuStrip();
        _showItem = new Forms.ToolStripMenuItem("隐藏桌宠", null, (_, _) => ToggleVisibility());
        _topMostItem = new Forms.ToolStripMenuItem("置顶显示") { CheckOnClick = true };
        _startupItem = new Forms.ToolStripMenuItem("开机自启") { CheckOnClick = true };

        _topMostItem.Click += (_, _) =>
        {
            _settings.TopMost = _topMostItem.Checked;
            Topmost = _settings.TopMost;
            _settingsService.Save(_settings);
        };
        _startupItem.Click += (_, _) =>
        {
            _settings.StartWithWindows = _startupItem.Checked;
            StartupService.SetEnabled(_settings.StartWithWindows);
            _settingsService.Save(_settings);
            RefreshTrayMenu();
        };

        menu.Items.Add(_showItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(_topMostItem);
        menu.Items.Add(_startupItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(CreateScaleItem("缩放 75%", 0.75));
        menu.Items.Add(CreateScaleItem("缩放 100%", 1.0));
        menu.Items.Add(CreateScaleItem("缩放 125%", 1.25));
        menu.Items.Add(CreateScaleItem("缩放 150%", 1.5));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(new Forms.ToolStripMenuItem("重新加载素材", null, (_, _) => ReloadAssets()));
        menu.Items.Add(new Forms.ToolStripMenuItem("打开素材文件夹", null, (_, _) => OpenAssetsFolder()));
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(new Forms.ToolStripMenuItem("退出", null, (_, _) => ExitApplication()));
        menu.Opening += (_, _) => RefreshTrayMenu();

        _trayIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Information,
            Text = "流萤桌宠",
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => ToggleVisibility();
        RefreshTrayMenu();
    }

    private Forms.ToolStripMenuItem CreateScaleItem(string text, double scale)
    {
        var item = new Forms.ToolStripMenuItem(text);
        item.Click += (_, _) => ApplyScale(scale, save: true);
        item.Tag = scale;
        _scaleItems.Add(item);
        return item;
    }

    private void RefreshTrayMenu()
    {
        if (_showItem is null)
        {
            return;
        }

        _showItem.Text = IsVisible ? "隐藏桌宠" : "显示桌宠";

        if (_topMostItem is not null)
        {
            _topMostItem.Checked = _settings.TopMost;
        }

        if (_startupItem is not null)
        {
            _startupItem.Checked = StartupService.IsEnabled();
        }

        foreach (var item in _scaleItems)
        {
            if (item.Tag is double scale)
            {
                item.Checked = Math.Abs(_settings.Scale - scale) < 0.001;
            }
        }
    }

    private void ToggleVisibility()
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            Show();
            Activate();
        }

        RefreshTrayMenu();
    }

    private void ReloadAssets()
    {
        _assets.ClearCache();
        _loadedPose = null;
        SpriteImage.Source = null;
        ApplyPose(_currentPose);
        ForcePose(PetPose.Wave, TimeSpan.FromSeconds(1.2));
    }

    private void OpenAssetsFolder()
    {
        Directory.CreateDirectory(_assets.AssetsDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = _assets.AssetsDirectory,
            UseShellExecute = true
        });
    }

    private void ExitApplication()
    {
        _isExitRequested = true;
        _timer.Stop();
        SaveWindowPlacement();
        _trayIcon?.Dispose();
        System.Windows.Application.Current.Shutdown();
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out PointStruct lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct PointStruct
    {
        public int X;
        public int Y;
    }
}
