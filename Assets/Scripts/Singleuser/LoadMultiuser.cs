using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

public class LoadMultiuser : MonoBehaviour {

	public bool CreateRoom = false;
	public string RoomName = "room1";

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick(){
		//LocalNetworkManager.singleton.StartMatchMaker();
		var loading = SceneManager.LoadSceneAsync ("Multiuser", LoadSceneMode.Single);
		//yield return loading;
		var sceneMulti = SceneManager.GetSceneByName ("Multiuser");
		SceneManager.SetActiveScene (sceneMulti);

		GetComponent<DNASpawner>().enabled = true;
	}

}
