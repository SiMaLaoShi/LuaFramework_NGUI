using UnityEngine;
using System.Collections;
using LuaFramework;

public class StartGame : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    GameObject uiRoot = Instantiate(Resources.Load<GameObject>("Prefabs/UI Root"));

	    Debug.Log(LuaHelper.GetUIManager().MainUIRoot.name);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
