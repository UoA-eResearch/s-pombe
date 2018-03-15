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
    public GameObject infoPrefab;
	private MaterialPropertyBlock[] materials;
	private Quaternion[] rotations;
	public Color32[] colorsSpheresOff;
	public Color32[] colorsWeights;
	public Color32[] colorsSpheresOn;
	public GameObject menu;
	public GameObject togglePrefab;
	private List<GameObject> spheres = new List<GameObject>();
	private List<GameObject> genes = new List<GameObject>();
	private List<string> allGeneTags = new List<string>();
	private List<string> changingDropdownGeneTags = new List<string>();
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
			sphere.name = i.ToString() + " Chrom: " + c;
			numberOfChromosomes [c] = numberOfChromosomes [c] + 1;
			sphere.transform.localPosition = new Vector3(x, y, z);
			sphere.GetComponent<Renderer>().SetPropertyBlock(materials[c]);

            var info = Instantiate(infoPrefab, transform);
            info.transform.SetParent(sphere.transform);
            info.transform.localPosition = new Vector3(0, 1, 0);
            info.SetActive(false);

            spheres.Add(sphere);
		}

		Debug.Log ("Count of spheres: " + spheres.Count);
	}

	public string LoadGeneText(string sphName){

        var sphereObject = GameObject.Find(sphName);

		
		var sphereNumber = Int32.Parse(sphereObject.name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);
        Debug.Log("Sphere number: " + sphereNumber);
        string newString = null;

        for (var gene = 1; gene < genesChrom1.Count; gene++)
		{
            
            List<string> geneContent = genesChrom1.ElementAt(gene).Value;
			var tagElements = genesChrom1.ElementAt(gene).Key.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var geneText = new string[geneContent.Count];

            if (Int32.Parse(tagElements[0]) == sphereNumber || Int32.Parse(tagElements[1]) == sphereNumber) {

                for (var entry = 0; entry < geneContent.Count; entry++)
				{
					geneText[entry] = geneContent.ElementAt(entry);
				}
                newString = string.Join(" ", geneText);
            }
        }
        return newString;
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

				if (genes.First ().Key.Contains("Chromosome 1")) {}
				else if(genes.First ().Key.Contains("Chromosome 2")) {
					fromNum = numberOfChromosomes[0] + fromNum;
					toNum = numberOfChromosomes[0] + toNum;
				}else if(genes.First ().Key.Contains("Chromosome 3")) {
					fromNum = numberOfChromosomes[0] + numberOfChromosomes[1] + fromNum;
					toNum = numberOfChromosomes[0] + numberOfChromosomes[1] + toNum;
				}

				Debug.Log (fromNum + " " + genes.First().Key);
				var location = spheres [fromNum + 1].transform.position;

				var startPos = fromNum - 1;
				var endPos = toNum + 1;
				if (startPos < 0) {
					startPos = 0;
				}
				if (endPos > spheres.Count) {
					endPos = spheres.Count;
				}
					//location = Vector3.Lerp (spheres [fromNum + 1].transform.position, spheres [fromNum - 1].transform.position, 0.5f);
					//location = CalculateCubicBezierPoint (i, spheres [fromNum - 1].transform.position, spheres [fromNum + 1].transform.position, spheres [fromNum - 1].transform.position, spheres [fromNum + 1].transform.position);
					Vector3 posSphereFirst = spheres [fromNum].transform.position;
					Vector3 posBeforeFirst = spheres [startPos].transform.position;
					Vector3 posAfterFirst = spheres [fromNum + 1].transform.position;

					Vector3 startPoint = CalculateCubicBezierPoint (0.5f, posBeforeFirst, posSphereFirst, posBeforeFirst, posSphereFirst);

					Vector3 posSphereLast = spheres [toNum].transform.position;
					Vector3 posBeforeLast = spheres [toNum - 1].transform.position;
					Vector3 posAfterLast = spheres [endPos].transform.position;

					Vector3 endPoint = CalculateCubicBezierPoint (0.5f, posSphereLast, posAfterLast, posSphereLast, posAfterLast);

					var direction = posAfterLast;
					var geneName = string.Concat (fromNum.ToString () + " - " + toNum.ToString ());
					InstantiateGene (startPoint, endPoint, fromNum, direction, geneName);

					toggleSpheresOff ();
				
			}
		}
	}

	void toggleSpheresOff(){

		foreach (var sphere in spheres) {

			var numChrom = Int32.Parse(sphere.name.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);

			var propBlock = new MaterialPropertyBlock();
			var r = colorsSpheresOff[numChrom].r;
			var g = colorsSpheresOff[numChrom].g;
			var b = colorsSpheresOff[numChrom].b;

			propBlock.SetColor("_Color", new Color32(r, g, b, 10));
			materials [numChrom] = propBlock;
			sphere.GetComponent<Renderer> ().SetPropertyBlock (materials [numChrom]);
		}

	}

	void toggleSpheresOn(){

		foreach (var sphere in spheres) {

			var numChrom = Int32.Parse(sphere.name.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);

			var propBlock = new MaterialPropertyBlock();
			var r = colorsSpheresOn[numChrom].r;
			var g = colorsSpheresOn[numChrom].g;
			var b = colorsSpheresOn[numChrom].b;

			propBlock.SetColor("_Color", new Color32(r, g, b, 255));
			materials [numChrom] = propBlock;
			sphere.GetComponent<Renderer> ().SetPropertyBlock (materials [numChrom]);
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

					var direction = posAfterLast;
					var geneName = string.Concat (fromNum.ToString () + " - " + toNum.ToString ());
					InstantiateGene (startPoint, endPoint, fromNum, direction, geneName);
				}
			}
		}
	}

	private void InstantiateGene(Vector3 inputPosA, Vector3 inputPosB, int indexFirstSphere, Vector3 direction, String name) 
	{
		float Ancho = 40.0f;
		float Alto = 40.0f;

		Vector3 posC = ((inputPosB - inputPosA) * 0.5F ) + inputPosA;
		//posC = posC + new Vector3 (20, 20, 20);
		float lengthC = (inputPosB - inputPosA).magnitude; 
		float sineC= ( inputPosB.y - inputPosA.y ) / lengthC; 
		float angleC = Mathf.Asin( sineC ) * Mathf.Rad2Deg; 
		if (inputPosB.x < inputPosA.x) {angleC = 0 - angleC;} 

		Debug.Log( "inputPosA" + inputPosA + " : inputPosB" + inputPosB + " : posC" + posC + " : lengthC " + lengthC + " : sineC " + sineC + " : angleC " + angleC );

		GameObject gene = Instantiate( genePrefab, transform);
		gene.transform.localScale = new Vector3(Alto, Ancho, lengthC);
		gene.transform.localPosition = posC;
		//gene.transform.localScale = new Vector3(direction.x, 0, 10);
		gene.transform.rotation = Quaternion.Euler(0, 0, angleC);
		gene.transform.LookAt (direction);
		gene.name = "Gene " + name;
		//gene.GetComponent<Renderer> ().SetPropertyBlock (materials [4]);
		//gene.GetComponent<Renderer> ().material.SetColor("_Color", new Color(0.01176471f, 0.9215686f, 0.6705883f, 0.9f));
		genes.Add (gene);
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
		red.SetColor("_Color", colorsSpheresOn[0]);
		var green = new MaterialPropertyBlock();
		green.SetColor("_Color", colorsSpheresOn[1]);
		var blue = new MaterialPropertyBlock();
		blue.SetColor("_Color", colorsSpheresOn[2]);

		var peachRed = new MaterialPropertyBlock();
		peachRed.SetColor("_Color", colorsWeights[0]);
		var peach = new MaterialPropertyBlock();
		peach.SetColor("_Color", colorsWeights[1]);
		var peachBrown = new MaterialPropertyBlock();
		peachBrown.SetColor("_Color", colorsWeights[2]);
		var dullBrown = new MaterialPropertyBlock();
		dullBrown.SetColor("_Color", colorsWeights[3]);
		var yellowBrown = new MaterialPropertyBlock();
		yellowBrown.SetColor("_Color", colorsWeights[4]);

		var magenta = new MaterialPropertyBlock();
		magenta.SetColor("_Color", colorsWeights[5]);
		var darkRed = new MaterialPropertyBlock();
		darkRed.SetColor("_Color", colorsWeights[6]);

		var lightTurquoise = new MaterialPropertyBlock();
		lightTurquoise.SetColor("_Color", colorsWeights[7]);
		var turquoise = new MaterialPropertyBlock();
		turquoise.SetColor("_Color", colorsWeights[8]);

		var lightPurple = new MaterialPropertyBlock();
		lightPurple.SetColor("_Color", colorsWeights[9]);
		var purple = new MaterialPropertyBlock();
		purple.SetColor("_Color", colorsWeights[10]);
		var bluePurple = new MaterialPropertyBlock();
		bluePurple.SetColor("_Color", colorsWeights[11]);
		var redPurple = new MaterialPropertyBlock();
		redPurple.SetColor("_Color", colorsWeights[12]);

		var brightGreen = new MaterialPropertyBlock();
		brightGreen.SetColor("_Color", colorsWeights[13]);
		var dullGreen = new MaterialPropertyBlock();
		dullGreen.SetColor("_Color", colorsWeights[14]);


		var brightBlue = new MaterialPropertyBlock();
		brightBlue.SetColor("_Color", colorsWeights[15]);
		var dullBlue = new MaterialPropertyBlock();
		dullBlue.SetColor("_Color", colorsWeights[16]);
		var darkBlue = new MaterialPropertyBlock();
		darkBlue.SetColor("_Color", colorsWeights[17]);

		var lightYellow = new MaterialPropertyBlock();
		lightYellow.SetColor("_Color", colorsWeights[18]);
		var orange = new MaterialPropertyBlock();
		orange.SetColor("_Color", colorsWeights[19]);


		var lightGreen = new MaterialPropertyBlock();
		lightGreen.SetColor("_Color", colorsWeights[20]);
		var neonGreen = new MaterialPropertyBlock();
		neonGreen.SetColor("_Color", colorsWeights[21]);
		var grassGreen = new MaterialPropertyBlock();
		grassGreen.SetColor("_Color", colorsWeights[22]);
		var darkGreen = new MaterialPropertyBlock();
		darkGreen.SetColor("_Color", colorsWeights[23]);

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
			toggle.GetComponentInChildren<Text>().color = colorsWeights[count];
			//toggle.GetComponentInChildren<CanvasRenderer>().SetColor(colors[count]);
			toggle.name = w.name;
			toggle.transform.localPosition = new Vector3(-270, y-80, 0);
			toggles.Add(toggle);
			y -= 30;
			markers.Add(w.name, new List<GameObject>());
			count += 1;
		}

		genesChrom1.Add ("Chromosome 1", null);
		genesChrom2.Add ("Chromosome 2", null);
		genesChrom3.Add ("Chromosome 3", null);

		ReadGeneFiles (genesChrom1, 0);
		ReadGeneFiles (genesChrom2, 1);
		ReadGeneFiles (genesChrom3, 2);

		InstantiateDropdownList (genesChrom1);
		InstantiateDropdownList (genesChrom2);
		InstantiateDropdownList (genesChrom3);

		LoadDropdownGeneTags ();

        LoadFromDat();

        //LoadGenesByClickSphere (genesChrom1, 10);
        //LoadGenesByClickSphere (genesChrom2, 10);
        //LoadGenesByClickSphere (genesChrom3, 10);
    }

	private void InstantiateDropdownList(Dictionary<string, List<string>> genes){
		allGeneTags.Add ("No Tag Selected");
		foreach (var gene in genes) {
			if (gene.Value != null) {
				allGeneTags.Add (gene.Key);
			}
		}
		changingDropdownGeneTags.AddRange (allGeneTags);
	}

	private void LoadDropdownGeneTags(){
		var dropdown = GameObject.Find("GeneTags").GetComponent<Dropdown> ();
		dropdown.options.Clear ();
		var count = 0;
		foreach (string tag in changingDropdownGeneTags) {
			count++;
			if (count > 20) {
				break;
			}
			dropdown.options.Add (new Dropdown.OptionData (tag.ToString()));
		}
	}

	public void DropdownInputChanged(int inputChoice){
		searchString = changingDropdownGeneTags.ElementAt (inputChoice);

		foreach (var gene in genes) {
			Destroy (gene);
			toggleSpheresOn ();
		}

		LoadGenesByString (genesChrom1);
		LoadGenesByString (genesChrom2);
		LoadGenesByString (genesChrom3);
	}

	public void SearchInputChanged(string inputText){
		
		changingDropdownGeneTags = new List<string> ();
		changingDropdownGeneTags.Add ("No Tag Selected");
		foreach (var tag in allGeneTags) {
			if (tag.Contains (inputText)) {
				changingDropdownGeneTags.Add (tag);
			}
		}

		LoadDropdownGeneTags ();
	}

	// Update is called once per frame
	void Update () {

	}
}