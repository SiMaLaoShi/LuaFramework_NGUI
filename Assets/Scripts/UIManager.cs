using System.Collections;
using System.Collections.Generic;
using LuaInterface;
using LuaFramework;
using UnityEngine;

public class UIManager : Manager
{
    private GameObject Root;
    private Camera uicam;
    private GameObject uiRoot;

    public GameObject MainUIRoot { get; private set; }
    public SubUIBase CurrentUI { get; private set; }

    public void InitUIManager(GameObject uiRoot)
    {
        Debug.Log("init uimanager");
        Root = uiRoot;
        this.uiRoot = uiRoot.transform.Find("GUI Camera/Root").gameObject;
        MainUIRoot = uiRoot.transform.Find("GUI Camera/MainUIRoot").gameObject;
        UIHelper.SetUIRoot(uiRoot.transform);
    }

    public Camera GetUICamera()
    {
        if (uicam == null)
            uicam = GameObject.FindGameObjectWithTag(AppConst.GUICAMERA).GetComponent<Camera>();

        return uicam;
    }

    [NoToLua]
    //供C# 调用
    public T CreateUI<T>(string strPrefab, bool local = false) where T : SubUIBase
    {
        var obj = LuaFramework.LuaHelper.GetResManager().LoadLocalGameObject(strPrefab);
        SubUIBase uiBase = UnGfx.GetSafeComponent<T>(obj);
        return uiBase as T;
    }

    #region 创建UI

    public SubUIBase CreateUISync(string npath)
    {
        return CreateUISync(npath, false, null, 0);
    }

    public SubUIBase CreateUISync(string npath, bool bpopUI)
    {
        return CreateUISync(npath, bpopUI, null, 0);
    }

    public SubUIBase CreateUISync(string npath, bool bpopUI, SubUIBase parent)
    {
        return CreateUISync(npath, bpopUI, parent, 0);
    }

    public void UpdateUIBaseType(SubUIBase uiBase, int type)
    {
        if (uiBase == null)
        {
            Debug.Log("uiBase为空");
            return;
        }

        switch (type)
        {
            case 1:
                uiBase.ChangeUIType(UIType.MainWnd);
                break;
            case 2:
                uiBase.ChangeUIType(UIType.SubWnd);
                break;
            case 3:
                uiBase.ChangeUIType(UIType.Modal);
                break;
            case 4:
                uiBase.ChangeUIType(UIType.Tips);
                break;
            case 5:
                uiBase.ChangeUIType(UIType.SingleTips);
                break;
            case 6:
                uiBase.ChangeUIType(UIType.TopTips);
                break;
            case 7:
                uiBase.ChangeUIType(UIType.UIItem);
                break;
            default:
                break;
        }
    }

    /// <summary>
    ///     同步创建一个UI
    /// </summary>
    /// <param name="npath"></param>
    /// <param name="bpopUI">是否关闭上一个UI</param>
    /// <param name="parentUI">是否附属于父UI，</param>
    /// <returns></returns>
    public SubUIBase CreateUISync(string npath, bool bpopUI, SubUIBase parentUI, int nType )
    {
        var name = GetFileName(npath);
        SubUIBase ui = null;
        //如果当前UIList存在当前界面，就重新初始化
        for (var i = 0; i < UIList.Count; i++)
        {
            ui = UIList[i];
            if (UIList[i] != null && UIList[i].name == name)
                if (ui.type != UIType.UIItem)
                {
                    InitUIByType(ui, parentUI, bpopUI);
                    return ui;
                }
        }


        //如果当前字典中存在当前UI就重新初始化
        if (hidenUIDic.ContainsKey(name))
        {
            ui = hidenUIDic[name];
            if (ui != null)
                if (ui.type != UIType.UIItem)
                {
                    InitUIByType(ui, parentUI, bpopUI);
                    return ui;
                }
        }

        //如果当前内存中不存在该UI就重新创建
        var assest = ResManager.LoadResourcesSync(npath);
        if (assest)
        {
            var obj = UnGfx.Instantiate(assest);
            ui = UnGfx.GetSafeComponent<SubUIBase>(obj);
            UpdateUIBaseType(ui, nType);
            InitUIByType(ui, parentUI, bpopUI);
            return ui;
        }
        return null;
    }

