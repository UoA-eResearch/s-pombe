using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.Networking;

public class ToggleHandler : MonoBehaviour
{
	private LoadDat dat;
	public void Awake()
	{
		dat = GameObject.Find("root").GetComponent<LoadDat>();
	}

	//[ServerCallback]
	public void Toggle(bool on)
	{
		if (on)
		{
			//dat.RpcLoadWeight(gameObject.name);
			dat.LoadWeight(gameObject.name);
			Debug.Log ("In toggle handler on");
		}
		else
		{
			//dat.RpcRemoveWeight (gameObject.name);
			dat.RemoveWeight (gameObject.name);
			Debug.Log("In toggle handler off");
		}
	}
}