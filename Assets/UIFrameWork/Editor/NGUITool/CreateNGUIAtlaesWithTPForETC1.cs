using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class CreateNGUIAtlaesWithTPForETC1
{
    [MenuItem("UIFrameWork/批量替换图集的材质为透明通道分离")]
    static void ReplaceAtlasRGBA()
    {

        string[] uids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/Resources/UI/Atlas_New" });
        //string[] uids = Selection.assetGUIDs;
        for (int i = 0; i < uids.Length; i++)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(uids[i]));
            UIAtlas atlas = go.GetComponent<UIAtlas>();
            if (atlas != null)
            {
                Material material = atlas.spriteMaterial;
                Texture texture = material.GetTexture("_MainTex");

                material.shader = Shader.Find("Unlit/Atlas/Transparent Colored (TextureClip)");
                var path = AssetDatabase.GetAssetPath(texture);
                if (texture != null && Path.GetExtension(path) == ".png" && !path.Contains("_Alpha") && !path.Contains("_RGB"))
                {
                    SetTextureReadable(path);
                    SeperateRGBandAlphaChannel(path);

                    TextureSetting(path, TextureImporterType.Advanced, TextureImporterFormat.ETC_RGB4, false);
                    Texture2D _mainTex = AssetDatabase.LoadAssetAtPath(path.Replace(".png", "_RGB.png"), typeof(Texture2D)) as Texture2D;
                    Texture2D _alphaTex = AssetDatabase.LoadAssetAtPath(path.Replace(".png", "_Alpha.png"), typeof(Texture2D)) as Texture2D;

                    material.SetTexture("_MainTex", _mainTex);
                    material.SetTexture("_AlphaTex", _alphaTex);
                }

            }
            EditorUtility.DisplayProgressBar("批量分离替换中", AssetDatabase.GUIDToAssetPath(uids[i]),
                (float) i / (uids.Length));
        }
        EditorUtility.ClearProgressBar();

    }

    [MenuItem("Assets/替换图集的贴图的压缩算法为Dither444")]
    static void ReplaceAtlasTexture()
    {

        string[] uids = AssetDatabase.FindAssets("t:Prefab",
            new string[] {AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0])});
        for (int i = 0; i < uids.Length; i++)
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(uids[i]));
            UIAtlas atlas = go.GetComponent<UIAtlas>();
            if (atlas != null)
            {
                Material material = atlas.spriteMaterial;
                Texture texture = material.GetTexture("_MainTex");

                var path = AssetDatabase.GetAssetPath(texture);
                Debug.Log("path " + path);
                if(path == null || path.Contains("_dither"))
                    continue;
                var target = path.Replace(".png", "_dither.png");
                path = Environment.CurrentDirectory + "/" + path;
                var temp = Environment.CurrentDirectory + "/" + target;
                Debug.Log("temp " + temp);
                File.Copy(path, temp, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("target " + target);
                var importer = AssetImporter.GetAtPath(target) as TextureImporter;
                importer.textureType = TextureImporterType.Advanced;
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                material.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture>(target));

            }
            EditorUtility.DisplayProgressBar("批量替换中", AssetDatabase.GUIDToAssetPath(uids[i]),
                (float)i / (uids.Length));
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

    }



    #region 创建RGB和Alpha分离的TP图集
    [MenuItem("UIFrameWork/NGUI工具/批量创建RGB和Alpha分离的TP图集", false, 10)]
    static void BatchCreateUIAtlasPrefabs()
    {
        Object[] objs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        foreach (Object ob in objs)
        {
            CreateUIAtlasPrefab(ob);
        }
        AssetDatabase.Refresh();
    }

   

    [MenuItem("UIFrameWork/NGUI工具/单个创建RGB和Alpha分离的TP图集", false, 11)]
    static void SingleCreateUIAtlasPrefab()
    {
        CreateUIAtlasPrefab(Selection.activeObject);
        AssetDatabase.Refresh();
    }

    

    private static void CreateUIAtlasPrefab(Object ob)
    {
        string path = AssetDatabase.GetAssetPath(ob);
        if (string.IsNullOrEmpty(path) || !IsTextureFile(path))
        {
            Debug.LogError("未选中对象或者选择的对象不是图片");
            return;
        }

        if (Path.GetExtension(path) == ".png" && !path.Contains("_Alpha") && !path.Contains("_RGB"))
        {
            ShowProgress(path, 0.5f);

            #region 第一步：根据图片创建材质对象
            Material mat = new Material(Shader.Find("UI/UI_ETC"));
            mat.name = ob.name;
            AssetDatabase.CreateAsset(mat, path.Replace(".png", ".mat"));
            SetTextureReadable(path);

            SeperateRGBandAlphaChannel(path);

            TextureSetting(path, TextureImporterType.Advanced, TextureImporterFormat.ETC_RGB4, false);
            Texture2D _mainTex = AssetDatabase.LoadAssetAtPath(path.Replace(".png", "_RGB.png"), typeof(Texture2D)) as Texture2D;
            Texture2D _alphaTex = AssetDatabase.LoadAssetAtPath(path.Replace(".png", "_Alpha.png"), typeof(Texture2D)) as Texture2D;
            mat.SetTexture("_MainTex", _mainTex);
            mat.SetTexture("_AlphaTex", _alphaTex);
            #endregion

            GameObject go = null;
            UIAtlas uiAtlas = null;
            if ((go = AssetDatabase.LoadAssetAtPath(path.Replace(".png", ".prefab"), typeof(GameObject)) as GameObject) != null)
            {
                uiAtlas = SetAtlasInfo(go, path, mat);
            }
            else
            {
                go = new GameObject(ob.name);
                go.AddComponent<UIAtlas>();
                uiAtlas = SetAtlasInfo(go, path, mat);

                #region 第三步：创建预设
                CreatePrefab(go, ob.name, path);
                #endregion
            }
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
        }
    }


    private static UIAtlas SetAtlasInfo(GameObject go, string path, Material mat)
    {
        #region 第二步：给对象添加组件、给材质球关联着色器及纹理同时关联tp产生的坐标信息文件
        if (AssetDatabase.LoadAssetAtPath(path.Replace(".png", ".txt"), typeof(TextAsset)))
        {
            UIAtlas uiAtlas = go.GetComponent<UIAtlas>();
            uiAtlas.spriteMaterial = mat;
            //加载tp产生的记事本
            TextAsset ta = AssetDatabase.LoadAssetAtPath(path.Replace(".png", ".txt"), typeof(TextAsset)) as TextAsset;
            NGUIJson.LoadSpriteData(uiAtlas, ta);
            uiAtlas.MarkAsChanged();
            return uiAtlas;
        }
        #endregion
        return null;
    }

    /// <summary>
    /// 创建临时预设
    /// </summary>
    public static Object CreatePrefab(GameObject go, string name, string path)
    {
        Object tmpPrefab = PrefabUtility.CreateEmptyPrefab(path.Replace(".png", ".prefab"));
        tmpPrefab = PrefabUtility.ReplacePrefab(go, tmpPrefab, ReplacePrefabOptions.ConnectToPrefab);
        Object.DestroyImmediate(go);
        return tmpPrefab;
    }

    public static float sizeScale = 1f;
    /// <summary>
    /// 分离RGB和ALPHA
    /// </summary>
    /// <param name="texturePath"></param>
    static void SeperateRGBandAlphaChannel(string texturePath)
    {
        Texture2D sourcetex = AssetDatabase.LoadAssetAtPath(texturePath, typeof(Texture2D)) as Texture2D;
        if (!sourcetex)
        {
            Debug.LogError("Load Texture Failed : " + texturePath);
            return;
        }
        //if (!HasAlphaChannel(sourcetex))
        //{
        //    Debug.LogError("Texture does not have Alpha channel : " + texturePath);
        //    return;
        //}
        Texture2D rgbTex = new Texture2D(sourcetex.width, sourcetex.height, TextureFormat.RGB24, true);
        Texture2D alphaTex = new Texture2D((int) (sourcetex.width * sizeScale), (int) (sourcetex.height * sizeScale),
            TextureFormat.RGB24, true);
        for (int i = 0; i < sourcetex.width; ++i)
        {
            for (int j = 0; j < sourcetex.height; ++j)
            {
                Color color = sourcetex.GetPixel(i, j);
                Color rgbColor = color;
                Color alphaColor = color;
                alphaColor.r = color.a;
                alphaColor.g = color.a;
                alphaColor.b = color.a;
                rgbTex.SetPixel(i, j, rgbColor);
                alphaTex.SetPixel((int)(i * sizeScale), (int)(j * sizeScale), alphaColor);
            }
        }
        rgbTex.Apply();
        alphaTex.Apply();
        byte[] bytes = null;
        bytes = alphaTex.EncodeToPNG();
        WritePNG(texturePath.Replace(".png", "_Alpha.png"), bytes);

        if (rgbTex != null)
        {
            bytes = rgbTex.EncodeToPNG();
            WritePNG(texturePath.Replace(".png", "_RGB.png"), bytes);
            Debug.Log("Fail to seperate RGB and Alpha channel for texture : " + texturePath);
        }
      

        Debug.Log("Succeed to seperate RGB and Alpha channel for texture : " + texturePath);
    }

    /// <summary>
    /// 写入图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bytes"></param>
    static void WritePNG(string path, byte[] bytes)
    {
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        TextureSetting(path, TextureImporterType.Advanced, TextureImporterFormat.ETC_RGB4, false);
    }

    /// <summary>
    /// 是否有alpha通道
    /// </summary>
    /// <param name="_tex"></param>
    /// <returns></returns>
    static bool HasAlphaChannel(Texture2D _tex)
    {
        for (int i = 0; i < _tex.width; ++i)
            for (int j = 0; j < _tex.height; ++j)
            {
                Color color = _tex.GetPixel(i, j);
                float alpha = color.a;
                if (alpha < 1.0f - 0.001f)
                {
                    return true;
                }
            }
        return false;
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    /// <param name="path"></param>
    /// <param name="val"></param>
    static public void ShowProgress(string path, float val)
    {
        EditorUtility.DisplayProgressBar("批量处理中...", string.Format("Please wait...  Path:{0}", path), val);
    }

    static void SetTextureReadable(string path)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter == null) return;
        textureImporter.textureType = TextureImporterType.Advanced;
        textureImporter.isReadable = true;
        AssetDatabase.ImportAsset(path);
    }

    /// <summary>
    /// 设置图片格式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="mTextureImporterType"></param>
    /// <param name="mTextureImporterFormat"></param>
    /// <param name="readEnable"></param>
    static void TextureSetting(string path, TextureImporterType mTextureImporterType = TextureImporterType.Advanced, TextureImporterFormat mTextureImporterFormat = TextureImporterFormat.RGBA32, bool readEnable = false)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        if (textureImporter == null) return;
        textureImporter.textureType = mTextureImporterType;
        if (textureImporter.textureType == TextureImporterType.Advanced)
        {
            textureImporter.spriteImportMode = SpriteImportMode.None;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = readEnable;
            textureImporter.alphaIsTransparency = false;
        }
        else if (textureImporter.textureType == TextureImporterType.Sprite)
        {
            textureImporter.mipmapEnabled = false;
        }
        textureImporter.SetPlatformTextureSettings("Android", 2048, mTextureImporterFormat);
        textureImporter.SetPlatformTextureSettings("Windows", 2048, mTextureImporterFormat);
        textureImporter.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGB4);
        textureImporter.SetAllowsAlphaSplitting(false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 判断是否是图片格式
    /// </summary>
    /// <param name="_path"></param>
    /// <returns></returns>
    static bool IsTextureFile(string _path)
    {
        string path = _path.ToLower();
        return path.EndsWith(".psd") || path.EndsWith(".tga") || path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".dds") || path.EndsWith(".bmp") || path.EndsWith(".tif") || path.EndsWith(".gif");
    }

    #endregion
}