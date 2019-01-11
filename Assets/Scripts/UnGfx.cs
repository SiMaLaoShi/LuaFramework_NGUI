using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class UnGfx
{
    public static Vector3 hidePos = new Vector3(0.0f, 65536f, 0.0f);
    public static string chanelInfo = "";
    private static Color cl = new Color(0.15f, 0.15f, 0.15f, 1f);
    private static Dictionary<string, Texture2D> texDic = new Dictionary<string, Texture2D>();
    private const float UN_PI = 3.141593f;
    public static Transform skyCamNode;

   

    public static Transform GetSkyCameraNode()
    {
        if ((bool)((UnityEngine.Object)Camera.main) && (UnityEngine.Object)UnGfx.skyCamNode == (UnityEngine.Object)null)
            UnGfx.skyCamNode = Camera.main.transform.Find("sky");
        return UnGfx.skyCamNode;
    }

    public static Vector3 TranslateOnPlane(Vector3 start, Vector3 normal, Vector3 origin, Vector3 dir)
    {
        float num1 = Vector3.Dot(start - origin, normal);
        float num2 = Vector3.Dot(dir, normal);
        return origin + dir * (num1 / num2);
    }

    public static void Attach(Transform parent, Transform child)
    {
        UnGfx.Attach(parent, child, true);
    }

    public static void Attach(Transform parent, Transform child, bool keepLocal)
    {
        if ((UnityEngine.Object)parent == (UnityEngine.Object)null)
            return;
        if (keepLocal)
        {
            Vector3 localPosition = child.localPosition;
            Quaternion localRotation = child.localRotation;
            Vector3 localScale = child.localScale;
            child.parent = parent;
            child.localPosition = localPosition;
            child.localRotation = localRotation;
            child.localScale = localScale;
        }
        else
            child.parent = parent;
    }

    public static void AttachToParentWithOffset(Transform parent, Transform child, Vector3 offset)
    {
        UnGfx.AttachToParentWithOffset(parent, child, offset, Quaternion.identity);
    }

    public static void AttachToParentWithOffset(Transform parent, Transform child, Vector3 offset, Quaternion quaternion)
    {
        if ((UnityEngine.Object)parent == (UnityEngine.Object)null)
            return;
        child.parent = parent;
        child.transform.localRotation = quaternion;
        child.transform.localPosition = Vector3.zero + offset;
    }

    public static Transform FindNodeByPath(Transform node, string path)
    {
        return node.Find(path);
    }

    public static Transform FindNode(Transform node, string name)
    {
        if ((UnityEngine.Object)node == (UnityEngine.Object)null || node.name == name)
            return node;
        foreach (object obj in node)
        {
            Transform node1 = UnGfx.FindNode(obj as Transform, name);
            if ((bool)((UnityEngine.Object)node1) && node1.name == name)
                return node1;
        }
        return (Transform)null;
    }

    public static void FindNodeList(Transform node, string name, List<Transform> list)
    {
        foreach (object obj in node)
        {
            Transform node1 = obj as Transform;
            if (node1.name == name)
                list.Add(node1);
            UnGfx.FindNodeList(node1, name, list);
        }
    }

    public static Transform FindNode2(Transform root, string name)
    {
        Transform[] componentsInChildren = root.GetComponentsInChildren<Transform>();
        for (int index = 0; index < componentsInChildren.Length; ++index)
        {
            if (componentsInChildren[index].name == name)
                return componentsInChildren[index];
        }
        return (Transform)null;
    }

    public static void DisableRoleParticle(Transform node)
    {
        Transform[] componentsInChildren = node.GetComponentsInChildren<Transform>();
        for (int index = 0; index < componentsInChildren.Length; ++index)
        {
            ParticleSystem componentInChildren1 = componentsInChildren[index].GetComponentInChildren<ParticleSystem>();
            ParticleSystemRenderer componentInChildren2 = componentsInChildren[index].GetComponentInChildren<ParticleSystemRenderer>();
            if ((bool)((UnityEngine.Object)componentInChildren1))
                UnityEngine.Object.Destroy((UnityEngine.Object)componentInChildren1);
            if ((bool)((UnityEngine.Object)componentInChildren2))
                UnityEngine.Object.Destroy((UnityEngine.Object)componentInChildren2);
        }
    }

    public static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Component component in obj.transform)
            UnGfx.SetLayerRecursively(component.gameObject, layer);
    }

    public static void SetStaticRecursively(GameObject obj, bool flag)
    {
        foreach (Component component in obj.transform)
            UnGfx.SetStaticRecursively(component.gameObject, flag);
    }

    public static T FindComponentInParent<T>(Transform node) where T : Component
    {
        for (; (bool)((UnityEngine.Object)node); node = node.parent)
        {
            T component = node.gameObject.GetComponent<T>();
            if ((bool)((UnityEngine.Object)component))
                return component;
        }
        return default(T);
    }

    public static void ReplaceNode(Transform node, Transform attach)
    {
        Vector3 localPosition = node.localPosition;
        Quaternion localRotation = node.localRotation;
        Vector3 localScale = node.localScale;
        attach.parent = node.parent;
        attach.name = node.name;
        attach.localPosition = localPosition;
        attach.localRotation = localRotation;
        attach.localScale = localScale;
        UnityEngine.Object.DestroyObject((UnityEngine.Object)node.gameObject);
    }

    public static void Detach(Transform node)
    {
        node.parent = (Transform)null;
    }

    public static void SetObjectCull(GameObject obj, bool hide)
    {
        foreach (Renderer componentsInChild in obj.GetComponentsInChildren<Renderer>())
            componentsInChild.enabled = !hide;
    }

    public static void FastHide(Transform node)
    {
        node.position = new Vector3(65536f, 65536f, 65536f);
    }

    public static void NormalizeQuaternion(ref Quaternion q)
    {
        float f = 0.0f;
        for (int index = 0; index < 4; ++index)
            f += q[index] * q[index];
        float num = 1f / Mathf.Sqrt(f);
        for (int index = 0; index < 4; ++index)
            q[index] *= num;
    }

    public static Matrix4x4 Convert(Quaternion q)
    {
        Matrix4x4 mat = new Matrix4x4();
        UnGfx.Convert(q, ref mat);
        return mat;
    }

    public static void Convert(Quaternion q, ref Matrix4x4 mat)
    {
        UnGfx.NormalizeQuaternion(ref q);
        mat.SetTRS(Vector3.zero, q, Vector3.one);
    }

    public static Quaternion Convert(Matrix4x4 m)
    {
        Quaternion q = new Quaternion();
        UnGfx.Convert(m, ref q);
        return q;
    }

    public static void Convert(Matrix4x4 m, ref Quaternion q)
    {
        q.w = Mathf.Sqrt(Mathf.Max(0.0f, 1f + m[0, 0] + m[1, 1] + m[2, 2])) / 2f;
        q.x = Mathf.Sqrt(Mathf.Max(0.0f, 1f + m[0, 0] - m[1, 1] - m[2, 2])) / 2f;
        q.y = Mathf.Sqrt(Mathf.Max(0.0f, 1f - m[0, 0] + m[1, 1] - m[2, 2])) / 2f;
        q.z = Mathf.Sqrt(Mathf.Max(0.0f, 1f - m[0, 0] - m[1, 1] + m[2, 2])) / 2f;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
    }

    public static BoxCollider Convert(Bounds bound)
    {
        BoxCollider bc = new BoxCollider();
        UnGfx.Convert(bound, ref bc);
        return bc;
    }

    public static void Convert(Bounds bound, ref BoxCollider bc)
    {
        bc.center = bound.center;
        bc.size = bound.size;
    }

    public static void SetIgnoreCollisionLayer(int layer)
    {
        for (int layer1 = 0; layer1 < 32; ++layer1)
        {
            if ((uint)(1 << layer1 & layer) > 0U)
            {
                for (int layer2 = 0; layer2 < 32; ++layer2)
                    Physics.IgnoreLayerCollision(layer1, layer2, true);
            }
        }
    }

    public static T GetSafeComponent<T>(GameObject obj) where T : Component
    {
        T obj1 = obj.GetComponent<T>();
        if (!(bool)((UnityEngine.Object)obj1))
            obj1 = obj.AddComponent<T>();
        return obj1;
    }

    public static T GetSafeComponent<T>(Transform node) where T : Component
    {
        GameObject gameObject = node.gameObject;
        T obj = gameObject.GetComponent<T>();
        if (!(bool)((UnityEngine.Object)obj))
            obj = gameObject.AddComponent<T>();
        return obj;
    }

    public static void AddCollider(GameObject obj)
    {
        foreach (MeshFilter componentsInChild in obj.GetComponentsInChildren<MeshFilter>())
        {
            Mesh mesh = componentsInChild.mesh;
            UnGfx.GetSafeComponent<MeshCollider>(componentsInChild.gameObject).sharedMesh = mesh;
        }
    }

    public static Material GetMainMaterial(GameObject obj)
    {
        Renderer component = obj.GetComponent<Renderer>();
        if ((bool)((UnityEngine.Object)component))
            return component.material;
        return (Material)null;
    }

    public static string RemoveCloneString(string name)
    {
        int length = name.IndexOf("(Clone)");
        if (length > 0)
            name = name.Substring(0, length);
        return name;
    }

    public static void CopyTransformPRS(Transform newTran, Transform oldTran)
    {
        newTran.localPosition = oldTran.localPosition;
        newTran.localRotation = oldTran.localRotation;
        newTran.localScale = oldTran.localScale;
    }

    public static void DeActiveTrail(Transform transform)
    {
        List<Transform> list = new List<Transform>();
        UnGfx.FindNodeList(transform, "Trail", list);
        for (int index = 0; index < list.Count; ++index)
            list[index].gameObject.SetActive(false);
    }

    public static void ClearTrail(Transform transform)
    {
        List<Transform> list = new List<Transform>();
        UnGfx.FindNodeList(transform, "Trail", list);
        for (int index = 0; index < list.Count; ++index)
        {
            list[index].parent = (Transform)null;
            UnityEngine.Object.Destroy((UnityEngine.Object)list[index].gameObject);
        }
    }

    public static T GetMapValue<T>(Dictionary<int, T> table, int id)
    {
        T obj;
        if (!table.TryGetValue(id, out obj))
            Debugger.LogError("Table Dictionary<int, {0}> do not have an index {1}", (object)typeof(T), (object)id);
        return obj;
    }

    public static float GetSkinMeshHeight(GameObject obj)
    {
        SkinnedMeshRenderer componentInChildren = obj.GetComponentInChildren<SkinnedMeshRenderer>();
        return (UnityEngine.Object)componentInChildren == (UnityEngine.Object)null ? 0.0f : componentInChildren.bounds.size.y;
    }

    public static string GetFileName(string path)
    {
        int num = path.LastIndexOf("/");
        if (num <= 0)
            num = path.LastIndexOf("\\");
        if (num > 0 && num + 1 < path.Length)
            path = path.Substring(num + 1);
        return path;
    }

    public static void ShakeMobile()
    {
    }

    public static long ProfileMemoryBegin()
    {
        return GC.GetTotalMemory(false);
    }

    public static void ProfileMemoryEnd(string name, long memory)
    {
        GC.Collect(0);
        memory -= GC.GetTotalMemory(false);
    }

    public static AsyncOperation ClearMemory()
    {
        GC.Collect(0);
        GC.Collect(1);
        return Resources.UnloadUnusedAssets();
    }

    public static void SetMaterialClip(Material mat, Vector4 v4)
    {
        float num1 = (float)(Screen.width / 2);
        float num2 = (float)(Screen.height / 2);
        v4.x /= num1;
        v4.y /= num2;
        v4.z /= 2f * num1;
        v4.w /= 2f * num2;
        mat.SetVector("_Clip", v4);
    }

    public static void SetRenderQueue(GameObject obj, string matName, int z)
    {
        Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
        for (int index = 0; index < componentsInChildren.Length; ++index)
        {
            if (componentsInChildren[index].material.shader.name == matName)
                componentsInChildren[index].material.renderQueue = z;
        }
    }

    public static void SetActorLight(GameObject obj, bool bflag)
    {
        Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
        for (int index1 = 0; index1 < componentsInChildren.Length; ++index1)
        {
            for (int index2 = 0; index2 < componentsInChildren[index1].materials.Length; ++index2)
            {
                componentsInChildren[index1].materials[index2].shader = bflag ? Shader.Find("Custom/MonsterRim") : Shader.Find("Custom/MobileDiffuse");
                if (bflag)
                {
                    componentsInChildren[index1].material.SetColor("_RimColor", Color.red);
                    componentsInChildren[index1].materials[index2].SetFloat("_RimWidth", 1.5f);
                }
            }
        }
    }

    public static string BytesToHexString(byte[] src)
    {
        StringBuilder stringBuilder = new StringBuilder("");
        if (src == null || src.Length == 0)
            return (string)null;
        for (int index = 0; index < src.Length; ++index)
        {
            int num = (int)src[index] & (int)byte.MaxValue;
            stringBuilder.Append(num.ToString("X2"));
        }
        return stringBuilder.ToString();
    }


    public static void Assert(UnityEngine.Object obj, string str)
    {
        if (!(obj == (UnityEngine.Object)null))
            return;
        Debugger.LogError(str);
    }

    public static void Assert(object obj, string str)
    {
        if (obj != null)
            return;
        Debugger.LogError(str);
    }

    public static Vector3 String2Vector3(string str)
    {
        string[] strArray = str.Split(',');
        if (strArray.Length != 3)
            return Vector3.zero;
        return new Vector3(float.Parse(strArray[0]), float.Parse(strArray[1]), float.Parse(strArray[2]));
    }

    public static string GetAniName(string baseName, int ntype)
    {
        return string.Format("{0}_{1}", (object)baseName, (object)ntype);
    }

    public static void PaserString2Camera(string str, ref Vector3 pos, ref Vector3 angle)
    {
        string[] strArray = str.Split(':');
        string str1 = strArray[0];
        string str2 = str1.Substring(1, str1.Length - 2);
        pos = UnGfx.String2Vector3(str2);
        string str3 = strArray[1];
        string str4 = str3.Substring(1, str3.Length - 2);
        angle = UnGfx.String2Vector3(str4);
    }

    public static string[] PaserActorArrayPos(string strPos)
    {
        return strPos.Split(':');
    }

    public static Vector3 PaserActorPos(string str)
    {
        str.Trim();
        return UnGfx.String2Vector3(str.Substring(1, str.Length - 2));
    }

    public static bool IsWifi
    {
        get
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }

    public static byte[] ConverStructToByte(object structObj)
    {
        int length = Marshal.SizeOf(structObj);
        byte[] destination = new byte[length];
        IntPtr num = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(structObj, num, false);
        Marshal.Copy(num, destination, 0, length);
        Marshal.FreeHGlobal(num);
        return destination;
    }

    public static string md5file(string file)
    {
        try
        {
            FileStream fileStream = new FileStream(file, FileMode.Open);
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash((Stream)fileStream);
            fileStream.Close();
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < hash.Length; ++index)
                stringBuilder.Append(hash[index].ToString("x2"));
            return stringBuilder.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }


    public static string GetColorCode(Color color, string context)
    {
        return UnGfx.GetColorCode(new Color32((byte)Mathf.CeilToInt(color.r * (float)byte.MaxValue), (byte)Mathf.CeilToInt(color.g * (float)byte.MaxValue), (byte)Mathf.CeilToInt(color.b * (float)byte.MaxValue), (byte)Mathf.CeilToInt(color.a * (float)byte.MaxValue)), context);
    }

    public static string GetColorCode(Color32 color, string context)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("[");
        stringBuilder.Append(color.r.ToString("X").PadLeft(2, '0'));
        stringBuilder.Append(color.g.ToString("X").PadLeft(2, '0'));
        stringBuilder.Append(color.b.ToString("X").PadLeft(2, '0'));
        stringBuilder.Append("]");
        stringBuilder.Append(context);
        stringBuilder.Append("[-]");
        return stringBuilder.ToString();
    }



    public static Dictionary<string, Transform> GetAllBipTF(Transform tf)
    {
        Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
        Transform[] componentsInChildren = tf.GetComponentsInChildren<Transform>();
        for (int index = 0; index < componentsInChildren.Length; ++index)
            dictionary[componentsInChildren[index].name] = componentsInChildren[index];
        return dictionary;
    }

    public static void ChangeSkinnedMeshRenderer(SkinnedMeshRenderer mine, SkinnedMeshRenderer target, Dictionary<string, Transform> dic)
    {
        List<Transform> transformList = new List<Transform>();
        foreach (UnityEngine.Object bone in target.bones)
        {
            string name = bone.name;
            if (dic.ContainsKey(name))
                transformList.Add(dic[name]);
        }
        mine.sharedMesh = target.sharedMesh;
        mine.materials = target.sharedMaterials;
        mine.bones = transformList.ToArray();
    }

    public static GameObject Instantiate(UnityEngine.Object asset)
    {
        if (asset == (UnityEngine.Object)null)
            return (GameObject)null;
        return (GameObject)UnityEngine.Object.Instantiate(asset);
    }

    public static Texture2D GetUITexture(string path)
    {
        Texture2D texture2D = (Texture2D)null;
        if (!UnGfx.texDic.TryGetValue(path, out texture2D))
        {
            texture2D = Resources.Load(path) as Texture2D;
            UnGfx.texDic.Add(path, texture2D);
        }
        return texture2D;
    }


    public static bool ObjectIsNULL(object obj)
    {
        return obj as UnityEngine.Object == (UnityEngine.Object)null;
    }

    public static int GetStringByteLength(string str)
    {
        return Encoding.Default.GetBytes(str).Length;
    }

    public static Vector3 Rotate(Vector3 v, float angle)
    {
        return Quaternion.Euler(0.0f, 0.0f, angle) * v;
    }

    public static float GetMoveTowards(float current, float target, float maxDelta)
    {
        return Mathf.MoveTowards(current, target, maxDelta);
    }

    public static float Random(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    public static Vector3 GetTerrianPos(Vector3 curPos)
    {
        float num = Terrain.activeTerrain.SampleHeight(curPos);
        curPos.y = (float)((double)num + (double)Terrain.activeTerrain.transform.position.y + 0.100000001490116);
        return curPos;
    }

    public static AnimationState GetAnimationState(Animation _anim, string name)
    {
        return _anim[name];
    }

    public static UnGfx.hitinfo Raycast(Ray ray, float distance, int UILayer)
    {
        UnGfx.hitinfo hitinfo = new UnGfx.hitinfo();
        hitinfo.isUIClick = Physics.Raycast(ray, out hitinfo.hit, distance, UILayer);
        return hitinfo;
    }

    public static UnGfx.hitinfo Raycast(Vector3 origin, Vector3 direction, float distance, int UILayer)
    {
        UnGfx.hitinfo hitinfo = new UnGfx.hitinfo();
        hitinfo.isUIClick = Physics.Raycast(origin, direction, out hitinfo.hit, distance, UILayer);
        return hitinfo;
    }

    public static RaycastHit Raycast1(Ray ray, float distance, int UILayer)
    {
        RaycastHit hitInfo;
        Physics.Raycast(ray, out hitInfo, distance, UILayer);
        return hitInfo;
    }

    public static RaycastHit[] BoxCastAll(Vector3 pos, Vector3 size, Quaternion rot, Vector3 dir, int layermask)
    {
        return Physics.BoxCastAll(pos, size, dir, rot, 0.0f, layermask);
    }

    public static void ScaleParticleSystem(GameObject go)
    {
        if ((UnityEngine.Object)go == (UnityEngine.Object)null)
            return;
        go.SetActive(false);
        go.transform.localScale = Vector3.one * 0.5f;
        float x = go.transform.lossyScale.x;
        ParticleSystem[] componentsInChildren = go.GetComponentsInChildren<ParticleSystem>(true);
        for (int index = componentsInChildren.Length - 1; index >= 0; --index)
        {
            ParticleSystem particleSystem = componentsInChildren[index];
            if (!((UnityEngine.Object)particleSystem == (UnityEngine.Object)null))
            {
                particleSystem.startSize *= x;
                particleSystem.startSpeed *= x;
                particleSystem.startRotation *= x;
            }
        }
        go.SetActive(true);
    }

    public class hitinfo
    {
        public bool isUIClick;
        public RaycastHit hit;
    }
}
