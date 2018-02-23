using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LoadDat_Single : MonoBehaviour {

	public int index = 0;
	TextAsset[] structures;
	TextAsset[] weights;
	TextAsset[] geneFiles;
	public GameObject spherePrefab;
	public GameObject markerPrefab;
	public GameObject genePrefab;
	private MaterialPropertyBlock[] materials;
	private Quaternion[] rotations;
	private Color32[] colors;
	public GameObject menu;
	public GameObject togglePrefab;
	private List<GameObject> spheres = new List<GameObject>();
	public Dictionary<string, List<string>> genesChrom1 = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> genesChrom2 = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> genesChrom3 = new Dictionary<string, List<string>>();
	public Dictionary<string, List<GameObject>> markers = new Dictionary<string, List<GameObject>>();
	private List<GameObject> toggles = new List<GameObject>();
	private string searchString = "";
	private int[] numberOfChromosomes;

	void LoadFromDat()
	{
		foreach (GameObject go in spheres) Destroy(go);
		spheres = new List<GameObject>();
		numberOfChromosomes = new int[]{0, 0, 0};
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
			sphere.name = i.ToString() + "Chrom: " + c;
			numberOfChromosomes [c] = numberOfChromosomes [c] + 1;
			sphere.transform.localPosition = new Vector3(x, y, z);
			sphere.GetComponent<Renderer>().SetPropertyBlock(materials[c]);
			spheres.Add(sphere);
		}
		Debug.Log ("Count of spheres: " + spheres.Count);

		genesChrom1.Add ("Chromosome 1", null);
		genesChrom2.Add ("Chromosome 2", null);
		genesChrom3.Add ("Chromosome 3", null);

		ReadGeneFiles (genesChrom1, 0);
		ReadGeneFiles (genesChrom2, 1);
		ReadGeneFiles (genesChrom3, 2);
		LoadGenesByString (genesChrom1);
		LoadGenesByString (genesChrom2);
		LoadGenesByString (genesChrom3);

		LoadGenesByClickSphere (genesChrom1, 10);
		LoadGenesByClickSphere (genesChrom2, 10);
		LoadGenesByClickSphere (genesChrom3, 10);
	}

	void ReadGeneFiles(Dictionary<string, List<string>> genes, int fileNum){
		geneFiles = Resources.LoadAll<TextAsset>("Genes/");

		var chromosomeFile = geneFiles [fileNum];

		string[] linesChrom1 = chromosomeFile.text.Split ('\n');

		List<string> tempList = new List<string> ();
		string name = "";

		foreach (string line in linesChrom1) {
			
			if (line.StartsWith ("gene ")) {
				var pos = CalculateSpherePosition (line);
				if (pos != null) {

					if (name != "") {
						genes.Add (name, tempList);
						name = "";
					}

					name = pos;
					tempList = new List<string> ();
				}
			}

			tempList.Add (line);

			if (line.Contains ("locus_tag") == true && name.Contains("locus_tag") == false) {
				name = name + " " + line;
			}
			if (line.Contains ("protein_id") == true && name.Contains("protein_id") == false) {
				name = name + " " + line;
			}
			if (line.Contains ("gene_synonym") == true && name.Contains("gene_synonym") == false) {
				name = name + " " + line;
			}
		}
	}

	string CalculateSpherePosition(string line){
		var words = line.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		string locationInfo = words [1];
		var numbers = locationInfo.Split (new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
		string from = "";
		string to = "";
		try {
			from = numbers [0];
			to = numbers [1];

			from = Regex.Match (from, @"\d+").Value;
			to = Regex.Match (to, @"\d+").Value;

			int fromNum = Int32.Parse (from) / 3583;
			int toNum = Int32.Parse (to) / 3583;

			string pos = fromNum + " " + toNum;
			return pos;
		} catch (Exception e) {return null;}
	}


	void LoadGenesByString(Dictionary<string, List<string>> genes){

		foreach (var dictEntry in genes) {
			if (dictEntry.Key.Contains (searchString) || searchString == "") {

				if(dictEntry.Key.Contains("Chromosome")){continue;}

				var words = dictEntry.Key.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

				var fromNum = Int32.Parse(words [0]);
				var toNum = Int32.Parse(words [1]);

				Debug.Log (fromNum + " " + genes.First().Key);

				if (genes.First ().Key == "Chromosome 1") {}
				else if(genes.First ().Key == "Chromosome 2") {
					fromNum = numberOfChromosomes[0] + fromNum;
					toNum = numberOfChromosomes[0] + toNum;
				}else if(genes.First ().Key == "Chromosome 3") {
					fromNum = numberOfChromosomes[0] + numberOfChromosomes[1] + fromNum;
					toNum = numberOfChromosomes[0] + numberOfChromosomes[1] + toNum;
				}

				Debug.Log (fromNum + " " + genes.First().Key);
				var location = spheres [fromNum + 1].transform.position;


				if (fromNum > 0) {
					//location = Vector3.Lerp (spheres [fromNum + 1].transform.position, spheres [fromNum - 1].transform.position, 0.5f);
					//location = CalculateCubicBezierPoint (i, spheres [fromNum - 1].transform.position, spheres [fromNum + 1].transform.position, spheres [fromNum - 1].transform.position, spheres [fromNum + 1].transform.position);
					Vector3 posSphereFirst = spheres [fromNum].transform.position;
					Vector3 posBeforeFirst = spheres [fromNum - 1].transform.position;
					Vector3 posAfterFirst = spheres [fromNum + 1].transform.position;

					Vector3 startPoint = CalculateCubicBezierPoint (0.5f, posBeforeFirst, posSphereFirst, posBeforeFirst, posSphereFirst);

					Vector3 posSphereLast = spheres [toNum].transform.position;
					Vector3 posBeforeLast = spheres [toNum - 1].transform.position;
					Vector3 posAfterLast = spheres [toNum + 1].transform.position;

					Vector3 endPoint = CalculateCubicBezierPoint (0.5f, posSphereLast, posAfterLast, posSphereLast, posAfterLast);

					var direction = posAfterLast - posBeforeFirst;
					var geneName = string.Concat (fromNum.ToString () + " - " + toNum.ToString ());
					DrawALine (startPoint, endPoint, fromNum, direction, geneName);
				}
			}
		}
	}

	void LoadGenesByClickSphere(Dictionary<string, List<string>> genes, int sphereNum){

		foreach (var dictEntry in genes) {
			if(dictEntry.Key.Contains("Chromosome")){continue;}
			
			var words = dictEntry.Key.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

			var fromNum = Int32.Parse(words [0]);
			var toNum = Int32.Parse(words [1]);

			if (genes.First ().Key == "Chromosome 1") {}
			else if(genes.First ().Key == "Chromosome 2") {
				fromNum = numberOfChromosomes[0] + fromNum;
				toNum = numberOfChromosomes[0] + toNum;
			}else if(genes.First ().Key == "Chromosome 3") {
				fromNum = numberOfChromosomes[0] + numberOfChromosomes[1] + fromNum;
				toNum = numberOfChromosomes[0] + numberOfChromosomes[1] + toNum;
			}

			if (sphereNum >= fromNum && sphereNum <= toNum) {
				var location = spheres [fromNum + 1].transform.position;


				if (fromNum > 0) {
					//location = Vector3.Lerp (spheres [fromNum + 1].transform.position, spheres [fromNum - 1].transform.position, 0.5f);
					//location = CalculateCubicBezierPoint (i, spheres [fromNum - 1].transform.position, spheres [fromNum + 1].transform.position, spheres [fromNum - 1].transform.position, spheres [fromNum + 1].transform.position);
					Vector3 posSphereFirst = spheres [fromNum].transform.position;
					Vector3 posBeforeFirst = spheres [fromNum - 1].transform.position;
					Vector3 posAfterFirst = spheres [fromNum + 1].transform.position;

					Vector3 startPoint = CalculateCubicBezierPoint (0.5f, posBeforeFirst, posSphereFirst, posBeforeFirst, posSphereFirst);

					Vector3 posSphereLast = spheres [toNum].transform.position;
					Vector3 posBeforeLast = spheres [toNum - 1].transform.position;
					Vector3 posAfterLast = spheres [toNum + 1].transform.position;

					Vector3 endPoint = CalculateCubicBezierPoint (0.5f, posSphereLast, posAfterLast, posSphereLast, posAfterLast);

					var direction = posAfterLast - posBeforeFirst;
					var geneName = string.Concat (fromNum.ToString () + " - " + toNum.ToString ());
					DrawALine (startPoint, endPoint, fromNum, direction, geneName);
				}
			}
		}
	}

	private void DrawALine(Vector3 inputPosA, Vector3 inputPosB, int indexFirstSphere, Vector3 direction, String name) 
	{
		float Ancho = 0.0f;
		float Alto = 2.0f;
		Vector3 result;

		Vector3 posC = ((inputPosB - inputPosA) * 0.5F ) + inputPosA;
		posC = posC + new Vector3 (0, 0, 0);
		float lengthC = (inputPosB - inputPosA).magnitude; 
		float sineC= ( inputPosB.y - inputPosA.y ) / lengthC; 
		float angleC = Mathf.Asin( sineC ) * Mathf.Rad2Deg; 
		if (inputPosB.x < inputPosA.x) {angleC = 0 - angleC;} 

		Debug.Log( "inputPosA" + inputPosA + " : inputPosB" + inputPosB + " : posC" + posC + " : lengthC " + lengthC + " : sineC " + sineC + " : angleC " + angleC );

		GameObject gene = Instantiate( genePrefab, spheres[indexFirstSphere].transform.position, Quaternion.identity);
		//gene.transform.localScale = new Vector3(lengthC, Ancho, Alto);
		gene.transform.localPosition = inputPosA;
		gene.transform.localScale = new Vector3(direction.x, 0, 10);
		gene.transform.rotation = Quaternion.Euler(0, 0, angleC);
		gene.transform.LookAt (direction);
		gene.name = name;
		gene.GetComponent<Renderer> ().SetPropertyBlock (materials [4]);
		//genes.Add (gene);
	}

	private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;

		Vector3 p = uuu * p0; 
		p += 3 * uu * t * p1; 
		p += 3 * u * tt * p2; 
		p += ttt * p3; 

		return p;
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
				marker.transform.localPosition = new Vector3(0, 0, 0);
				marker.transform.rotation = rot;
				marker.transform.localScale = new Vector3(.1f, w / 10f, .1f);
				marker.GetComponent<Renderer>().SetPropertyBlock(mat);
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

	// Use this for initialization
	void Start () {
		structures = Resources.LoadAll<TextAsset>("Structures/");
		weights = Resources.LoadAll<TextAsset>("Weights/");
		InitColors();
		InitRotations();
		Debug.Log(structures.Length + " structures " + weights.Length + " weights");

		// Setup menu
		var y = menu.GetComponent<RectTransform>().rect.height / 2 - 30;
		var count = 0;

		foreach (var w in weights)
		{
			var toggle = Instantiate(togglePrefab, menu.transform);
			toggle.GetComponentInChildren<Text>().text = w.name;
			toggle.GetComponentInChildren<Text>().color = colors[count];
			//toggle.GetComponentInChildren<CanvasRenderer>().SetColor(colors[count]);
			toggle.name = w.name;
			toggle.transform.localPosition = new Vector3(0, y, 0);
			toggles.Add(toggle);
			y -= 30;
			markers.Add(w.name, new List<GameObject>());
			count += 1;
		}
		LoadFromDat();
	}

	// Update is called once per frame
	void Update () {

	}
}