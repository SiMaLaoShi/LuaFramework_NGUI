------------------------------------------------------
--- FileName: UIMessageBox.lua
--- Created By SiMaLaoShi.
--- DateTime: 2019/1/11 11:41
--- Describe: 消息提示框
------------------------------------------------------

local gameObject, transform, this, uiBase

UIMessageBox = {}

function UIMessageBox.Awake()
    this = UIMessageBox
    uiBase = this.uiBase
    gameObject = this.gameObject
    transform = this.gameObject.transform

    this.InitWidget()
    this.InitEvent()
    this.Init()
end

function UIMessageBox.Start()

end

function UIMessageBox.InitWidget()
    this.btnClose = transform:Find("Button")
end

function UIMessageBox.InitEvent()
    uiBase:AddOnClickListener(this.btnClose,this.CloseClick,0)
end

function UIMessageBox.CloseClick()
    UIManager.CreateUISync(UIConfig.UILogin,false)
end

function UIMessageBox.Init()

end

function UIMessageBox.OnDestroy()

end 