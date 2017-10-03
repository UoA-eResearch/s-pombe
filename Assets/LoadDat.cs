using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

public class LoadDat : MonoBehaviour {

	public int index = 0;
	TextAsset[] structures;
	TextAsset[] weights;
	public GameObject spherePrefab;
	public GameObject markerPrefab;
	private MaterialPropertyBlock[] materials;

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
			sphere.GetComponent<Renderer>().SetPropertyBlock(materials[c]);
		}
	}

	public void LoadNext()
	{
		index++;
		LoadFromDat();
	}

	public void LoadWeight(string name)
	{
		var match = "";
		foreach (var w in weights)
		{
			if (w.name == name)
			{
				match = w.text;
			}
		}
		if (match == "")
		{
			Debug.LogError(name + " is not a valid weight filename");
			return;
		}
		var lines = match.Split('\n');
		var ints = lines.Where(s => s != "").Select(s => Convert.ToInt16(s)).ToList();
		Debug.Log("min: " + ints.Min() + ", max: " + ints.Max());
		for (int i = 0; i < ints.Count; i++)
		{
			var sphere = transform.GetChild(i);
			var w = ints[i];
			if (w > 0)
			{
				var marker = Instantiate(markerPrefab);
				marker.transform.parent = sphere;
				marker.transform.localPosition = new Vector3(0, .5f, 0);
				marker.transform.localScale = new Vector3(.1f, w / 10f, .1f);
				marker.GetComponent<Renderer>().SetPropertyBlock(materials[3]);
			}
		}
	}

	void InitColors()
	{
		var red = new MaterialPropertyBlock();
		red.SetColor("_Color", Color.red);
		var green = new MaterialPropertyBlock();
		green.SetColor("_Color", Color.green);
		var blue = new MaterialPropertyBlock();
		blue.SetColor("_Color", Color.blue);
		var magenta = new MaterialPropertyBlock();
		magenta.SetColor("_Color", Color.magenta);
		materials = new MaterialPropertyBlock[] { red, green, blue, magenta };
	}

	// Use this for initialization
	void Start () {
		structures = Resources.LoadAll<TextAsset>("Structures/");
		weights = Resources.LoadAll<TextAsset>("Weights/");
		InitColors();
		Debug.Log(structures.Length + " structures " + weights.Length + " weights");
		LoadFromDat();
		LoadWeight("Weight_H3K4_sum_over_granule");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