    public void CreateUI(string npath, LuaFunction func)
    {
        CreateUI(npath, func, false, null);
    }

    public void CreateUI(string npath, LuaFunction func, ResourceManager.Priority priority)
    {
        CreateUI(npath, func, false, null, priority);
    }

    public void CreateUI(string npath, LuaFunction func, bool bpopUI)
    {
        CreateUI(npath, func, bpopUI, null);
    }

    public void CreateUI(string npath, LuaFunction func, bool bpopUI, ResourceManager.Priority priority)
    {
        CreateUI(npath, func, bpopUI, null, priority);
    }

    /// <summary>
    ///     异步创建UI
    /// </summary>
    /// <param name="npath">路径</param>
    /// <param name="func">创建完成的回调</param>
    /// <param name="bpopUI">上一个UI</param>
    /// <param name="parentUI">父UI</param>
    public void CreateUI(string npath, LuaFunction func, bool bpopUI, SubUIBase parentUI)
    {
        CreateUI(npath, func, bpopUI, parentUI, ResourceManager.Priority.non);
    }

    /// <summary>
    ///     异步创建UI，并且设置异步加载的优先级，不设置时priority值保持为ResourceManager.Priority.non
    /// </summary>
    /// <param name="npath">路径</param>
    /// <param name="func">创建完成的回调</param>
    /// <param name="bpopUI">上一个UI</param>
    /// <param name="parentUI">父UI</param>
    /// <param name="priority">优先级</param>
    public void CreateUI(string npath, LuaFunction func, bool bpopUI, SubUIBase parentUI,
        ResourceManager.Priority priority)
    {
        var name = GetFileName(npath);
        SubUIBase ui = null;
        for (var i = 0; i < UIList.Count; i++)
        {
            ui = UIList[i];
            if (UIList[i] != null && UIList[i].name == name)
            {
                InitUIByType(ui, parentUI, bpopUI);
                if (func != null)
                {
                    func.Call(ui);
                    ui.AddLuaFunList(func);
                }

                return;
            }
        }

        if (hidenUIDic.ContainsKey(name))
        {
            ui = hidenUIDic[name];
            if (ui != null)
            {
                InitUIByType(ui, parentUI, bpopUI);
                if (func != null)
                {
                    func.Call(ui);
                    ui.AddLuaFunList(func);
                }

                return;
            }
        }

        ResourceManager.LoadResCallback callBack = assest => { CreateUIImpl(assest, func, bpopUI, parentUI); };
        ResManager.LoadResourceAsync(npath, callBack, priority);
    }

    private void CreateUIImpl(Object assest, LuaFunction func, bool bpopUI = false, SubUIBase parentUI = null)
    {
        if (assest == null)
            return;

        var obj = UnGfx.Instantiate(assest);

        var ui = UnGfx.GetSafeComponent<SubUIBase>(obj);
        InitUIByType(ui, parentUI, bpopUI);
        if (func != null)
        {
            func.Call(ui);
            ui.AddLuaFunList(func);
        }
    }

    #endregion

    #region Manager

    //列表内控制显示逻辑，具有堆栈的效果
    private readonly List<SubUIBase> UIList = new List<SubUIBase>();
    private readonly List<SubUIBase> SubWndList = new List<SubUIBase>();

    private readonly List<UIBase> MainWndList = new List<UIBase>();
    private readonly List<bool> MainWndStates = new List<bool>();
    private readonly Dictionary<string, bool> MainWndStateInitDic = new Dictionary<string, bool>();

    private readonly List<UIBase> SingleList = new List<UIBase>();
    private readonly List<SubUIBase> TopTipsList = new List<SubUIBase>();
    private readonly List<SubUIBase> TipsList = new List<SubUIBase>();
    private readonly List<SubUIBase> SingleTipList = new List<SubUIBase>();
    private readonly List<SubUIBase> firstLevelDic = new List<SubUIBase>();

