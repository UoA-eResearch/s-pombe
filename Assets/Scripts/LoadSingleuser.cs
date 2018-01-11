using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSingleuser : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick(){
		//InternetManager.singleton.StopHost();
		//InternetManager.singleton.StopServer ();
		//InternetManager.singleton.StopMatchMaker ();
		//SceneManager.LoadScene ("Singleuser", LoadSceneMode.Single);
		//SceneManager.UnloadSceneAsync ("Multiuser");
		//Network.Disconnect ();
		//LocalNetworkManager.singleton.StopMatchMaker();
		//GameObject intMang = GameObject.Find("InternetManager");
		//GameObject.Destroy (intMang);
		var loading = SceneManager.LoadSceneAsync ("Singleuser", LoadSceneMode.Single);
		//yield return loading;
		var sceneSingle = SceneManager.GetSceneByName ("Singleuser");
		SceneManager.SetActiveScene (sceneSingle);
	}
}
