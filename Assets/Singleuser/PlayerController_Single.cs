using UnityEngine;

public class PlayerController_Single : MonoBehaviour
{
	public GameObject menu;
	void ViveControl(int controllerId)
	{
		var controller = SteamVR_Controller.Input(controllerId);
		if (controller.GetPress(SteamVR_Controller.ButtonMask.Trigger))
		{
			var v = controller.velocity;
			v.Scale(transform.localScale);
			transform.position += v * 10;
			transform.Rotate(controller.angularVelocity, Space.World);
		}
        /*
		if (controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
		{
			var s = controller.GetAxis().y;
			float scale = 1.05f;
			if (s < 0)
			{
				scale = .95f;
			}
			transform.localScale *= scale;
		}*/
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
		var leftI = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
		var rightI = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
		if (leftI == rightI)
		{
			// Single Controller
			rightI = -1;
		}

		if (leftI != -1)
		{
			ViveControl(leftI);
		}

		if (rightI != -1)
		{
			ViveControl(rightI);
		}
	}
}
