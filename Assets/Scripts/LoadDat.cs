using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using UnityEngine.UI;

public class LoadDat : NetworkBehaviour {

	//[SyncVar]
	public int index;
	TextAsset[] structures;
	TextAsset[] weights;
	public GameObject spherePrefab;
	public GameObject markerPrefab;
	private MaterialPropertyBlock[] materials;
	private Quaternion [] rotations;
	public Color32[] colors;
	public GameObject menuPrefab;
	public GameObject togglePrefab;
	public List<GameObject> spheres = new List<GameObject>();
	public Dictionary<string, List<GameObject>> markers = new Dictionary<string, List<GameObject>>();
	public List<GameObject> toggles = new List<GameObject>();


	[ClientRpc]
	public void RpcLoadFromDat()
	{
		index = GameObject.FindGameObjectWithTag ("LocalPlayer").GetComponent<DNASpawner> ().getIndex ();

		gameObject.tag = "DNA";
		structures = Resources.LoadAll<TextAsset>("Structures/");
		Debug.Log(structures.Length + " structures " + "index" + index);

		InitColors();
		InitRotations();

		foreach (GameObject go in spheres) Destroy(go);
		spheres = new List<GameObject>();
		print (index);
		print (structures.Length);
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



	[ClientRpc]
	public void RpcSpawnMenu(){

		var menu = GameObject.Find ("Menu(Clone)");
		menu.tag = "Menu";
		menu.transform.localPosition = new Vector3(0, 0, 8);
		weights = Resources.LoadAll<TextAsset>("Weights/");

		colors = gameObject.GetComponent<LoadDat> ().getColors ();

		var y = menu.GetComponent<RectTransform>().rect.height / 2 - 30;
		var count = 0;
		foreach (var w in weights)
		{
			Color color = colors[count];
			var toggle = Instantiate(togglePrefab, menu.transform);
			toggle.GetComponentInChildren<Text>().text = w.name;
			toggle.GetComponentInChildren<Text>().color = color;
			toggle.name = w.name;
			toggle.transform.localPosition = new Vector3(0, y, 0);
			toggles.Add(toggle);
			y -= 30;
			markers.Add(w.name, new List<GameObject>());
			count += 1;
		}
	}

	[ClientRpc]
	public void RpcAssignButton(GameObject menu){
		Debug.Log ("In Assign Button");
		Button nextBut = menu.GetComponentInChildren<Button> ();
		nextBut.onClick.AddListener (DeleteDNA);
	}

	public void DeleteDNA()
	{
		Debug.Log("You have clicked the button!" + index);
		CmdDelete ();
	}


	[Command]
	public void CmdDelete(){
		Debug.Log ("In CmdDelete");
		RpcDelete ();
	}

	[ClientRpc]
	public void RpcDelete(){
		Debug.Log ("dna object valid?" + this.gameObject);
		NetworkServer.Destroy (this.gameObject);
		GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<DNASpawner> ().InstantiateDNA();
	}


	[Command]
	public void CmdToggle(bool on, string name)
	{
		var dna = GameObject.FindGameObjectWithTag ("DNA");
		var dat = dna.GetComponent<LoadDat>();

		if (on)
		{
			Debug.Log ("In toggle handler on");
			dat.RpcLoadWeight(name);
		}
		else
		{
			Debug.Log("In toggle handler off");
			dat.RpcRemoveWeight (name);
		}
	}

	[Command]
	public void CmdLoadFromDat(){
		RpcLoadFromDat ();
	}


	[Command]
	public void CmdLoadWeight(String name)
	{
		RpcLoadWeight (name);
	}

	public Color32[] getColors(){
		return colors;
	}
		


	[ClientRpc]
	public void RpcLoadWeight(String name)
	{
		
		TextAsset[] weights = Resources.LoadAll<TextAsset>("Weights/");
		Debug.Log(weights.Length + " weights");
		var numChoice = 0;
		var match = "";

		foreach (var w in weights)
		{
			if (w.name == name)
			{
				match = w.text;
				break;
			}
			numChoice++;
		}
		if (match == "")
		{
			Debug.LogError(name + " is not a valid weight filename");
			return;
		}
		var lines = match.Split('\n');
		var ints = lines.Where(s => s != "").Select(s => Convert.ToInt16(s)).ToList();
		Debug.Log("min: " + ints.Min() + ", max: " + ints.Max());


		var rot = rotations[numChoice];
		var mat = materials[numChoice + 3];

		for (int i = 0; i < ints.Count; i++)
		{
			var sphere = spheres[i];
			var w = ints[i];
			if (w > 0)
			{
				var marker = Instantiate(markerPrefab, sphere.transform);
				marker.name = name;
				marker.tag = "Marker";
				marker.transform.localPosition = new Vector3(0, 0, 0);
				marker.transform.rotation = rot;
				marker.transform.localScale = new Vector3(.1f, w / 10f, .1f);
				marker.GetComponent<Renderer>().SetPropertyBlock(mat);
				gameObject.GetComponent<LoadDat>().markers[name].Add(marker);
			}
		}
	}


	[ClientRpc]
	public void RpcRemoveWeight(String name){
		var goMarkers = gameObject.GetComponent<LoadDat> ().markers [name];
		foreach (var go in goMarkers) Destroy(go);
		gameObject.GetComponent<LoadDat> ().markers [name] = new List<GameObject>();
	}

	void InitColors()
	{
		var red = new MaterialPropertyBlock();
		red.SetColor("_Color", Color.red);
		var green = new MaterialPropertyBlock();
		green.SetColor("_Color", Color.green);
		var blue = new MaterialPropertyBlock();
		blue.SetColor("_Color", Color.blue);

		colors = new Color32[] { new Color(0.8705882f, 0.3294118f, 0.3294118f, 1), new Color(0.7568628f, 0.3960784f, 0.4509804f, 1), new Color(0.6941177f, 0.4784314f, 0.4509804f, 1), new Color(0.627451f, 0.5176471f, 0.454902f, 1), new Color(0.4745098f, 0.3058824f, 0.07450981f, 1),
			new Color(1f, 0f, 1f, 1), new Color(0.3882353f, 0.1137255f, 0.1803922f, 1), new Color(0.6941177f, 0.9607843f, 0.9019608f, 1), new Color(0.7568628f, 0.3960784f, 0.4509804f, 1), new Color(0.6392157f, 0.4901961f, 0.8666667f, 1), new Color(0.2470588f, 0.07843138f, 0.4470588f, 1),
			new Color(0.3411765f, 0.08627451f, 0.8627451f, 1), new Color(0.3176471f, 0.03529412f, 0.3058824f, 1), new Color(0.01176471f, 0.9215686f, 0.6705883f, 1), new Color(0.09803922f, 0.6509804f, 0.5803922f, 1), new Color(0.09019608f, 0.6196079f, 0.854902f, 1), new Color(0.2078431f, 0.2588235f, 0.4862745f, 1),
			new Color(0.1294118f, 0.2470588f, 0.4078431f, 1), new Color(0.9490196f, 0.9058824f, 0.427451f, 1), new Color(0.9568627f, 0.6901961f, 0.2862745f, 1), new Color(0.5921569f, 0.9647059f, 0.3215686f, 1), new Color(0.172549f, 0.8588235f, 0.3333333f, 1), new Color(0.3019608f, 0.627451f, 0.2117647f, 1), new Color(0.1568628f, 0.2392157f, 0.007843138f, 1)};

		var peachRed = new MaterialPropertyBlock();
		peachRed.SetColor("_Color", colors[0]);
		var peach = new MaterialPropertyBlock();
		peach.SetColor("_Color", colors[1]);
		var peachBrown = new MaterialPropertyBlock();
		peachBrown.SetColor("_Color", colors[2]);
		var dullBrown = new MaterialPropertyBlock();
		dullBrown.SetColor("_Color", colors[3]);
		var yellowBrown = new MaterialPropertyBlock();
		yellowBrown.SetColor("_Color", colors[4]);

		var magenta = new MaterialPropertyBlock();
		magenta.SetColor("_Color", colors[5]);
		var darkRed = new MaterialPropertyBlock();
		darkRed.SetColor("_Color", colors[6]);

		var lightTurquoise = new MaterialPropertyBlock();
		lightTurquoise.SetColor("_Color", colors[7]);
		var turquoise = new MaterialPropertyBlock();
		turquoise.SetColor("_Color", colors[8]);

		var lightPurple = new MaterialPropertyBlock();
		lightPurple.SetColor("_Color", colors[9]);
		var purple = new MaterialPropertyBlock();
		purple.SetColor("_Color", colors[10]);
		var bluePurple = new MaterialPropertyBlock();
		bluePurple.SetColor("_Color", colors[11]);
		var redPurple = new MaterialPropertyBlock();
		redPurple.SetColor("_Color", colors[12]);

		var brightGreen = new MaterialPropertyBlock();
		brightGreen.SetColor("_Color", colors[13]);
		var dullGreen = new MaterialPropertyBlock();
		dullGreen.SetColor("_Color", colors[14]);


		var brightBlue = new MaterialPropertyBlock();
		brightBlue.SetColor("_Color", colors[15]);
		var dullBlue = new MaterialPropertyBlock();
		dullBlue.SetColor("_Color", colors[16]);
		var darkBlue = new MaterialPropertyBlock();
		darkBlue.SetColor("_Color", colors[17]);

		var lightYellow = new MaterialPropertyBlock();
		lightYellow.SetColor("_Color", colors[18]);
		var orange = new MaterialPropertyBlock();
		orange.SetColor("_Color", colors[19]);


		var lightGreen = new MaterialPropertyBlock();
		lightGreen.SetColor("_Color", colors[20]);
		var neonGreen = new MaterialPropertyBlock();
		neonGreen.SetColor("_Color", colors[21]);
		var grassGreen = new MaterialPropertyBlock();
		grassGreen.SetColor("_Color", colors[22]);
		var darkGreen = new MaterialPropertyBlock();
		darkGreen.SetColor("_Color", colors[23]);

		materials = new MaterialPropertyBlock[] { red, green, blue, peachRed, peach, peachBrown, dullBrown, yellowBrown, magenta, darkRed, lightTurquoise, turquoise, lightPurple, purple, bluePurple, redPurple, brightGreen,
			dullGreen, brightBlue, dullBlue, darkBlue, lightYellow, orange, lightGreen, neonGreen, grassGreen, darkGreen };
	}


	void InitRotations() {
		rotations = new Quaternion[] {new Quaternion(-0.1f, -0.4f, -0.7f, 0.5f), new Quaternion(-0.5f, -0.5f, 0.2f, 0.7f), new Quaternion(-0.2f, 0.8f, 0.3f, 0.4f), new Quaternion(-0.7f, 0.7f, -0.2f, 0.1f), new Quaternion(0.3f, -0.3f, -0.5f, 0.8f), new Quaternion(0.7f, 0.3f, 0.1f, 0.7f), new Quaternion(0.6f, -0.4f, 0.7f, 0.1f),
			new Quaternion(-0.6f, 0.3f, 0.1f, 0.7f), new Quaternion(0.4f, 0.6f, 0.7f, 0.1f), new Quaternion(0.2f, -0.2f, -0.8f, 0.5f), new Quaternion(-0.7f, -0.6f, -0.3f, 0.1f), new Quaternion(0.7f, -0.4f, -0.3f, 0.5f), new Quaternion(-0.5f, -0.2f, 0.6f, 0.6f), new Quaternion(0.4f, -0.8f, 0.3f, 0.3f), new Quaternion(-0.9f, 0.0f, -0.2f, 0.2f), new Quaternion(0.5f, 0.6f, 0.6f, 0.2f), new Quaternion(0.9f, 0.3f, 0.3f, 0.1f),
			new Quaternion(-0.2f, -0.7f, -0.7f, 0.3f), new Quaternion(-0.3f, 0.9f, 0.2f, 0.3f), new Quaternion(0.3f, 0.6f, 0.4f, 0.6f), new Quaternion(-0.6f, 0.5f, -0.6f, 0.3f), new Quaternion(0.4f, 0.0f, 0.5f, 0.8f), new Quaternion(0.1f, 0.5f, -0.6f, 0.7f), new Quaternion(-0.6f, -0.7f, 0.1f, 0.4f)};

	}
		


	// Update is called once per frame
	void Update () {

	}
}