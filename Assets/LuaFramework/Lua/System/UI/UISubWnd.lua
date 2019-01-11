------------------------------------------------------
--- FileName: UISubWnd.lua
--- Created By SiMaLaoShi.
--- DateTime: 2019/1/11 14:38
--- Describe: 依附在其他界面上的界面
------------------------------------------------------

local gameObject, transform, this, uiBase

UISubWnd = {}

function UISubWnd.Awake()
    this = UISubWnd
    uiBase = this.uiBase
    gameObject = this.gameObject
    transform = this.gameObject.transform

    this.InitWidget()
    this.InitEvent()
    this.Init()
end

function UISubWnd.Start()

end

function UISubWnd.InitWidget()
    --this.btnOpen = transform:Find("Button")
end

function UISubWnd.InitEvent()
    --uiBase:AddOnClickListener(this.btnOpen,this.OpenClick,0)
end

function UISubWnd.OpenClick(go,i)
    UIManager.CreateUISync(UIConfig.UIMessageBox)
end

function UISubWnd.Init()

end

function UISubWnd.OnDestroy()

end 