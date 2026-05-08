namespace LiuYingPet;

internal enum PetPose
{
    Idle,
    Blink,
    Wave,
    Happy,
    Startled,
    Dragged,
    RunRight1,
    RunRight2,
    RunLeft1,
    RunLeft2,
    Morning,
    Noon,
    Evening,
    Night,
    DeepNight,
    Clicked,
    ReleaseBounce
}

internal static class PetPoseText
{
    public static string GetLabel(PetPose pose) => pose switch
    {
        PetPose.Idle => "待机",
        PetPose.Blink => "眨眼",
        PetPose.Wave => "打招呼",
        PetPose.Happy => "开心",
        PetPose.Startled => "吓一跳",
        PetPose.Dragged => "被抱起",
        PetPose.RunRight1 or PetPose.RunRight2 => "向右小跑",
        PetPose.RunLeft1 or PetPose.RunLeft2 => "向左小跑",
        PetPose.Morning => "早安",
        PetPose.Noon => "元气满满",
        PetPose.Evening => "傍晚陪伴",
        PetPose.Night => "有点困",
        PetPose.DeepNight => "睡着啦",
        PetPose.Clicked => "害羞开心",
        PetPose.ReleaseBounce => "回弹",
        _ => "流萤"
    };

    public static string GetBubble(PetPose pose) => pose switch
    {
        PetPose.Wave => "你好呀~",
        PetPose.Happy => "你靠近啦！",
        PetPose.Startled => "欸？吓我一跳",
        PetPose.Dragged => "轻一点抱我~",
        PetPose.Morning => "早安，今天也一起加油",
        PetPose.Noon => "状态很好！",
        PetPose.Evening => "休息一下也可以哦",
        PetPose.Night => "有点困啦",
        PetPose.DeepNight => "该睡觉了……",
        PetPose.Clicked => "嘿嘿，在这里",
        _ => GetLabel(pose)
    };
}
