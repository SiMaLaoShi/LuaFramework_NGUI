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
    /// <summary> UI锚点 </summary>
    public Anchor anchor = Anchor.Center;
    /// <summary> 关闭ui的时候是否销毁 </summary>
    public bool bdestory = true; 

    protected bool beSetLuaData;
    protected bool beStart; //是否调用的Start
    public bool beUnique = true; //是否是独有的
    public bool beUseLua = true; //是否使用lua
    public bool bMainUI = false; //是否是主UI

    public bool isFullUI = false;

    private LuaFunction func = null;

    public List<bool> childStates = new List<bool>();
    public List<SubUIBase> childUIs = new List<SubUIBase>(); //子UI列表

    protected List<LuaBaseRef> funcList = new List<LuaBaseRef>(); //自身的所有方法

    public bool isHiedUI = false;
    protected LuaState luaState;
    protected string moduleName = string.Empty; //lua中的model名字

    protected Transform myTrans;

    public Action OnUIClosed = delegate { }; //关闭时的委托

    
    public SubUIBase parentUI; //父UI

    protected LuaTable self; //自身
    [SerializeField]
    protected WndState state = WndState.Init; //窗口状态
    public UIType type = UIType.SubWnd; //UI类型
    private bool hideState = false;//保存shortHide 之前状态
    private Vector3 initTrans;

    public LuaTable table
    {
        get { return self; }
    }

    public WndState State
    {
        get { return state; }
        set { state = value; }
    }

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

    protected virtual void CallLuaFunction(string name)
    {
        if (beUseLua == false) return;
        if (luaState == null || LuaHelper.GetLuaManager() == null) return;
        luaState.Call(name, false);
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

    #region SubUIBase 的生命周期
    public virtual void ShowUI()
    {
        if (state == WndState.Opened)
            return;
        gameObject.SetActive(true);
        state = WndState.Opened;
        uiMgr.ShowUI(this);
        //暂不改动位置
        //transform.localPosition = Vector3.zero;
        if (parentUI != null)
            if (parentUI.state == WndState.Suspend)
                SuspendUI();
        if (beUseLua) CallLuaFunction(moduleName + ".ShowUI");
    }
    /// <summary>
    ///     挂起窗口，只改变位置不SetActive，需要时自动显示
    /// </summary>
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

    public void CloseAllChild()
    {
        CheckList(childUIs);
        while (childUIs.Count > 0)
            childUIs[0].Destroy();
        childStates.Clear();
    }


    #endregion

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

    #region LuaFunction 操作

    public void AddLuaFunList(LuaFunction func)
    {
        funcList.Add(func);
    }

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
