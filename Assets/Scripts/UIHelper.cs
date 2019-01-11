using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using LuaFramework;
using LuaInterface;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIHelper
{
    public static Transform mainUIRoot, uiRoot, uiRootPatent;

    public static void SetUIRoot(Transform parent)
    {
        uiRoot = UnGfx.FindNode(parent, "Root");
        mainUIRoot = UnGfx.FindNode(parent, "MainUIRoot");
        uiRootPatent = parent;
    }

    public static bool Delay(int delayTime)
    {
        DateTime now = DateTime.Now;
        int s;
        do
        {
            TimeSpan spand = DateTime.Now - now;
            s = spand.Seconds;
        }
        while (s < delayTime);
        return true;
    }

    public static Dictionary<string, Transform> GetAllBipTF(Transform tf)
    {
        Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
        Transform[] componentsInChildren = tf.GetComponentsInChildren<Transform>();
        for (int index = 0; index < componentsInChildren.Length; ++index)
            dictionary[componentsInChildren[index].name] = componentsInChildren[index];
        return null;
    }

    public static void DynamicSprite(UISprite sprite, string atlasResPath, string spriteName)
    {
        UIAtlas atlas = null;
        GameObject altasGo = null;
        //altasGo = LuaHelper.GetResManager().LoadResourcesSyncByType(atlasResPath, typeof(GameObject)) as GameObject;
        atlas = altasGo.GetComponent<UIAtlas>();
        sprite.atlas = atlas;
        sprite.spriteName = spriteName;
    }

    public static void SetUIWidgetGrey(UIWidget ui, bool grey, string color)
    {
        byte alpha = (byte)Mathf.Ceil(ui.color.a * 255);
        if (grey)
            ui.color = new Color32(1, 1, 1, alpha);
        else
        {
            if (color != null)
            {
                char[] separator = new char[] { ',' };
                string[] colorArr = color.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                ui.color = new Color32(byte.Parse(colorArr[0]), byte.Parse(colorArr[1]), byte.Parse(colorArr[2]), alpha);
            }
            else
                ui.color = new Color32(255, 255, 255, alpha);
        }
    }
    public static void SetUIWidgetGrey(UIWidget ui, bool grey)
    {
        SetUIWidgetGrey(ui, grey, null);
    }

    public static void ShowPopEffect(GameObject gameObject)
    {
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 3);
        TweenRotation.Begin(gameObject, 0.2f, Quaternion.Euler(0, 0, 0));
        gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        TweenScale tweenScale = TweenScale.Begin(gameObject, 0.2f, new Vector3(1.03f, 1.03f, 1.03f));
        tweenScale.SetOnFinished(delegate ()
        {
            TweenScale.Begin(gameObject, 0.1f, new Vector3(1f, 1f, 1f));
        });
    }

    public static void AddInputChangeListener(UIInput input, LuaFunction func, LuaTable self)
    {
        if (!input)
        {
            return;
        }
        EventDelegate.Callback dele = ((EventDelegate.Callback)DelegateFactory.CreateDelegate(typeof(EventDelegate.Callback), func, self));
        EventDelegate.Add(input.onChange, dele);
    }

}
