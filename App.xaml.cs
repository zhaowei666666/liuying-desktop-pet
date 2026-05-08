using System.Windows;
using System.Windows.Threading;

namespace LiuYingPet;

public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        EnsureWpfEnvironment();
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var window = new MainWindow(e.Args);
        MainWindow = window;
        window.Show();
    }

    private static void EnsureWpfEnvironment()
    {
        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("windir")))
        {
            return;
        }

        var systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
        Environment.SetEnvironmentVariable(
            "windir",
            string.IsNullOrWhiteSpace(systemRoot) ? @"C:\Windows" : systemRoot,
            EnvironmentVariableTarget.Process);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ErrorLogService.Append(e.Exception);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            ErrorLogService.Append(exception);
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ErrorLogService.Append(e.Exception);
        e.SetObserved();
    }
}
