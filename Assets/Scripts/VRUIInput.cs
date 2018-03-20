using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class VRUIInput : MonoBehaviour
{
	private SteamVR_LaserPointer laserPointer;
	private SteamVR_TrackedController trackedController;
    GameObject currentSphere = null;
    GameObject tempSphere = null;

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
        
        trackedController.PadClicked -= HandlePadClicked;
        trackedController.PadClicked += HandlePadClicked;
        
    }


    private void HandlePadClicked(object sender, ClickedEventArgs e)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent("Scrollbar") != null)
            {
                if (e.padY < 0)
                {
                    EventSystem.current.currentSelectedGameObject.GetComponent<Scrollbar>().value -= .3f;
                }
                else
                {
                    EventSystem.current.currentSelectedGameObject.GetComponent<Scrollbar>().value += .3f;
                }

            }
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }




    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
	{

        Debug.Log("In trigger");
        if (currentSphere != null)
        {
            //Debug.Log("Sphere name " + currentSphere.name);
            GameObject info = currentSphere.transform.GetChild(0).gameObject;
            if (info.activeInHierarchy)
            {
                info.SetActive(false);
                currentSphere.GetComponent<LoadDat_Single>().RemoveAllGenes(true);
            }
            else
            {
                info.SetActive(true);
                tempSphere = currentSphere;
                
                string geneText = tempSphere.GetComponentInParent<LoadDat_Single>().LoadGeneText(tempSphere.name);

                //string[] lines = geneText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                info.GetComponentInChildren<Text>().text = geneText;
                var scrollbars = info.GetComponentsInChildren<Scrollbar>();

                foreach (var bar in scrollbars) {
                    bar.value = 1.0f;
                }

                //tempSphere = null;

            }
        }

        if (EventSystem.current.currentSelectedGameObject != null)
		{
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
            textInput.Select();
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
        var sphere = e.target.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Debug.Log("HandlePointerIn Sphere"+ e.target.gameObject + " "  + sphere.name);
            currentSphere = sphere.gameObject;
            OnEnable();
        }
    }

	private void HandlePointerOut(object sender, PointerEventArgs e)
	{

		var button = e.target.GetComponent<Button>();
		var toggle = e.target.GetComponent<Toggle>();
		var textInput = e.target.GetComponent<InputField>();
		var dropdown = e.target.GetComponent<Dropdown>();
        var scrollbar = e.target.GetComponent<Scrollbar>();
        var sphere = e.target.GetComponent<SphereCollider>();

        if (button != null || toggle != null || textInput != null || dropdown != null || scrollbar != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
			Debug.Log("HandlePointerOut", e.target.gameObject);
		}
        if (sphere != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            currentSphere = null;
            Debug.Log("HandlePointerOut", e.target.gameObject);
        }
    }
}