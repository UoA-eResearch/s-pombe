using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DNASpawner : NetworkBehaviour {

	public GameObject dnaPrefab;
	public GameObject menuPrefab;
	private int index = 0;

	public void Start(){
		
		Debug.Log ("in start client");

		if (isLocalPlayer) {
			if (!isServer) {
					Debug.Log ("spawn now");
					CmdSpawn ();
			}
		}
	}

	[Command]
	public void CmdSpawn(){
		Debug.Log ("In Cmd");
		var dna = (GameObject)Instantiate (dnaPrefab, transform.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority (dna, connectionToClient);

		var menu = Instantiate(menuPrefab, transform.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority (menu, connectionToClient);

		var func = dna.GetComponent<LoadDat> ().DeleteDNA ();
		Button nextBut = menu.GetComponentInChildren<Button> ();
		nextBut.onClick.AddListener (func);

		dna.GetComponent<LoadDat> ().RpcLoadFromDat ();
		dna.GetComponent<LoadDat> ().RpcSpawnMenu ();
	}


	[Command]
	public void CmdLoadNext(){

		GameObject dna = (GameObject)Instantiate (dnaPrefab, transform.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority (dna, connectionToClient);

		var dat = GetComponent<LoadDat> ();
		dat.index = index;
		dat.RpcLoadFromDat ();

		var toggles = dat.toggles;
		foreach (var t in toggles) {
			if (t.GetComponent<Toggle> ().isOn) {
				dat.CmdLoadWeight (t.name);
			}
		}
	}

	public void LoadNext(){
		Debug.Log (isLocalPlayer);
		Debug.Log (isServer);
		//if (isLocalPlayer) {
		//	if (!isServer) {
		Debug.Log ("Delete now and load next.");

		RpcDelete ();
		index++;

		//CmdLoadNext ();
		//	}
		//}
	}

	[Command]
	public void CmdDelete(){
		RpcDelete ();
	}

	[ClientRpc]
	public void RpcDelete(){
		GameObject dna = GameObject.FindGameObjectWithTag ("DNA");
		Debug.Log ("dna object valid?" + dna);
		NetworkServer.Destroy (dna);
	}



}
