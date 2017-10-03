using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class ToggleHandler : MonoBehaviour
{
	private LoadDat dat;
	public void Awake()
	{
		dat = GameObject.Find("root").GetComponent<LoadDat>();
	}
	public void Toggle(bool on)
	{
		if (on)
		{
			dat.LoadWeight(gameObject.name);
		}
		else
		{
			var gameObjects = dat.markers[gameObject.name];
			foreach (var go in gameObjects) Destroy(go);
			dat.markers[gameObject.name] = new List<GameObject>();
		}
	}
}