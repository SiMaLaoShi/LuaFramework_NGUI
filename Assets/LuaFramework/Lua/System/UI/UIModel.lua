------------------------------------------------------
--- FileName: UIModel.lua
--- Created By SiMaLaoShi.
--- DateTime: 2019/1/11 14:37
--- Describe: 独立的界面
------------------------------------------------------

local gameObject, transform, this, uiBase

UIModel = {}

function UIModel.Awake()
    this = UIModel
    uiBase = this.uiBase
    gameObject = this.gameObject
    transform = this.gameObject.transform

    this.InitWidget()
    this.InitEvent()
    this.Init()
end

function UIModel.Start()

end

function UIModel.InitWidget()

end

function UIModel.InitEvent()

end

function UIModel.Init()

end

function UIModel.OnDestroy()

end 