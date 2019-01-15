using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

internal static class CustomMenuItem
{
    [MenuItem("GameObject/Move/MoveUp &UP", false, 10)]
    public static void MoveUp()
    {
        Move(Vector3.up);
    }

    [MenuItem("GameObject/Move/MoveDown &DOWN", false, 10)]
    public static void MoveDown()
    {
        Move(Vector3.down);
    }

    [MenuItem("GameObject/Move/MoveLeft &LEFT", false, 10)]
    public static void MoveLeft()
    {
        Move(Vector3.left);
    }

    [MenuItem("GameObject/Move/MoveRight &RIGHT", false, 10)]
    public static void MoveRight()
    {
        Move(Vector3.right);
    }

    private static void Move(Vector3 v3)
    {
        foreach (var go in Selection.gameObjects) go.transform.localPosition += v3;
    }


    [MenuItem("GameObject/海外工具/检查子物体UILabel是否有中文", false, 10)]
    public static void CheckChinese()
    {
        foreach (var gameObject in Selection.gameObjects)
        {
            var uiLables = gameObject.GetComponentsInChildren<UILabel>();
            int count = 0;
            foreach (var item in uiLables)
                if (HasChinese(item.text))
                {
                    Debug.Log(item.gameObject.name + "\t" + item.text);
                    count++;
                }

            Debug.Log("有" + count + "UILabel带有中文");
        }
    }


    [MenuItem("GameObject/海外工具/把UILabel赋值为空字符", false, 10)]
    public static void ClearUILabel()
    {
        foreach (var gameObject in Selection.gameObjects)
        {
            var uiLables = gameObject.GetComponentsInChildren<UILabel>();
            foreach (var item in uiLables)
                if (HasChinese(item.text))
                    item.text = String.Empty;
            ;
        }
    }


    public static bool HasChinese(string str)
    {
        return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
    }
}