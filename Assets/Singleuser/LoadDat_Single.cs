using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections;

public class LoadDat_Single : MonoBehaviour {

	public int index = 0;
	TextAsset[] structures;
	TextAsset[] weights;
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
	public Dictionary<string, List<string>> genesComplete = new Dictionary<string, List<string>>();
	public Dictionary<string, List<GameObject>> markers = new Dictionary<string, List<GameObject>>();
	private List<GameObject> toggles = new List<GameObject>();
	private string searchString = "";
    private string removeString = "";
    private int[] numberOfChromosomes;
	private List<GameObject> geneObjects = new List<GameObject>();
	List<Dictionary<string, List<string>>> dictionaries = new List<Dictionary<string, List<string>>>();
	private List<GameObject> contentPanels = new List<GameObject>();
	private Dictionary<int, string> geneInfo = new Dictionary<int, string> ();
	Dictionary<int, List<int>> linkSpheresGenes = new Dictionary<int, List<int>> ();
	private int whichChromosome = 1;
    private bool isCalculating = false;


	////////////////////////////////////////////////////////
	/// GET ADDITIONAL GENE INFORMATION /////////
	///////////////////////////////////////////////////////
    string LoadGeneText(string sphName) {
        var sphereNumber = Int32.Parse(sphName.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[0]);

        var sphereObject = spheres[sphereNumber];

        string newString = "";

        for (var gene = 0; gene < genesComplete.Count; gene++)
        {

            List<string> geneContent = genesComplete.ElementAt(gene).Value;
            var tagElements = genesComplete.ElementAt(gene).Key.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            var geneText = new string[geneContent.Count];
            var tempString = "";

            if (Int32.Parse(tagElements[0]) == sphereNumber || Int32.Parse(tagElements[1]) == sphereNumber)
            {

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

    public void LinkSpheresTagsNumbers(){
		
		//First Tag is "No Tag Selected"
		for (var tag = 1; tag < allGeneTags.Count; tag++){

			var tagSphereNums = allGeneTags.ElementAt(tag).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

			var firstSphere = Int32.Parse(tagSphereNums[0]);
			var secondSphere = Int32.Parse(tagSphereNums[1]);
			for (var num = firstSphere; num <= secondSphere; num++)
			{
				
				if (linkSpheresGenes.ContainsKey(num)) {
					linkSpheresGenes [num].Add (tag);
					//Debug.Log ("Sphere number: " + num + " has tag at geneTags " + tag);
				} else {
					var newList = new List<int> ();
					newList.Add (tag);
					linkSpheresGenes.Add (num, newList);
					//Debug.Log ("Sphere number: " + num + " has new list with tag at geneTags " + tag);
				}
			}
		}
    }

	public void LoadGenesByClickSphere(int sphereNum){
        if (linkSpheresGenes.ContainsKey(sphereNum)) {
            var geneIndexes = linkSpheresGenes[sphereNum];

            foreach (var indexOfGene in geneIndexes)
            {
                searchString = allGeneTags.ElementAt(indexOfGene);
                StartCoroutine(LoadGenesByString());
            }
        }
	}

	IEnumerator LoadGenesByString(){
        isCalculating = true;
		
		var words = searchString.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		var fromNum = Int32.Parse (words [0]);
		var toNum = Int32.Parse (words [1]);

		for (var i = fromNum; i <= toNum; i++) {
			activateGlow (i);
            yield return new WaitForSeconds(3.0F);
        }
		toggleSpheresOff ();
        isCalculating = false;
	}

	private void activateGlow(int sphereNum){
		
		geneObjects.ElementAt(sphereNum).SetActive(true);
		genes.Add (sphereNum);

        string info = LoadGeneText (spheres [sphereNum].name);

        if (!geneInfo.ContainsKey(sphereNum)){
			geneInfo.Add(sphereNum, info);
			PrintGeneInfo();
		}

	}


    private void PrintGeneInfo()
    {

        foreach (var panel in contentPanels)
        {
            panel.GetComponentInChildren<Text>().text = "";
            panel.GetComponentInChildren<Text>().alignment = TextAnchor.UpperLeft;
        }
        
        for (var i = 0; i < geneInfo.Count; i++)
        {
            

            var info = geneInfo.ElementAt(i);

            if (i < contentPanels.Count)
            {
                var infoText = Regex.Replace(info.Value, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                infoText = "Sphere number: " + info.Key + "\n\n" + infoText;

                contentPanels[i].GetComponentInChildren<Text>().alignment = TextAnchor.UpperLeft;
                contentPanels[i].GetComponentInChildren<Text>().text = infoText;
            }

            string pattern = @"(\/locus_tag="".*?\"")";
            MatchCollection matches = Regex.Matches(info.Value, pattern);

            var uniqueMatches = matches
                .OfType<Match>()
                .Select(m => m.Value)
                .Distinct()
                .ToList();

            string locusTags = "";
            foreach (var locusTag in uniqueMatches)
            {
                locusTags = locusTags + "\n" + locusTag.Split(new string[] { "\"" }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            locusTags = "Sphere number: " + info.Key + "\n" + locusTags;
            geneObjects[info.Key].transform.GetChild(0).gameObject.GetComponentInChildren<Text>().text = locusTags;

            
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

	public IEnumerator RemoveGenesOnClickSphere(int sphereNum) {
        if(isCalculating)
            yield return new WaitForSeconds(1.0F);

        var geneIndexes = linkSpheresGenes[sphereNum];

        foreach (var indexOfGene in geneIndexes)
        {
            removeString = allGeneTags.ElementAt(indexOfGene);
            RemoveGenesByString();
            yield return new WaitForSeconds(2.0F);
        }
        genes.RemoveAt(sphereNum);

    }

    public void RemoveGenesByString() {
        var words = removeString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        
        var fromNum = Int32.Parse(words[0]);
        var toNum = Int32.Parse(words[1]);

        for (var i = fromNum; i <= toNum; i++)
        {
            geneInfo.Remove(i);
            Debug.Log(geneObjects.ElementAt(i));
            geneObjects.ElementAt(i).SetActive(false);
        }
        PrintGeneInfo();
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

			StartCoroutine(LoadGenesByString ());
		}
	}


	//INSTANTIATE DROPDOWN ONCE
	private void InstantiateDropdownList(){
		allGeneTags.Add ("No Tag Selected");

		foreach (var gene in genesComplete) {
			allGeneTags.Add (gene.Key);
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

		StartCoroutine(LoadDropdownGeneTags ());
	}


	//DISPLAY ONLY CERTAIN AMOUNT OF TAGS IN DROPDOWN DEPENDING ON TEXTFIELD SEARCH INPUT
	IEnumerator LoadDropdownGeneTags(){
		var dropdown = GameObject.Find("GeneTags").GetComponent<Dropdown> ();
		dropdown.options.Clear ();
		var count = 0;
		foreach (string tag in changingDropdownGeneTags) {
			count++;
			if (count > 100) {
				break;
			}
			dropdown.options.Add (new Dropdown.OptionData (tag.ToString()));
            yield return null;
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

	private void ReadGeneFiles(){
		var chromosomeFile = Resources.Load<TextAsset>("Genes/Genes_Complete");

		string[] lines = chromosomeFile.text.Split ('\n');

		List<string> tempList = new List<string> ();
		string name = "";

		foreach (string line in lines) {

			if (line.StartsWith ("gene ")) {
				if (line.Contains("gene complement(<1..5662)")) {
					whichChromosome = 1;
				}
				if (line.Contains("gene 5451..6318")) {
					whichChromosome = 2;
				}
				if (line.Contains("gene complement(1..2852)")) {
					whichChromosome = 3;
				}

				var pos = CalculateSpherePosition (line);

				if (pos != null) {
					if (name != "") {
						genesComplete.Add (name, tempList);
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
		genesComplete.Add (name, tempList);
	}








	////////////////////////////////////////////////////////
	/// GET NUMBER OF SPHERE CORRELATING TO GENE POSITION /////////
	///////////////////////////////////////////////////////

	string CalculateSpherePosition(string line){
		var words = line.Split (new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		string locationInfo = words [1];
		var numbers = locationInfo.Split (new string[] { ".." }, StringSplitOptions.RemoveEmptyEntries);
		string fro = "";
		string to = "";
		try {
			fro = numbers [0];
			to = numbers [1];

			fro = Regex.Match (fro, @"\d+").Value;
			to = Regex.Match (to, @"\d+").Value;

			int fromNum = Int32.Parse (fro) / 3499;
			int toNum = Int32.Parse (to) / 3499;

			if(whichChromosome == 2){
				fromNum += 1593;
				toNum += 1593;
			}
			if(whichChromosome == 3){
				fromNum += 2882;
				toNum += 2882;
			}

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

		ReadGeneFiles ();

		LoadFromDat();

		InstantiateDropdownList ();

		StartCoroutine( LoadDropdownGeneTags ());

		LinkSpheresTagsNumbers ();

		
        LoadGenesByClickSphere(3002);
        //LoadGenesByClickSphere (0);
        

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
		peachRed.SetColor("_EmissionColor", colorsWeights[0]);
		var peach = new MaterialPropertyBlock();
		peach.SetColor("_EmissionColor", colorsWeights[1]);
		var peachBrown = new MaterialPropertyBlock();
		peachBrown.SetColor("_EmissionColor", colorsWeights[2]);
		var dullBrown = new MaterialPropertyBlock();
		dullBrown.SetColor("_EmissionColor", colorsWeights[3]);
		var yellowBrown = new MaterialPropertyBlock();
		yellowBrown.SetColor("_EmissionColor", colorsWeights[4]);

		var magenta = new MaterialPropertyBlock();
		magenta.SetColor("_EmissionColor", colorsWeights[5]);
		var darkRed = new MaterialPropertyBlock();
		darkRed.SetColor("_EmissionColor", colorsWeights[6]);

		var lightTurquoise = new MaterialPropertyBlock();
		lightTurquoise.SetColor("_EmissionColor", colorsWeights[7]);
		var turquoise = new MaterialPropertyBlock();
		turquoise.SetColor("_EmissionColor", colorsWeights[8]);

		var lightPurple = new MaterialPropertyBlock();
		lightPurple.SetColor("_EmissionColor", colorsWeights[9]);
		var purple = new MaterialPropertyBlock();
		purple.SetColor("_EmissionColor", colorsWeights[10]);
		var bluePurple = new MaterialPropertyBlock();
		bluePurple.SetColor("_EmissionColor", colorsWeights[11]);
		var redPurple = new MaterialPropertyBlock();
		redPurple.SetColor("_EmissionColor", colorsWeights[12]);

		var brightGreen = new MaterialPropertyBlock();
		brightGreen.SetColor("_EmissionColor", colorsWeights[13]);
		var dullGreen = new MaterialPropertyBlock();
		dullGreen.SetColor("_EmissionColor", colorsWeights[14]);


		var brightBlue = new MaterialPropertyBlock();
		brightBlue.SetColor("_EmissionColor", colorsWeights[15]);
		var dullBlue = new MaterialPropertyBlock();
		dullBlue.SetColor("_EmissionColor", colorsWeights[16]);
		var darkBlue = new MaterialPropertyBlock();
		darkBlue.SetColor("_EmissionColor", colorsWeights[17]);

		var lightYellow = new MaterialPropertyBlock();
		lightYellow.SetColor("_EmissionColor", colorsWeights[18]);
		var orange = new MaterialPropertyBlock();
		orange.SetColor("_EmissionColor", colorsWeights[19]);


		var lightGreen = new MaterialPropertyBlock();
		lightGreen.SetColor("_EmissionColor", colorsWeights[20]);
		var neonGreen = new MaterialPropertyBlock();
		neonGreen.SetColor("_EmissionColor", colorsWeights[21]);
		var grassGreen = new MaterialPropertyBlock();
		grassGreen.SetColor("_EmissionColor", colorsWeights[22]);
		var darkGreen = new MaterialPropertyBlock();
		darkGreen.SetColor("_EmissionColor", colorsWeights[23]);

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
        contentPanels.Add(GameObject.Find("Content4"));
        contentPanels.Add(GameObject.Find("Content5"));
        contentPanels.Add(GameObject.Find("Content6"));
        contentPanels.Add(GameObject.Find("Content7"));
        contentPanels.Add(GameObject.Find("Content8"));
        contentPanels.Add(GameObject.Find("Content9"));
    }
}