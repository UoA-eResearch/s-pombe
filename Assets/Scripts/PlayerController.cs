using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	
	public GameObject menu;
	public Transform dna;
	private GameObject fader;
	private Animator anim;
	private Vector3 destination;

	void Awake(){
		fader = GameObject.Find ("Fader");
		anim = fader.GetComponent<Animator> ();
	}


	void ViveControl(int controllerId)
	{

			Debug.Log ("In vivecontrol now");
			var controller = SteamVR_Controller.Input(controllerId);
			if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
			{

			//GameObject player = GameObject.FindGameObjectWithTag ("LocalPlayer");
			//NetworkIdentity playerID = player.GetComponent<NetworkIdentity> ();
			//CmdSetAuth (netId, playerID);


				var v = controller.velocity;
				v.Scale(transform.localScale);
				transform.position += v * 100;
				transform.Rotate(controller.angularVelocity, Space.World);
			}
			if (controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
			{
				var s = controller.GetAxis().y;
				float scale = 1.05f;
				if (s < 0)
				{
					scale = .95f;
				}
				transform.localScale *= scale;
			}

			if (controller.GetPress (SteamVR_Controller.ButtonMask.Touchpad)) {
				Vector2 touchpad = (controller.GetAxis (Valve.VR.EVRButtonId.k_EButton_Axis0));

				if (touchpad.y > 0.7f) {
					print ("Moving Up");
					transform.localScale *= 1.05f;
				} else if (touchpad.y < -0.7f) {
					print ("Moving Down");
					transform.localScale *= .95f;
				}

				if (touchpad.x > 0.7f) {
					print ("Moving Right");
					CmdBasecampForward ();

				} else if (touchpad.x < -0.7f) {
					print ("Moving left");
				}
			}

			if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
			{
				controller.TriggerHapticPulse(1000);
				if (menu.activeSelf)
				{
					menu.SetActive(false);
				}
				else
				{
					menu.SetActive(true);
					menu.transform.position = Camera.main.transform.position;
					menu.transform.rotation = Camera.main.transform.rotation;
					menu.transform.position += menu.transform.forward * 10;
				}
			}
	}

	void Update()
	{
			var leftI = SteamVR_Controller.GetDeviceIndex (SteamVR_Controller.DeviceRelation.Leftmost);
			var rightI = SteamVR_Controller.GetDeviceIndex (SteamVR_Controller.DeviceRelation.Rightmost);

			if (leftI == rightI) {
				// Single Controller
				rightI = -1;
			}

			if (leftI != -1) {
				ViveControl (leftI);
			}

			if (rightI != -1) {
				ViveControl (rightI);
			}
			
		if (Input.GetKeyDown (KeyCode.Space)) {
			print ("space key was pressed");
			//GameObject player =  GameObject.FindGameObjectWithTag ("LocalPlayer");
			//NetworkIdentity playerID = player.GetComponent<NetworkIdentity> ();
			//CmdSetAuth (netId, playerID);

			CmdBasecampForward ();
		}


	}

	[Command]
	public void CmdBasecampForward(){
		RpcBasecampForward ();
	}

	[ClientRpc]
	public void RpcBasecampForward(){
		Vector3 origin = new Vector3 (0, 0, 0);
		Vector3 basecamp1 = new Vector3 (1000, 0, 0);
		Vector3 basecamp2 = new Vector3 (2000, 0, 0);
		Vector3 basecamp3 = new Vector3 (1000, 1000, 0);
		Vector3 basecamp4 = new Vector3 (1000, 0, 1000);


		if (dna.position.Equals(origin)) {
			destination = basecamp1;
		} else if(dna.position.Equals(basecamp1)) {
			destination = basecamp2;
		}
		else if(dna.position.Equals(basecamp2)) {
			destination = basecamp3;
		}
		else if(dna.position.Equals(basecamp3)) {
			destination = basecamp4;
		}
		else if(dna.position.Equals(basecamp4)) {
			destination = origin;
		}

		FadeOut();
		Invoke ("SetPosition", 2.5f);
		Invoke ("FadeIn", 2.5f);
	}

	[Command]
	public void CmdBasecampBackward(){
		RpcBasecampBackward ();
	}

	[ClientRpc]
	public void RpcBasecampBackward(){
		Vector3 origin = new Vector3 (0, 0, 0);
		Vector3 basecamp1 = new Vector3 (1000, 0, 0);
		Vector3 basecamp2 = new Vector3 (2000, 0, 0);
		Vector3 basecamp3 = new Vector3 (1000, 1000, 0);
		Vector3 basecamp4 = new Vector3 (1000, 0, 1000);


		if (dna.position.Equals(origin)) {
			destination = basecamp4;
		} else if(dna.position.Equals(basecamp1)) {
			destination = origin;
		}
		else if(dna.position.Equals(basecamp2)) {
			destination = basecamp1;
		}
		else if(dna.position.Equals(basecamp3)) {
			destination = basecamp2;
		}
		else if(dna.position.Equals(basecamp4)) {
			destination = basecamp3;
		}

		FadeOut();
		Invoke ("SetPosition", 2.5f);
		Invoke ("FadeIn", 2.5f);
	}

	void SetPosition(){
		dna.position = destination;
	}


	void FadeOut()
	{
		anim.SetBool("FadeIn", false);
		anim.SetBool("FadeOut", true);
	}


	void FadeIn()
	{
		anim.SetBool("FadeOut", false);
		anim.SetBool("FadeIn", true);
	}


	[Command]
	public void CmdSetAuth(NetworkInstanceId objectId, NetworkIdentity player){

		var iObject = NetworkServer.FindLocalObject (objectId);
		var networkIdentity = iObject.GetComponent<NetworkIdentity> ();
		var otherOwner = networkIdentity.clientAuthorityOwner;
		Debug.Log ("other owner: " + otherOwner);

		if (otherOwner == player.connectionToClient) {
			Debug.Log ("Player is the owner, return");
			Debug.Log (player.hasAuthority);
			return;
		} else {
		if (otherOwner != null) {
			Debug.Log ("Remove owner");
			networkIdentity.RemoveClientAuthority (otherOwner);
		}
		Debug.Log ("Assign owner now");
		networkIdentity.AssignClientAuthority (player.connectionToClient);
		}
	}
		
}