    /// <summary>
    ///     保存隐藏的窗口，需要时重新打开
    /// </summary>
    private readonly Dictionary<string, SubUIBase> hidenUIDic = new Dictionary<string, SubUIBase>();
    private readonly List<SubUIBase> suspendUIList = new List<SubUIBase>();

    //上一个UI
    private SubUIBase popUI;

    /// <summary>
    ///     只负责将相对应的窗口装入list中
    /// </summary>
    /// <param name="uiBase"></param>
    /// <param name="parentUI"></param>
    /// <param name="bpopUI"></param>
    [NoToLua]
    private void InitUIByType(SubUIBase uiBase, SubUIBase parentUI = null, bool bpopUI = false)
    {
        if (uiBase.bMainUI) return;

        if (parentUI != null)
        {
            if (parentUI.childUIs.Contains(uiBase))
            {
                uiBase.ShowUI();
                return;
            }
            uiBase.parentUI = parentUI;
            parentUI.childUIs.Add(uiBase);
            parentUI.childStates.Add(uiBase.gameObject.activeSelf);
        }

        if (uiBase.isOneLvUI)
        {
            if (firstLevelDic.Contains(uiBase))
                Debug.LogWarning("同时打开一个一级界面这是一个错误的操作\tname:" + uiBase.name);
            else
                firstLevelDic.Add(uiBase);
        }

        if (bpopUI)
        {
            if (uiBase.isHiedUI)
            {
                for (var i = 0; i < UIList.Count; i++)
                    if (UIList[i] != null)
                    {
                        if (UIList[i].gameObject.GetComponent<TweenPosition>())
                        {
                            
                        }
                        else
                        {
                            if (UIList[i].SuspendUI())
                            {
                                suspendUIList.Add(UIList[i]);
                            }
                        }
                    }
            }
            else
            {
                if (popUI != null && popUI.type != UIType.TopTips)
                {
                    popUI.Close();
                    popUI = uiBase;
                }
            }
            if (popUI == null) popUI = uiBase;
        }

        CheckList(UIList);
        //如果UIList存在当前UI，直接显示当前UI
        if (UIList.Contains(uiBase))
        {
            ShowUI(uiBase);
            return;
        }

        UIList.Add(uiBase);
        switch (uiBase.type)
        {
            case UIType.MainWnd:
                //保持状态同步
                for (var i = MainWndList.Count - 1; i >= 0; i--)
                    if (MainWndList[i] == null)
                        MainWndStates.RemoveAt(i);

                CheckList(MainWndList);
                //如果是全屏窗口，发生提权，同时关闭主界面
                if (uiBase.isFullUI)
                {
                    uiBase.type = UIType.Modal;
                    CheckList(SingleList);
                    if (uiBase is UIBase)
                    {
                        SingleList.Add(uiBase as UIBase);
                    }
                    else
                    {
                        uiBase.type = UIType.SubWnd;
                        SubWndList.Add(uiBase);
                    }
                }
                else
                {
                    //如果不是uibase则发生退化，逻辑同subui
                    if (uiBase is UIBase)
                    {
                        MainWndList.Add(uiBase as UIBase);
                        MainWndStates.Add(true);
                    }
                    else
                    {
                        uiBase.type = UIType.SubWnd;
                        SubWndList.Add(uiBase);
                    }
                }

                break;
            case UIType.SubWnd:
                /*if (uiBase.parentUI == null)
                {
                    uiBase.type = UIType.Tips;
                    CheckList(TipsList);
                    if(uiBase is UIBase)
                        TipsList.Add(uiBase as UIBase);
                }
                else
                {
                    SubWndList.Add(uiBase);
                }*/
                SubWndList.Add(uiBase);
                break;
            case UIType.Modal:
                CheckList(SingleList);
                //single与main的区别在于single优先级高于main，且默认是全屏的，会关闭主界面
                if (uiBase is UIBase)
                    SingleList.Add(uiBase as UIBase);
                else
                    SubWndList.Add(uiBase);
                break;
            case UIType.Tips:
                //Tips为悬浮在其他窗口上方的通知性提示，无互斥
                CheckList(TipsList);
                TipsList.Add(uiBase);
                break;
            case UIType.SingleTips:
                CheckList(SingleTipList);
                SingleTipList.Add(uiBase);
                break;
            case UIType.TopTips:
                //出现时关闭其他所有窗口
                CheckList(TopTipsList);
                TopTipsList.Add(uiBase);
                break;
            case UIType.UIItem:
                break;
            default:
                break;
        }

        ShowUI(uiBase);
        //Debug.Log("ShowUI:" + uiBase.name);
    }

