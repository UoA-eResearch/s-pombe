using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DNASpawner : NetworkBehaviour {

	public GameObject root;

	public override void OnStartServer(){

		//Debug.Log ("In DNA Spawner");
		//GameObject dna = (GameObject)Instantiate (root, transform.position, transform.rotation);

		//NetworkServer.Spawn (dna);
		//dna.GetComponent<LoadDat> ().Init ();
	}
}
