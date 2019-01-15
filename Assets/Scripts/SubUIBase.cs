using System;
using UnityEngine;
using System.Collections;
using LuaInterface;
using System.Collections.Generic;
using LuaFramework;
using System.IO;

public enum UIType
{
    /// <summary> 主窗口模式(主要窗口,拥有单独的bundle) 必须拥有UIPanel </summary>
    MainWnd = 1,

    /// <summary> 子窗口模式(附带脚本,可有窗口行为) </summary>
    SubWnd,

    /// <summary> 独占窗口模式(会关闭其他窗口) </summary>
    Modal,

    /// <summary> 消息提示(普通非互斥) 2</summary>
    Tips,

    /// <summary> 独占消息提示(此消息与其他互斥) 2</summary>
    SingleTips,

    /// <summary> 顶级消息提示(独占且置顶，用于死亡断线等提示) 3</summary>
    TopTips,

    /// <summary> UI子物体,不附带脚本,做动态加载用 </summary>
    UIItem,
}

[ExecuteInEditMode]
public class SubUIBase : MonoBehaviour {
    public enum Anchor
    {
        None = 0,
        Right,
        TopLeft,
        Center,
        Space3d,
        TopRight,
        BottomLeft,
        Left,
        Bottom,
        BottomRight
    }

    public enum WndState
    {
        Init = 0,
        Opened,
        Suspend,
        Hiden,
        Closed,
    }

    private static UIManager uiMgr;

    // UI锚点
    public Anchor anchor = Anchor.Center;
    // 关闭ui的时候是否销毁
    public bool bdestory = true; 
    // 是否已经加了了lua脚本
    protected bool beSetLuaData;
    // Start 方法是否调用
    protected bool beStart; 
    // 在lua中是否是全局变量(静态)
    public bool beUnique = true; 
    // 是否使用lua脚本控制
    public bool beUseLua = true;
    // 是否是主界面上的UI(头像，聊天框等等)
    public bool bMainUI = false; 
    // 全屏
    public bool isFullUI = false;

    public bool isHiedUI = false;

    // lua脚本名字
    protected string moduleName = string.Empty;
    // 自身Transform组件
    protected Transform myTrans;

    public Action OnUIClosed = delegate { }; //关闭时的委托

    // 依附的ui
    public SubUIBase parentUI;
    // 附属ui的状态，显示或者隐藏
    public List<bool> childStates = new List<bool>();
    // 附属ui列表
    public List<SubUIBase> childUIs = new List<SubUIBase>();

    protected LuaTable self;
    protected LuaState luaState;
    protected List<LuaBaseRef> funcList = new List<LuaBaseRef>();
    
    [SerializeField]
    protected WndState state = WndState.Init; //窗口状态
    public UIType type = UIType.SubWnd; //UI类型
    private Vector3 initTrans;

    #region 属性
    public LuaTable table
    {
        get { return self; }
    }

    public WndState State
    {
        get { return state; }
        set { state = value; }
    }

    #endregion


    public bool isOneLvUI { get; internal set; }

    protected virtual void Init()
    {
        myTrans = transform;
    }

    public virtual void Close()
    {            
        if (bdestory)
            Destroy();
        else
            HideUI();
    }

    public virtual void SetAutoClose()
    {

    }

    protected virtual void Attach()
    {
        if (Application.isPlaying == false)
            return;
        var root = bMainUI ? UIHelper.mainUIRoot : UIHelper.uiRoot;

        if (bMainUI)
        {
            var pos = transform.localPosition;
            pos.z = 800;
            transform.localPosition = pos;
        }

        /*已经挂接到 uiroot 内了*/
        if (myTrans.root == root) return;

        switch (anchor)
        {
            case Anchor.Right:
                root = root.FindChild("Right");
                break;
            case Anchor.TopLeft:
                root = root.FindChild("TopLeft");
                break;
            case Anchor.Center:
                root = root.FindChild("Center");
                break;
            case Anchor.Space3d:
                root = UnGfx.FindNode(root, "UI3D");
                break;
            case Anchor.TopRight:
                root = UnGfx.FindNode(root, "TopRight");
                break;
            case Anchor.BottomLeft:
                root = root.FindChild("BottomLeft");
                break;
            case Anchor.Left:
                root = root.FindChild("Left");
                break;
            case Anchor.Bottom:
                root = root.FindChild("Bottom");
                break;
            case Anchor.BottomRight:
                root = UnGfx.FindNode(root, "BottomRight");
                break;
            case Anchor.None:
            default:
                return;
        }

        UnGfx.Attach(root, myTrans);
    }

    public void SetUITexIcon(UITexture tex, string iconPath)
    {
        var src = LoadTexture(iconPath);
        if (src != null)
            tex.mainTexture = src;
        else
            tex.mainTexture = null;
    }

    public string CorrectPath(string path)
    {
        if (Path.HasExtension(path)) path = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);