    #region 一级界面的操作

    public void RemoveFirstLevel(SubUIBase uiBase)
    {
        if (firstLevelDic.Contains(uiBase)) firstLevelDic.Remove(uiBase);
    }

    public int GetFirstLevelCount()
    {
        return firstLevelDic.Count;
    }

    #endregion


    /// <summary>
    ///     设置窗口的depth
    /// </summary>
    private void ConfigDepth()
    {
    }

    public void ShowUI(SubUIBase uiBase)
    {
        //Debug.Log("name" + uiBase.gameObject.name);
        if (uiBase.State == SubUIBase.WndState.Opened) return;

        if (uiBase.bMainUI) return;

        uiBase.ShowUI();
        CurrentUI = uiBase;
        switch (uiBase.type)
        {
            case UIType.MainWnd:
                uiBase.childStates.Clear();
                for (var i = 0; i < uiBase.childUIs.Count; i++)
                {
                    uiBase.childUIs[i].ShowUI();
                    uiBase.childStates.Add(true);
                }

                break;
            case UIType.SubWnd:
                break;
            case UIType.Modal:
                for (var i = 0; i < SingleList.Count - 1; i++)
                    //挂起其他窗口
                    SingleList[i].SuspendUI();
                //Debug.Log("UIName" + uiBase.name);
                HideMainUI();
                MainWndStates.Clear();
                foreach (var mainWnd in MainWndList)
                {
                    if (MainWndStateInitDic.ContainsKey(mainWnd.name))
                    {
                        MainWndStates.Add(MainWndStateInitDic.ContainsKey(mainWnd.name));
                    }
                    else
                    {
                        MainWndStates.Add(mainWnd.State == SubUIBase.WndState.Opened);
                        MainWndStateInitDic.Add(mainWnd.name, true);
                    }

                    mainWnd.HideUI();
                }

                break;
            case UIType.Tips:
                var panels = uiBase.GetComponentsInChildren<UIPanel>(true);
                for (var i = 0; i < panels.Length; i++) panels[i].sortingOrder = 2;
                break;
            case UIType.SingleTips:
                //独占式通知
                if (SingleTipList.Count > 0)
                    SingleTipList[SingleTipList.Count - 1].HideUI();
                HideMainUI();
                break;
            case UIType.TopTips:
                /*if (MainWndList.Count > 0)
                    MainWndList[MainWndList.Count - 1].HideUI();
                if (SingleList.Count > 0)
                    SingleList[SingleList.Count - 1].HideUI();
                if (SingleTipList.Count > 0)
                    SingleTipList[SingleTipList.Count - 1].HideUI();
                for (var i = 0; i < TipsList.Count; i++) TipsList[i].HideUI();*/

                uiBase.GetComponent<UIPanel>().depth = 2000 + TopTipsList.Count * 5;
                uiBase.GetComponent<UIPanel>().sortingOrder = 3;
                break;
            case UIType.UIItem:
                break;
            default:
                break;
        }
    }

