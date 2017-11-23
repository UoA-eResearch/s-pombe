using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadMenu : NetworkBehaviour {
	
	private TextAsset[] weights;
	private Color32[] colors;
	private GameObject dna;
	public GameObject togglePrefab;
	public List<GameObject> toggles = new List<GameObject>();
	private LoadDat dat;


	
	// Update is called once per frame
	void Update () {
		
	}
}