        path = path.Replace('\\', '/');
        return path;
    }

    /// <summary>
    ///     同步加载
    /// </summary>
    private Texture LoadTexture(string path)
    {
        path = CorrectPath(path);
        //var tex = LuaHelper.GetResManager().LoadResourcesSyncByType(path, typeof(Texture)) as Texture;
        var tex = new Texture();
        if (!tex) return null;

        return tex;
    }

    #region Unity 生命周期

    protected virtual void Awake()
    {
        if (Application.isPlaying == false) return;

        if (uiMgr == null) uiMgr = LuaHelper.GetUIManager();

        Init();
        state = WndState.Init;
        name = UnGfx.RemoveCloneString(name);
        moduleName = name;
        initTrans = transform.localPosition;

        if (beUseLua && Application.isPlaying)
        {
            luaState = LuaHelper.GetLuaManager().GetLuaState();
            if (InitLua()) CallLuaFunction(moduleName + ".Awake");
        }
    }

    protected virtual void Start()
    {
        if (beUseLua && Application.isPlaying)
        {
            InitLua();
            CallLuaFunction(moduleName + ".Start");
            beStart = true;
        }
    }

    protected virtual void OnEnable()
    {
        if (beStart) CallLuaFunction(moduleName + ".OnEnable");
    }

    protected virtual void OnDisable()
    {
        CallLuaFunction(moduleName + ".OnDisable");
    }

    protected virtual void OnDestroy()
    {
        Destroy();
        SafeRelease(ref self);
    }
    #endregion

    #region Lua 脚本操作

    protected void SafeRelease(ref LuaFunction func)
    {
        if (func != null)
        {
            func.Dispose();
            func = null;
        }
    }

    protected void SafeRelease(ref LuaTable table)
    {
        if (table != null)
        {
            table.Dispose();
            table = null;
        }
    }

    public void AddLuaFunList(LuaFunction func)
    {
        funcList.Add(func);
    }

    protected virtual void CallLuaFunction(string name)
    {
        if (beUseLua == false) return;
        if (luaState == null || LuaHelper.GetLuaManager() == null) return;
        luaState.Call(name, false);
    }

    private bool InitLua()
    {
        if (!beSetLuaData)
        {
            beSetLuaData = true;

            luaState.Require("System/UI/" + moduleName);

            self = luaState.GetTable(moduleName);

            if (!beUnique)
            {
                var func = self.RawGetLuaFunction("New");
                func.BeginPCall();
                func.PCall();
                self = func.CheckLuaTable();
                func.EndPCall();
                func.Dispose();
            }

            self.name = moduleName;
            self["name"] = moduleName;
            SetUserData();
            return true;
        }

        return false;
    }

    private void SetUserData()
    {
        self["gameObject"] = gameObject;
        self["transform"] = transform;
        self["uiBase"] = this;
    }
    #endregion

    #region 窗口的的生命周期

    public virtual void ShowUI()
    {
        if (state == WndState.Opened)
            return;
        gameObject.SetActive(true);
        state = WndState.Opened;
        uiMgr.ShowUI(this);
        if (parentUI != null)
            if (parentUI.state == WndState.Suspend)
                SuspendUI();
        if (beUseLua) CallLuaFunction(moduleName + ".ShowUI");
    }

    public virtual bool SuspendUI()
    {
        if (state == WndState.Opened)
        {
            state = WndState.Suspend;
            var v3 = transform.localPosition;
            v3.y += 10000f;
            transform.localPosition = v3;
            CheckList(childUIs);

            for (var i = 0; i < childUIs.Count; i++)
                childUIs[i].SuspendUI();

            if (beUseLua)
                CallLuaFunction(moduleName + ".SuspendUI");
            return true;
        }
        return false;
    }

    /// <summary>
    ///     恢复窗口，只改变位置不SetActive，需要时自动显示
    /// </summary>
    public virtual bool ResumeUI()
    {
        if (state == WndState.Suspend)
        {
            state = WndState.Opened;
            var v3 = transform.localPosition;
            v3.y -= 10000f;
            transform.localPosition = v3;
            CheckList(childUIs);
            for (var i = 0; i < childUIs.Count; i++)
                childUIs[i].ResumeUI();
            if (beUseLua)
                CallLuaFunction(moduleName + ".ResumeUI");
            return true;
        }

        return false;
    }

    public virtual void HideUI()
    {
        if (state != WndState.Hiden && Application.isPlaying)
        {
            gameObject.SetActive(false);
            state = WndState.Hiden;
            uiMgr.HideUI(this);
            //暂不改动位置
            if (beUseLua) CallLuaFunction(moduleName + ".HideUI");
        }
    }

    public virtual void Destroy()
    {
        //如果当前UI是在栈顶，就先隐藏
        if (state == WndState.Opened)
        {
            state = WndState.Hiden;
            uiMgr.HideUI(this);
        }

        //然后close
        if (state != WndState.Closed && Application.isPlaying)
        {
            if (beUseLua)
            {
                CallLuaFunction(moduleName + ".OnDestroy");
                luaState = null;
            }
            CloseAllChild();
            state = WndState.Closed;
            uiMgr.DestroyUI(this);
            if (OnUIClosed != null) OnUIClosed();
            if (LuaHelper.GetUIManager().GetCurPopUI() == this) LuaHelper.GetUIManager().GetCurPopUI();
            Destroy(gameObject);
        }

        
    }

    #endregion

    public void CloseAllChild()
    {
        CheckList(childUIs);
        while (childUIs.Count > 0)
            childUIs[0].Destroy();
        childStates.Clear();
    }


    #region 公用脚本
    private void CheckList<T>(T list) where T : IList, ICollection
    {
        var count = list.Count;
        for (var i = 0; i < count; i++)
            if (list[i] == null)
            {
                list.RemoveAt(i);
                i--;
                count--;
            }
    }

    public void ChangeUIType(UIType type)
    {
        this.type = type;
    }
    #endregion

    #region 层级管理

    protected virtual void AddToList()
    {
    }

    protected virtual void RemoveFromList()
    {
    }

    protected virtual void AddToList(LinkedList<UIBase> list, ref int depth)
    {
    }

    protected virtual void RemoveFromList(LinkedList<UIBase> list, ref int depth, int beginDepth)
    {
    }

    #endregion
}
