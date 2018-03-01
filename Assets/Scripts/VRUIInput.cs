using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class VRUIInput : MonoBehaviour
{
	private SteamVR_LaserPointer laserPointer;
	private SteamVR_TrackedController trackedController;

	private void OnEnable()
	{
		laserPointer = GetComponent<SteamVR_LaserPointer>();
		laserPointer.PointerIn -= HandlePointerIn;
		laserPointer.PointerIn += HandlePointerIn;
		laserPointer.PointerOut -= HandlePointerOut;
		laserPointer.PointerOut += HandlePointerOut;

		trackedController = GetComponent<SteamVR_TrackedController>();
		if (trackedController == null)
		{
			trackedController = GetComponentInParent<SteamVR_TrackedController>();
		}
		trackedController.TriggerClicked -= HandleTriggerClicked;
		trackedController.TriggerClicked += HandleTriggerClicked;
	}


	private void HandleTriggerClicked(object sender, ClickedEventArgs e)
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
            if (EventSystem.current.currentSelectedGameObject.GetComponent("Scrollbar") != null)
            {
                EventSystem.current.currentSelectedGameObject.GetComponent<Scrollbar>().value += .1f;
            }
			ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
		}
	}

	private void HandlePointerIn(object sender, PointerEventArgs e)
	{
		var button = e.target.GetComponent<Button>();
		if (button != null)
		{
			button.Select();
			Debug.Log("HandlePointerIn", e.target.gameObject);
		}
		var toggle = e.target.GetComponent<Toggle>();
		if (toggle != null)
		{
			toggle.Select();
			Debug.Log("HandlePointerIn", e.target.gameObject);
		}
		var textInput = e.target.GetComponent<InputField>();
		if (textInput != null)
		{
			textInput.Select ();
			Debug.Log("HandlePointerIn text input", e.target.gameObject);
		}
		var dropdown = e.target.GetComponent<Dropdown>();
		if (dropdown != null)
		{
            dropdown.Select ();
            Debug.Log("HandlePointerIn dropdown", e.target.gameObject);
		}
        var scrollbar = e.target.GetComponent<Scrollbar>();
        if (scrollbar != null)
        {
            Debug.Log("HandlePointerIn Scrollbar", e.target.gameObject);
            scrollbar.Select();
        }
    }

	private void HandlePointerOut(object sender, PointerEventArgs e)
	{

		var button = e.target.GetComponent<Button>();
		var toggle = e.target.GetComponent<Toggle>();
		var textInput = e.target.GetComponent<InputField>();
		var dropdown = e.target.GetComponent<Dropdown>();
        var scrollbar = e.target.GetComponent<Scrollbar>();

        if (button != null || toggle != null || textInput != null || dropdown != null || scrollbar != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			Debug.Log("HandlePointerOut", e.target.gameObject);
		}
    }
}