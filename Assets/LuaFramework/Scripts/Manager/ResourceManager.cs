using UnityEngine;
using System.Collections;
using System.IO;
using System;
using UObject = UnityEngine.Object;

namespace LuaFramework {
    public class ResourceManager : Manager {
        private AssetBundle shared;

        public enum Priority
        {
            non = -100,
            Low = -1,
            Normal = 0,
            High = 1,
            Top = 2,
        }

        public delegate void LoadResCallback(UObject resLoaded);


        public void LoadResourceAsync(string path, LoadResCallback cb, string abName = null)
        {
            LoadResourceAsync(path, cb, Priority.non, abName);
        }

        /// <summary>
        /// �첽������Դ��������Դ�������ȼ�
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cb"></param>
        /// <param name="priority"></param>
        public void LoadResourceAsync(string path, LoadResCallback cb, Priority priority, string abName = null)
        {
            StartCoroutine(OnLoadLocalResAsync(path, cb, priority));
        }

        IEnumerator OnLoadLocalResAsync(string resName, LoadResCallback cb, Priority priority)
        {
            ResourceRequest req = Resources.LoadAsync(resName, typeof(UnityEngine.Object));
            if (priority != Priority.non)
                req.priority = (int)priority;
            yield return req;

            if (cb != null)
                cb(req.asset);
        }

        /// <summary>
        /// 初始�?
        /// </summary>
        public void initialize(Action func) {
            if (AppConst.ExampleMode) {
                //------------------------------------Shared--------------------------------------
                string uri = Util.DataPath + "shared" + AppConst.ExtName;
                Debug.LogWarning("LoadFile::>> " + uri);

                shared = AssetBundle.LoadFromFile(uri);
#if UNITY_5
                shared.LoadAsset("Dialog", typeof(GameObject));
#else
                shared.Load("Dialog", typeof(GameObject));
#endif
            }
            if (func != null) func();    //资源初始化完成，回调游戏管理器，执行后续操作 
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        public AssetBundle LoadBundle(string name) {
            string uri = Util.DataPath + name.ToLower() + AppConst.ExtName;
            AssetBundle bundle = AssetBundle.LoadFromFile(uri); //关联数据的素材绑�?
            return bundle;
        }

        //public UObject LoadResourcesSyncByType(string path, Type type, string abName = null)
        //{
        //    UObject obj = Resources.Load(path, type); ;
        //    if (AppConst.isNetRes)
        //    {
        //        if (obj == null)
        //        {
        //            obj = LoadAssetSyncByType(path, type, abName);
        //            ResetShader(obj);
        //        }
        //    }

        //    return obj;
        //}

        //public UObject LoadAssetSyncByType(string path, Type type, string abName = null)
        //{
        //    path = path.Replace("\\", "/");
        //    if (abName == null)
        //    {
        //        abName = path;
        //    }
        //    string assetName = path.Substring(path.LastIndexOf("/") + 1);
        //    AssetBundleInfo bundleInfo = LoadAssetBundle(abName);
        //    if (bundleInfo != null)
        //        return bundleInfo.m_AssetBundle.LoadAsset(assetName, type);
        //    return null;
        //}

        public GameObject LoadLocalGameObject(string path, string abName = null)
        {
            UObject prefab = LoadResourcesSync(path, abName);
            if (prefab == null)
            {
                Debug.LogWarning("Resoure�²�����·��:" + path);
                return null;
            }
            GameObject obj = GameObject.Instantiate(prefab) as GameObject;
            return obj;
        }

        public UObject LoadResourcesSync(string path, string abName = null)
        {
            path = path.Replace("\\", "/");
            UObject obj;
            obj = Resources.Load(path, typeof(UObject));
            return obj;
        }

        /// <summary>
        /// 销毁资�?
        /// </summary>
        void OnDestroy() {
            if (shared != null) shared.Unload(true);
            Debug.Log("~ResourceManager was destroy!");
        }
    }
}