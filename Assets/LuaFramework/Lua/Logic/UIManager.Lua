------------------------------------------------------
--- FileName: UIManager.lua
--- Created By SiMaLaoShi.
--- DateTime: 2019/1/11 10:35
--- Describe: UI管理器
------------------------------------------------------

local this
UIManager = {}

function UIManager.Start()
    this = UIManager
end

function UIManager.CreateUISync(nPath,bPop,parent,nType)
    if not bPop then
        bPop = false
    end
    if not nType then
        nType = UIType.Default
    end
    return uiMgr:CreateUISync(nPath,bPop,parent,nType)
end

function UIManager.CloseUI(uiBase)
    if not uiBase then
        error("关闭的ui为空")
        return
    end
    uiBase:Close()
end

function UIManager.CreateUIAsync()

end

function UIManager.ShutDown()
    this.UnRegisterMessage()
end 

