using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class LoadDat_Singleuser : MonoBehaviour {

	public int index = 0;
	TextAsset[] structures;
	TextAsset[] weights;
	public GameObject spherePrefab;
	public GameObject markerPrefab;
	private MaterialPropertyBlock[] materials;
	public GameObject menu;
	public GameObject togglePrefab;
	private List<GameObject> spheres = new List<GameObject>();
	public Dictionary<string, List<GameObject>> markers = new Dictionary<string, List<GameObject>>();
	private List<GameObject> toggles = new List<GameObject>();

	void LoadFromDat()
	{
		foreach (GameObject go in spheres) Destroy(go);
		spheres = new List<GameObject>();
		var lines = structures[index].text.Split('\n');
		for (int i = 0; i < lines.Length; i++)
		{
			var l = lines[i];
			if (l.Length == 0) continue;
			var bits = l.Split();
			var c = int.Parse(bits[0]);
			var x = float.Parse(bits[1]);
			var y = float.Parse(bits[2]);
			var z = float.Parse(bits[3]);
			var sphere = Instantiate(spherePrefab, transform);
			sphere.name = i.ToString();
			sphere.transform.localPosition = new Vector3(x, y, z);
			sphere.GetComponent<Renderer>().SetPropertyBlock(materials[c]);
			spheres.Add(sphere);
		}
	}

	public void LoadNext()
	{
		index++;
		LoadFromDat();
		foreach (var t in toggles)
		{
			if (t.GetComponent<Toggle>().isOn)
			{
				LoadWeight(t.name);
			}
		}
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
			var sphere = spheres[i];
			var w = ints[i];
			if (w > 0)
			{
				var marker = Instantiate(markerPrefab, sphere.transform);
				marker.name = name;
				marker.transform.localPosition = new Vector3(0, .5f, 0);
				marker.transform.localScale = new Vector3(.1f, w / 10f, .1f);
				marker.GetComponent<Renderer>().SetPropertyBlock(materials[3]);
				markers[name].Add(marker);
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
		// Setup menu
		var y = menu.GetComponent<RectTransform>().rect.height / 2 - 30;
		foreach (var w in weights)
		{
			var toggle = Instantiate(togglePrefab, menu.transform);
			toggle.GetComponentInChildren<Text>().text = w.name;
			toggle.name = w.name;
			toggle.transform.localPosition = new Vector3(0, y, 0);
			toggles.Add(toggle);
			y -= 30;
			markers.Add(w.name, new List<GameObject>());
		}
		LoadFromDat();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
