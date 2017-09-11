using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDat : MonoBehaviour {

	public int index = 0;
	TextAsset[] structures;
	public GameObject spherePrefab;
	public Material[] materials;

	void LoadFromDat()
	{
		foreach (Transform childTransform in transform) Destroy(childTransform.gameObject);
		var lines = structures[index].text.Split('\n');
		Debug.Log(lines.Length);
		foreach (var l in lines)
		{
			if (l.Length == 0) continue;
			var bits = l.Split();
			var c = int.Parse(bits[0]);
			var x = float.Parse(bits[1]);
			var y = float.Parse(bits[2]);
			var z = float.Parse(bits[3]);
			var sphere = Instantiate(spherePrefab);
			sphere.transform.parent = transform;
			sphere.transform.localPosition = new Vector3(x, y, z);
			sphere.transform.localScale = Vector3.one * 30;
			sphere.GetComponent<Renderer>().material = materials[c];
		}
	}

	public void LoadNext()
	{
		index++;
		LoadFromDat();
	}

	// Use this for initialization
	void Start () {
		structures = Resources.LoadAll<TextAsset>("Structures/");
		Debug.Log(structures.Length + " structures");
		foreach (var s in structures)
		{
			Debug.Log(s.name);
		}
		LoadFromDat();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
