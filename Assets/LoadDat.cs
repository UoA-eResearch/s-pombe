using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadDat : MonoBehaviour {

	public int index = 0;
	TextAsset[] structures;
	public GameObject spherePrefab;
	MaterialPropertyBlock red;
	MaterialPropertyBlock green;
	MaterialPropertyBlock blue;

	void LoadFromDat()
	{
		foreach (Transform childTransform in transform) Destroy(childTransform.gameObject);
		var lines = structures[index].text.Split('\n');
		foreach (var l in lines)
		{
			if (l.Length == 0) continue;
			var bits = l.Split();
			var c = bits[0];
			var x = float.Parse(bits[1]);
			var y = float.Parse(bits[2]);
			var z = float.Parse(bits[3]);
			var sphere = Instantiate(spherePrefab);
			sphere.transform.parent = transform;
			sphere.transform.localPosition = new Vector3(x, y, z);
			sphere.transform.localScale = Vector3.one * 15;
			var renderer = sphere.GetComponent<MeshRenderer>();
			switch (c)
			{
				case "0":
					renderer.SetPropertyBlock(red);
					break;
				case "1":
					renderer.SetPropertyBlock(blue);
					break;
				case "2":
					renderer.SetPropertyBlock(green);
					break;
				default:
					break;
			}
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
		red = new MaterialPropertyBlock();
		red.SetColor("_Color", new Color(1, 0, 0));
		green = new MaterialPropertyBlock();
		green.SetColor("_Color", new Color(0, 1, 0));
		blue = new MaterialPropertyBlock();
		blue.SetColor("_Color", new Color(0, 0, 1));
		Debug.Log(structures.Length);
		LoadFromDat();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
