using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DNASpawner : NetworkBehaviour {

	public GameObject dnaPrefab;
	public GameObject menuPrefab;
	//[SyncVar]
	public int ind;

	public void Start(){
		
		Debug.Log ("in start client");

		if (isLocalPlayer) {
			ind = 0;
			if (!isServer) {
				Debug.Log ("spawn now");
				CmdSpawn ();
			}
		}
	}

	public int getIndex(){
		return ind;
	}

	public void InstantiateDNA(){

		if (isLocalPlayer) {
			ind++;
			if (!isServer) {
				dnaPrefab = (GameObject)Resources.Load("root", typeof(GameObject));
				CmdReSpawn ();
			}
		}
	}


	[Command]
	public void CmdReSpawn(){
		RpcReSpawn ();
	}


	[ClientRpc]
	public void RpcReSpawn(){
		Debug.Log (isLocalPlayer + " " + isServer);

		if (connectionToClient != null) {
			GameObject dna = (GameObject)Instantiate ((GameObject)Resources.Load ("root", typeof(GameObject)), transform);
			NetworkServer.SpawnWithClientAuthority (dna, connectionToClient);


			LoadDat dat = dna.GetComponent<LoadDat> ();
			dat.RpcLoadFromDat ();
			dat.RpcAssignButton (GameObject.Find ("Menu(Clone)"));
		}
	}


	[Command]
	public void CmdSpawn(){
		var dna = (GameObject)Instantiate ((GameObject)Resources.Load("root", typeof(GameObject)), transform);
		NetworkServer.SpawnWithClientAuthority (dna, connectionToClient);
		var dat = dna.GetComponent<LoadDat> ();
		dat.RpcLoadFromDat ();

		var menu = Instantiate(menuPrefab, transform.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority (menu, connectionToClient);
		dat.RpcSpawnMenu ();

		dat.RpcAssignButton (GameObject.Find("Menu(Clone)"));
	}

	void Update(){

	}

}
