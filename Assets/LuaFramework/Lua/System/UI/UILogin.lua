------------------------------------------------------
--- FileName: UILogin.lua
--- Created By SiMaLaoShi.
--- DateTime: 2019/1/11 11:08
--- Describe: 登录界面
------------------------------------------------------

local gameObject, transform, this, uiBase
local ActorFashionTable = require("Table.ActorFashionTable")
UILogin = {}

function UILogin.Awake()
    this = UILogin
    uiBase = this.uiBase
    gameObject = this.gameObject
    transform = this.transform

    this.InitWidget()
    this.InitEvent()
    this.Init()
end

function UILogin.Start()
    print("登录界面开始")
end

function UILogin.InitWidget()
    this.btnOpen = transform:Find("Open")
end

function UILogin.InitEvent()
    uiBase:AddOnClickListener(this.btnOpen,this.OpenClick,0)
end

function UILogin.OpenClick(go,i)
   UIManager.CreateUISync(UIConfig.UIMainWndFull,false,nil,UIType.MainWnd)
end


function UILogin.Init()

end

function UILogin.OnDestroy()

end 