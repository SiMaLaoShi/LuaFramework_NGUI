------------------------------------------------------
--- FileName: UIMainWndFull.lua
--- Created By SiMaLaoShi.
--- DateTime: 2019/1/11 14:38
--- Describe: 全屏的界面
------------------------------------------------------

local gameObject, transform, this, uiBase

UIMainWndFull = {}

function UIMainWndFull.Awake()
    this = UIMainWndFull
    uiBase = this.uiBase
    gameObject = this.gameObject
    transform = this.gameObject.transform

    this.InitWidget()
    this.InitEvent()
    this.Init()
end

function UIMainWndFull.Start()
    UIManager.CreateUISync(UIConfig.UISubWnd,false,uiBase,UIType.SubWnd)
end

function UIMainWndFull.InitWidget()
    this.btnClose = transform:Find("BtnClose")
    this.btnOpen = transform:Find("Open")
end

function UIMainWndFull.InitEvent()
    uiBase:AddOnClickListener(this.btnClose,this.CloseClick,0)
    uiBase:AddOnClickListener(this.btnOpen,this.OpenClick,0)
end

function UIMainWndFull.CloseClick(go,i)
    UIManager.CloseUI(uiBase)
end

function UIMainWndFull.OpenClick(go,i)
    UIManager.CreateUISync(UIConfig.UIMessageBox,false,nil,UIType.Tips)
end

function UIMainWndFull.Init()

end

function UIMainWndFull.OnDestroy()

end 