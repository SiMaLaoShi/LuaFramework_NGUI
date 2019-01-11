using System;
using UnityEngine;
using System.Collections.Generic;
using LuaInterface;

[RequireComponent(typeof(UIPanel))]
public class UIBase : SubUIBase
{
    private const int delta = 2;

    private const string maskPath = "";

    protected static LinkedList<UIBase> uiList = new LinkedList<UIBase>();
    protected static LinkedList<UIBase> noDepthList = new LinkedList<UIBase>();
    protected static int maxDepth2D = 1;

    public bool beAutoClose { get; set; }
    public bool beKeepDepth { get; set; }
    public bool beMaskWin { get; set; }

    protected Transform maskUI;

    protected UIPanel panel;
    protected UIPanel[] panels;

    private Transform parent;

    protected override void Init()
    {
        myTrans = transform;
        panel = GetComponent<UIPanel>();
        Attach();
        if (!beKeepDepth)
        {
            panels = gameObject.GetComponentsInChildren<UIPanel>(true);
            Array.Sort(panels, (p1, p2) => { return p1.depth - p2.depth; });
            AddToList();
        }
        else
            noDepthList.AddLast(this);


    }

    public override void SetAutoClose()
    {
        base.SetAutoClose();
    }

    #region Mono的生命周期 

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }


    #endregion

    #region 窗口的生命周期

    public static void DestroyAll()
    {
        foreach (UIBase uiBase in uiList)
        {
            uiBase.Destroy();
        }

        foreach (UIBase uiBase in noDepthList)
        {
            uiBase.Destroy();
        }
        uiList.Clear();
        noDepthList.Clear();
    }

    #endregion

    public override void Destroy()
    {
        base.Destroy();
    }

    protected override void AddToList()
    {
        AddToList(uiList, ref maxDepth2D);
    }

    protected override void RemoveFromList()
    {
        base.RemoveFromList();
    }

    protected override void AddToList(LinkedList<UIBase> list, ref int depth)
    {
        if (beKeepDepth)
        {
            return;
        }

        if (list.Count > 0)
        {
#if UNITY_EDITOR
            if (list.Find(this) != null)
            {
                Debugger.LogError("ui {0} already in ui List",name);
                return;
            }
#endif
        }

        depth = SetDepth(depth) + delta;
        list.AddLast(this);
    }

    protected override void RemoveFromList(LinkedList<UIBase> list, ref int depth, int beginDepth)
    {
        if (beKeepDepth || list.Count == 0)
        {
            noDepthList.Remove(this);
            return;
        }

        list.Remove(this);
        depth = beginDepth;
        foreach (var ui in list) depth = ui.SetDepth(depth) + delta;
    }

    private int SetDepth(int value)
    {
        var baseLine = panels[0].depth;
        value -= baseLine;
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].depth += value;
        }

        return panels[panels.Length - 1].depth;
    }

    public void AddSubWindow(UIBase ui)
    {
        if (ui.type == UIType.SubWnd)
        {
            childUIs.Add(ui);
            childStates.Add(ui.gameObject.activeSelf);
        }
    }

    public void RemoveSubWindow(UIBase ui)
    {
        if (ui.type == UIType.SubWnd)
        {
            childUIs.Remove(ui);
            childStates.Remove(ui.gameObject.activeSelf);
        }
    }

    #region UIEventListener

    public void AddOnClickListener(Transform node, LuaFunction func, int data)
    {
        var listener = AddEventListener(node, data);
        func.name = string.Format("{0}.{1}.OnClick", moduleName, node.name);
        //listener.onClick += delegate(GameObject go, int parameter) { LuaHelper.GetSoundManager().PlayAudio("Prefabs/Sound/effect/an_tongyong"); };
        listener.onClick =
            (LuaUIEventListener.VoidDelegate)DelegateTraits<LuaUIEventListener.VoidDelegate>.Create(func);
    }

    public void AddOnClickListener(Transform node, LuaFunction func, LuaTable self, int data)
    {
        var listener = AddEventListener(node, data);
        func.name = string.Format("{0}.{1}.OnClick", moduleName, node.name);
        listener.onClick =
            (LuaUIEventListener.VoidDelegate)DelegateTraits<LuaUIEventListener.VoidDelegate>.Create(func, self);
    }

    public void AddOnPressListener(Transform node, LuaFunction func, int data)
    {
        var listener = AddEventListener(node, data);
        func.name = string.Format("{0}.{1}.OnPress", moduleName, node.name);
        listener.onPress =
            (LuaUIEventListener.BoolDelegate)DelegateTraits<LuaUIEventListener.BoolDelegate>.Create(func);
    }

    public void AddOnPressListener(Transform node, LuaFunction func, LuaTable self, int data)
    {
        var listener = AddEventListener(node, data);
        func.name = string.Format("{0}.{1}.OnPress", moduleName, node.name);
        listener.onPress =
            (LuaUIEventListener.BoolDelegate)DelegateTraits<LuaUIEventListener.BoolDelegate>.Create(func, self);
    }

    public void AddOnDragListener(Transform node, LuaFunction func, int data)
    {
        var listener = AddEventListener(node, data);
        func.name = string.Format("{0}.{1}.OnDrag", moduleName, node.name);
        listener.onDrag =
            (LuaUIEventListener.VectorDelegate)DelegateTraits<LuaUIEventListener.VectorDelegate>.Create(func);
    }

    public void AddOnDragListener(Transform node, LuaFunction func, LuaTable self, int data)
    {
        var listener = AddEventListener(node, data);
        func.name = string.Format("{0}.{1}.OnDrag", moduleName, node.name);
        listener.onDrag =
            (LuaUIEventListener.VectorDelegate)DelegateTraits<LuaUIEventListener.VectorDelegate>.Create(func, self);
    }

    private LuaUIEventListener AddEventListener(Transform node, int data)
    {
        var listener = LuaUIEventListener.Get(node.gameObject);
        listener.data = data;
        return listener;
    }

    #endregion
}
