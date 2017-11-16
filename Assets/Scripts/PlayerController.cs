using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
	
	public GameObject menu;

	void ViveControl(int controllerId)
	{

			Debug.Log ("In vivecontrol now");
			var controller = SteamVR_Controller.Input(controllerId);
			if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
			{

			GameObject player = GameObject.FindGameObjectWithTag ("LocalPlayer");
			NetworkIdentity playerID = player.GetComponent<NetworkIdentity> ();
			CmdSetAuth (netId, playerID);


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

		}

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