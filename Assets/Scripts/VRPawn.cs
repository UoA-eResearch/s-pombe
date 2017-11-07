using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

public class VRPawn : NetworkBehaviour {

    public Transform Head;
    public Transform LeftController;
    public Transform RightController;

	//[SyncVar]
	//public GameObject rootPrefab;

    void Start () {
		//Debug.Log ("In Start");
		//Debug.Log (isServer);
		//Debug.Log (isLocalPlayer);

        if (isLocalPlayer) { 
            GetComponentInChildren<SteamVR_ControllerManager>().enabled = true;
            GetComponentsInChildren<SteamVR_TrackedObject>(true).ToList().ForEach(x => x.enabled = true);
            Head.GetComponentsInChildren<MeshRenderer>(true).ToList().ForEach(x => x.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
            gameObject.name = "LocalPlayer";

			//GameObject root = Instantiate(GameObject.Find ("root"), transform.position, transform.rotation);
			//Debug.Log("In local player");
			//CmdSpawn ();
			if (isServer) {
				//GameObject root = GameObject.Find ("root");//Instantiate (rootPrefab, transform.position, transform.rotation);
				//Debug.Log(root);
				//root.GetComponent<LoadDat> ().Init ();
				//NetworkServer.Spawn (root);
			} else {
				
			}


        } else
        {
            gameObject.name = "RemotePlayer";

        }

	}

	//[Command]
	//void CmdSpawn(){
	//	Debug.Log ("In spawn");
		//GameObject root = Instantiate (GameObject.Find("root"), transform.position, transform.rotation);

		//Debug.Log (connectionToClient);
	//	Spawn();
	//}

	//[Server]
	//void Spawn(){
	//	GameObject root = GameObject.Find ("root");
	//	LoadDat dat = root.GetComponent<LoadDat> ();
	//	dat.Init ();
	//	NetworkServer.Spawn (root);
	//}
		

    void OnDestroy()
    {
        GetComponentInChildren<SteamVR_ControllerManager>().enabled = false;
        GetComponentsInChildren<SteamVR_TrackedObject>(true).ToList().ForEach(x => x.enabled = false);
    }
}