    public void HideUI(SubUIBase uiBase)
    {
        //currentUI = null;
        if (uiBase.State != SubUIBase.WndState.Hiden)
        {
            uiBase.HideUI();
            return;
        }
        if (uiBase.bMainUI) return;

        hidenUIDic[uiBase.name] = uiBase;
        switch (uiBase.type)
        {
            case UIType.MainWnd:
                uiBase.childStates.Clear();
                for (var i = 0; i < uiBase.childUIs.Count; i++)
                {
                    //控制自己的子UI
                    uiBase.childStates.Add(uiBase.childUIs[i].gameObject.activeSelf);
                    uiBase.childUIs[i].HideUI();
                }

                break;
            case UIType.SubWnd:

                //uiBase.childStates.Clear();
                //for (int i = 0; i < uiBase.childUIs.Count; i++)
                //{
                //    uiBase.childStates.Add(uiBase.childUIs[i].gameObject.activeSelf);
                //    uiBase.childUIs[i].HideUI();
                //}

                break;
            case UIType.Modal:
                uiBase.childStates.Clear();
                for (var i = 0; i < uiBase.childUIs.Count; i++)
                {
                    uiBase.childStates.Add(uiBase.childUIs[i].gameObject.activeSelf);
                    uiBase.childUIs[i].HideUI();
                }

                CheckList(SingleList);
                SubUIBase nextUI = null;
                for (var i = 0; i < SingleList.Count; i++)
                    if (SingleList[i].State == SubUIBase.WndState.Opened ||
                        SingleList[i].State == SubUIBase.WndState.Suspend)
                        nextUI = SingleList[i];

                if (!nextUI)
                {
                    ShowMainUI();
                    //恢复主窗口状态
                    for (var i = 0; i < MainWndList.Count; i++)
                    {
                        MainWndList[i].gameObject.SetActive(MainWndStates[i]);
                        MainWndList[i].State = MainWndStates[i] ? SubUIBase.WndState.Opened : SubUIBase.WndState.Hiden;
                        MainWndStateInitDic.Remove(MainWndList[i].name);
                        foreach (var ui in MainWndList[i].childUIs)
                        {
                            ui.ShowUI();
                        }
                    }
                }
                else
                {
                    HideMainUI();
                    nextUI.ResumeUI();
                }

                break;
            case UIType.Tips:
                //Tips为悬浮在其他窗口上方的通知性提示，无互斥
                CheckList(TipsList);
                TipsList.Remove(uiBase);
                break;
            case UIType.SingleTips:
                CheckList(SingleTipList);
                SingleTipList.Remove(uiBase);
                break;
            case UIType.TopTips:
                //出现时关闭其他所有窗口
                CheckList(TopTipsList);
                TopTipsList.Remove(uiBase);
                break;
            case UIType.UIItem:
                break;
            default:
                break;
        }
    }

    public void DestroyUI(SubUIBase uiBase)
    {
        if (uiBase.bMainUI) return;

        if (uiBase.gameObject.activeSelf) uiBase.HideUI();

        if (uiBase.State != SubUIBase.WndState.Closed)
        {
            uiBase.Destroy();
            return;
        }
        //走c#调用里面UIList 不会添加
        if (uiBase.bdestory == false)
            hidenUIDic[uiBase.name] = uiBase;
        else if (hidenUIDic.ContainsKey(uiBase.name)) hidenUIDic.Remove(uiBase.name);

        if (!UIList.Contains(uiBase)) return;
        UIList.Remove(uiBase);

        if (uiBase.isHiedUI)
            for (var i = 0; i < UIList.Count; i++)
            {
                if (UIList[i].gameObject.GetComponent<TweenPosition>())
                {
                }
                else
                {
                    for (int j = 0; j < suspendUIList.Count; j++)
                    {
                        suspendUIList[j].ResumeUI();
                        suspendUIList.RemoveAt(j);
                    }
                }

            }

        switch (uiBase.type)
        {
            case UIType.MainWnd:
                //保持状态同步
                var count = MainWndList.Count;
                for (var i = 0; i < count; i++)
                    if (MainWndList[i] == null)
                    {
                        MainWndStates.RemoveAt(i);
                        MainWndList.RemoveAt(i);
                        i--;
                        count--;
                    }

                var state = MainWndStateInitDic.Remove(uiBase.name);
                if (!state) Debug.LogWarning("MainWndStateInitDic没有这个键" + uiBase.name);
                MainWndStates.RemoveAt(MainWndList.IndexOf(uiBase as UIBase));
                MainWndList.Remove(uiBase as UIBase);
                break;
            case UIType.SubWnd:
                //SubWndList.Add(uiBase);
                SubWndList.Remove(uiBase);

                if (uiBase.parentUI != null)
                {
                    var childUIs = uiBase.parentUI.childUIs;
                    CheckList(childUIs);
                    var childUIStates = uiBase.parentUI.childStates;
                    childUIStates.RemoveAt(childUIs.IndexOf(uiBase));
                    childUIs.Remove(uiBase);
                }

                break;
            case UIType.Modal:
                CheckList(MainWndList);
                SingleList.Remove(uiBase as UIBase);

                break;
            case UIType.Tips: //2
                CheckList(TipsList);
                TipsList.Remove(uiBase as UIBase);
                break;
            case UIType.SingleTips: //4
                CheckList(SingleList);
                SingleTipList.Remove(uiBase as UIBase);
                break;
            case UIType.TopTips: //6
                /*if (MainWndList.Count > 0)
                    MainWndList[MainWndList.Count - 1].ShowUI();
                if (SingleList.Count > 0)
                    SingleList[SingleList.Count - 1].ShowUI();
                if (SingleTipList.Count > 0)
                    SingleTipList[SingleTipList.Count - 1].ShowUI();
                for (var i = 0; i < TipsList.Count; i++) TipsList[i].ShowUI();*/
                break;
            case UIType.UIItem:
                break;
            default:
                break;
        }
    }

