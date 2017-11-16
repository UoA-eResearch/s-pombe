using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DNASpawner : NetworkBehaviour {

	public GameObject dnaPrefab;
	public GameObject menuPrefab;

	public void Start(){
		
		Debug.Log ("in start client");
		Debug.Log(isServer);
		Debug.Log(isLocalPlayer);

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
		GameObject dna = (GameObject)Instantiate (dnaPrefab, transform.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority (dna, connectionToClient);

		GameObject menu = (GameObject)Instantiate (menuPrefab, transform.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority (menu, connectionToClient);

		dna.GetComponent<LoadDat> ().RpcInit ();

	}
}
