using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ToggleHandler : NetworkBehaviour
{
	private LoadDat dat;
	public void Awake()
	{
		dat = GameObject.Find("root").GetComponent<LoadDat>();
	}

	[ServerCallback]
	public void Toggle(bool on)
	{
		if (on)
		{
			dat.RpcLoadWeight(gameObject.name);
			Debug.Log ("In toggle handler on");
		}
		else
		{
			dat.RpcRemoveWeight (gameObject.name);
			Debug.Log("In toggle handler off");
		}
	}
}