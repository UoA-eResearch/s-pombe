using UnityEngine;
using System.Collections;

namespace TalesFromTheRift
{
	public class OpenCanvasKeyboard : MonoBehaviour 
	{
		// Canvas to open keyboard under
		public Canvas CanvasObject;
		public GameObject CanvasKeyboardObject;

		// Optional: Input Object to receive text 
		public GameObject inputObject;

		public void OpenKeyboard() 
		{
			CanvasKeyboardObject.SetActive (true);
			CanvasKeyboardObject.GetComponent<CanvasKeyboard> ().inputObject = inputObject;
			//CanvasKeyboard.Open(CanvasObject, inputObject != null ? inputObject : gameObject);
		}

		public void CloseKeyboard() 
		{	
			//TalesFromTheRift.CanvasKeyboard kb =  CanvasKeyboardObject.FindObjectOfType<CanvasKeyboard>();

			//CanvasKeyboard.Close ();
			CanvasKeyboardObject.SetActive (false);
			//CanvasKeyboard.Close ();
		}
	}
}