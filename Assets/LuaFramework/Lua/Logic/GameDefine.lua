
UIConfig = {
    UILogin = "Prefabs/UILogin",
    UIMessageBox = "Prefabs/UIMessageBox",
}

UIType =
{
    --默认使用Prefab设置的
    Default = 0,
    --子窗口模式(附带脚本,可有窗口行为
    MainWnd = 1,
    --主窗口模式(主要窗口,拥有单独的bundle)
    SubWnd = 2,
    --独占窗口模式(会关闭其他窗口)
    Single = 3,
    --消息提示(普通非互斥)
    Tips = 4,
    --独占消息提示(此消息与其他互斥)
    SingleTips = 5,
    --顶级消息提示(独占且置顶，用于死亡断线等提示)
    TopTips = 6,
    --UI子物体,不附带脚本,做动态加载用
    UIItem = 7,
}