    /// <summary>
    ///     切换场景调用
    /// </summary>
    public void CloseAllUI()
    {
        var count = 0;
        CheckList(UIList);
        while (UIList.Count > count)
            if (UIList[count].parentUI == null && UIList[count].bMainUI == false && UIList[count].bdestory)
                UIList[count].Destroy();
            else
                count++;

        CheckList(UIList);
    }

    /// <summary>
    ///     AR相机调用
    /// </summary>
    public void CloseAllTipsList()
    {
        var count = 0;
        CheckList(TipsList);
        while (TipsList.Count > count)
            if (TipsList[count].bMainUI == false && TipsList[count].bdestory && TipsList[count].type == UIType.Tips)
                TipsList[count].Destroy();
            else
                count++;
    }

    /// <summary>
    ///     退出游戏时调用
    /// </summary>
    public void ClearAllUI()
    {
        var count = UIList.Count;
        //for (int i = 0; i < count; i++)
        //{
        //    SubUIBase ui = UIList[i];
        //    ui.Destroy();
        //    i = i - (count - UIList.Count);
        //    if (i < -1) i = -1;
        //    count = UIList.Count;
        //}
        while (UIList.Count > 0) UIList[0].Destroy();

        foreach (var item in hidenUIDic)
            if (item.Value)
                item.Value.Destroy();

        SubWndList.Clear();
        MainWndList.Clear();
        MainWndStates.Clear();
        SingleList.Clear();
        TopTipsList.Clear();
        TipsList.Clear();
        SingleTipList.Clear();
        hidenUIDic.Clear();
        firstLevelDic.Clear();
        suspendUIList.Clear();
        popUI = null;
    }

    public SubUIBase FindUIByName(string name)
    {
        for (var i = 0; i < UIList.Count; i++)
            if (UIList[i] != null && UIList[i].name == name)
                return UIList[i];

        return null;
    }

    public bool GetWndIsOpen()
    {
        //Debug.Log("singelist:" + SingleList.Count + "\nSubWndList:" + SubWndList.Count);
        return SingleList.Count > 0;
    }

    public SubUIBase GetCurPopUI()
    {
        return popUI;
    }

    public void SetCurPopUI(UIBase ui)
    {
        popUI = ui;
    }

    private LuaFunction func = null;

    private void HideMainUI()
    {
        MainUIRoot.transform.position = new Vector3(0f, 0f, 100000f);
        //LuaHelper.BPCallFunction("UIManager", "MonitorMainUIChange", 0);
    }

    private void ShowMainUI()
    {
        MainUIRoot.transform.position = new Vector3(0f, 0f, 0f);
        //LuaHelper.BPCallFunction("UIManager", "MonitorMainUIChange", 1);
    }

    #endregion

    #region privateMethod

    private string GetFileName(string npath)
    {
        var start = npath.LastIndexOf('/');
        var end = npath.LastIndexOf('.');
        if (end == -1)
            end = npath.Length;
        end = end - start - 1;
        if (end <= 0) return "";

        var name = npath.Substring(start + 1, end);
        return name;
    }

    /// <summary>
    ///     剔除list中的空对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
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

    #endregion
}