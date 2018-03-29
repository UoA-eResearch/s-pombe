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
	public Color[] colorsSpheresOff;
	public Color32[] colorsWeights;
	public Color[] colorsSpheresOn;
	public GameObject menu;
	public GameObject togglePrefab;
	private List<GameObject> spheres = new List<GameObject>();
	public List<int> genes = new List<int>();
	private List<string> allGeneTags = new List<string>();
	private List<string> changingDropdownGeneTags = new List<string>();
	public Dictionary<string, List<string>> genesChrom1 = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> genesChrom2 = new Dictionary<string, List<string>>();
	public Dictionary<string, List<string>> genesChrom3 = new Dictionary<string, List<string>>();
	public Dictionary<string, List<GameObject>> markers = new Dictionary<string, List<GameObject>>();
	private List<GameObject> toggles = new List<GameObject>();
	private string searchString = "";
	private int[] numberOfChromosomes;
	private List<GameObject> geneObjects = new List<GameObject>();
	List<Dictionary<string, List<string>>> dictionaries = new List<Dictionary<string, List<string>>>();
	private List<GameObject> contentPanels = new List<GameObject>();
	private Dictionary<int, string> geneInfo = new Dictionary<int, string> ();


	////////////////////////////////////////////////////////
	/// GET ADDITIONAL GENE INFORMATION /////////
	///////////////////////////////////////////////////////

	public string LoadGeneText(string sphName){

		var sphereNumber = Int32.Parse(sphName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

		var sphereObject = spheres [sphereNumber];

		string newString = "";

		for (var gene = 1; gene < genesChrom1.Count; gene++)
		{

			List<string> geneContent = genesChrom1.ElementAt(gene).Value;
			var tagElements = genesChrom1.ElementAt(gene).Key.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
			var geneText = new string[geneContent.Count];
			var tempString = "";

			if (Int32.Parse(tagElements[0]) == sphereNumber || Int32.Parse(tagElements[1]) == sphereNumber) {

				for (var entry = 0; entry < geneContent.Count; entry++)
				{
					geneText[entry] = geneContent.ElementAt(entry);
				}
				tempString = string.Join(" ", geneText);
			}
			newString = newString + "\n\n" + tempString;
		}

		return newString;
	}


	////////////////////////////////////////////////////////
	/// GENES /////////
	///////////////////////////////////////////////////////

	public void LoadGenesByClickSphere(int sphereNum){
        activateGlow(sphereNum);
        toggleSpheresOff();
    }

	public void LoadGenesByString(){

		var words = searchString.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		var fromNum = Int32.Parse (words [0]);
		var toNum = Int32.Parse (words [1]);

		for (var i = fromNum; i <= toNum; i++) {
			activateGlow (i);
		}
		toggleSpheresOff ();
	}

	private void activateGlow(int sphereNum){
		
		geneObjects.ElementAt(sphereNum).SetActive(true);
		genes.Add (sphereNum);

		string info = LoadGeneText (spheres [sphereNum].name);
		geneInfo.Add (sphereNum, info);
		PrintGeneInfo ();
	}

	private void PrintGeneInfo(){
		
		foreach (var panel in contentPanels) {
			panel.GetComponentInChildren<Text> ().text = "";
		}
        
		for(var i = 0; i < geneInfo.Count; i++){
			if(i < contentPanels.Count) {
				var info = geneInfo.ElementAt (i);
				contentPanels[i].GetComponentInChildren<Text> ().text = info.Value;

				string pattern = @"(\/locus_tag="".*?\"")";
				MatchCollection matches = Regex.Matches (info.Value, pattern);

				var uniqueMatches = matches
					.OfType<Match>()
					.Select(m => m.Value)
					.Distinct()
					.ToList();

				string locusTags = "";
				foreach (var locusTag in uniqueMatches) {
					locusTags = locusTags + "\n" + locusTag.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[1];
				}

				geneObjects [info.Key].transform.GetChild (0).gameObject.GetComponentInChildren<Text> ().text = locusTags;

			} else {
                //contentPanels[0].GetComponentInChildren<Text> ().text = geneInfo[i];
                //i = 1;
                break;
			}
		}
	}

	public void RemoveAllGenes(bool spheresOn) {
		
		foreach (var gene in genes)
		{
			geneObjects.ElementAt(gene).SetActive(false);
			genes = new List<int> ();
		}
		geneInfo = new Dictionary<int, string> ();
		PrintGeneInfo ();

		if (spheresOn) {
			toggleSpheresOn ();
		}
	}

	public void RemoveOneGene(int geneNumber) {
		
		geneObjects.ElementAt(geneNumber).SetActive(false);
		geneInfo.Remove (geneNumber);
		PrintGeneInfo ();

		if (geneObjects.Count > 0) {
			toggleSpheresOn ();
		}
	}


	////////////////////////////////////////////////////////
	/// NEXT STRUCTURE /////////
	///////////////////////////////////////////////////////

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

		foreach (var gene in genes) {
			geneObjects.ElementAt(gene).SetActive (true);
			toggleSpheresOff ();
		}
	}




	////////////////////////////////////////////////////////
	/// DROPDOWN /////////
	///////////////////////////////////////////////////////


	//CALL WHEN USER CHANGES SELECTION OF DROPDOWN INPUT
	public void DropdownInputChanged(int inputChoice){
		searchString = changingDropdownGeneTags.ElementAt (inputChoice);
        
		if (inputChoice == 0) {
			RemoveAllGenes(true);
		}else{
            RemoveAllGenes(false);

			LoadGenesByString ();
		}
	}


	//INSTANTIATE DROPDOWN ONCE
	private void InstantiateDropdownList(){
		allGeneTags.Add ("No Tag Selected");

		foreach (var dictionary in dictionaries){
			foreach (var gene in dictionary) {
				if (gene.Value != null) {
					allGeneTags.Add (gene.Key);
					var nums = gene.Key.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

					int numOne = Int32.Parse (nums [0]);
					int numTwo = Int32.Parse (nums [1]);

					spheres [numOne].transform.GetChild (0).gameObject.transform.GetChild (0).GetComponentInChildren<Text> ().text = gene.Key;

					if (numOne != numTwo) {
						spheres [numTwo].transform.GetChild (0).gameObject.transform.GetChild (0).GetComponentInChildren<Text> ().text = gene.Key;
					}
				}
			}
		}
		changingDropdownGeneTags.AddRange (allGeneTags);
	}


	////////////////////////////////////////////////////////
	/// TEXT INPUT /////////
	///////////////////////////////////////////////////////

	//CALL WHEN USER CHANGES TEXT INPUT FOR SEARCH
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


	//DISPLAY ONLY CERTAIN AMOUNT OF TAGS IN DROPDOWN DEPENDING ON TEXTFIELD SEARCH INPUT
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


	////////////////////////////////////////////////////////
	/// SPHERES TRANSPARENCY /////////
	///////////////////////////////////////////////////////

	void toggleSpheresOff(){

		foreach (var sphere in spheres) {

			var numChrom = Int32.Parse(sphere.name.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);

			var ps = sphere.GetComponent<ParticleSystem>();
			var psColorModule = ps.colorOverLifetime;
			psColorModule.color = colorsSpheresOff[numChrom];
		}

	}

	void toggleSpheresOn(){

		foreach (var sphere in spheres) {

			var numChrom = Int32.Parse(sphere.name.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[2]);

			var ps = sphere.GetComponent<ParticleSystem>();
			var psColorModule = ps.colorOverLifetime;
			psColorModule.color = colorsSpheresOn[numChrom];
		}

	}



	////////////////////////////////////////////////////////
	/// GENE FILES /////////
	///////////////////////////////////////////////////////
	// READ IN GENE FILES ONCE //

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








	////////////////////////////////////////////////////////
	/// GET NUMBER OF SPHERE CORRELATING TO GENE POSITION /////////
	///////////////////////////////////////////////////////

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


	////////////////////////////////////////////////////////
	/// WEIGHTS /////////
	///////////////////////////////////////////////////////

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
		Debug.Log (numChoice);
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
				marker.transform.localScale = new Vector3(1.0f, w /1.0f, 1.0f);
				marker.GetComponent<Renderer>().SetPropertyBlock(mat);
				markers[name].Add(marker);
			}
		}
	}



	////////////////////////////////////////////////////////
	/// INITIALIZATION /////////
	///////////////////////////////////////////////////////

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

		dictionaries.Add(genesChrom1);
		dictionaries.Add(genesChrom2);
		dictionaries.Add(genesChrom3);

		LoadFromDat();

		InstantiateDropdownList ();

		LoadDropdownGeneTags ();
	}

	////////////////////////////////////////////////////////
	/// INSTANTIATE DNA STRUCTURE INCLUDING SPHERES, GENES AND GENE INFO /////////
	///////////////////////////////////////////////////////
	void LoadFromDat()
	{
		foreach (GameObject go in spheres) Destroy(go);
		spheres = new List<GameObject>();
		geneObjects = new List<GameObject> ();
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
			var ps = sphere.GetComponent<ParticleSystem>();
			var psColorModule = ps.colorOverLifetime;
			psColorModule.color = colorsSpheresOn[c];

			var gene = sphere.transform.GetChild (0).gameObject;
			gene.name = i.ToString ();
			geneObjects.Add(gene);
			spheres.Add(sphere);
		}
		toggleSpheresOn ();
		Debug.Log ("Count of spheres: " + spheres.Count);
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

	void Awake(){
		contentPanels.Add(GameObject.Find ("Content1"));
		contentPanels.Add(GameObject.Find ("Content2"));
		contentPanels.Add(GameObject.Find ("Content3"));
	}
}