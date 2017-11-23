using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.Networking;

public class ToggleHandler : MonoBehaviour
{
	private GameObject dna;

	public void Toggle(bool on)
	{
		dna = GameObject.Find ("root(Clone)");
		print (gameObject);
		print (dna);
		print (on);
		dna.GetComponent<LoadDat>().CmdToggle (on, gameObject.name);
	}

}