using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <List>

// for stream writer
using System;
using System.IO;
using System.Xml;

public class Data : MonoBehaviour {

	// the dialog data, arranged by dialogID (Player.name + "_" + Persona.name)
	Dictionary<string,DialogData> dialogs = new Dictionary<string,DialogData>();

	// used for loading Persona GameObjects into Scene
	GameObject personnagesObject;
	public List<GameObject> prefabList = new List<GameObject>();
	public List<Material> materialList = new List<Material>();

	////////////////// Accessor


	public DialogData GetDialogData(string dialogID) {

		if (!dialogs.ContainsKey(dialogID)) return null;

		return dialogs[dialogID];

	}


	public Utterance GetUtteranceFromIndex(string dialogID, int index) {

		// first extract all the data
		DialogData dialogData = GetDialogData(dialogID);
		// make sure that utterance has a key
		if (!dialogData.utterances.ContainsKey(index)) return null;
		// ok, found it
		return dialogData.utterances[index];

	}


	public List<string> GetPersonaPhrasesFromIndex(string dialogID, int index) {

		// the list of phrases can potentially include 0 (empty), 1 (single), or several (random/choice)
		List<string> phrases = new List<string>();
		// get the utterance for this index
		Utterance utterance = GetUtteranceFromIndex(dialogID,index);
		// if inexistant, return nothing
		if (utterance == null) return phrases;
		// now extract the proper speech act
		return utterance.PersonaPhrases();

	}


	public List<string> GetPlayerPhrasesFromIndex(string dialogID, int index) {

		// the list of phrases can potentially include 0 (empty), 1 (single), or several (random/choice)
		List<string> phrases = new List<string>();
		// get the utterance for this index
		Utterance utterance = GetUtteranceFromIndex(dialogID,index);
		// if inexistant, return nothing
		if (utterance == null) return phrases;
		// now extract the proper speech act
		return utterance.PlayerPhrases();

	}
	

	////////////////// Init


	void Start () {

		personnagesObject = GameObject.Find("Personae");
		
		LoadXml();
	
	}
	

	////////////////// Load


	public void LoadXml() {

		ParseDialogs();
		ParsePersonae();

	}


	public Dictionary<string,DialogData> DialogNames() {

		ParseDialogNames();

		return dialogs;

	}
	

	////////////////// Loop


	void Update () {
	
	}


	////////////////// GameObject generation


	void RemovePersonnages() {

		foreach(Transform childTransform in personnagesObject.transform) {
    		Destroy(childTransform.gameObject);
		}

	}


	void InstantiatePersona(PersonaData personaData) {

		// the prefab we'll use to instantiate
		GameObject prefabToInstantiate = null;
		// find this prefab in the assets
		foreach(GameObject thisPrefab in prefabList) {
			// if this is the one
			if (thisPrefab.name == personaData.prefab) {
				prefabToInstantiate = thisPrefab;
				break;
			} // if
		} // foreach(GameObject

		// if we didn't find one
		if (prefabToInstantiate == null) {
			print("invalid prefab");
			return;
		}

		// the material we need to instantiate
		Material materialToInstantiate = null;
		// find this material in the list of available materials
		foreach(Material thisMaterial in materialList) {
			// if this is the one
			if (thisMaterial.name == personaData.faction) {
				materialToInstantiate = thisMaterial;
				break;
			} // if
		} // foreach(Material)

		// if we didn't find one
		if (materialToInstantiate == null) {
			print("invalid material");
			return;
		}

		GameObject newObject = (GameObject)Instantiate(prefabToInstantiate);
		// set the parent of this object
		newObject.transform.parent = personnagesObject.transform;
		// set the name of this object
		newObject.name = personaData.name;
		// get the delta of this object
		Vector3 deltaPosition = personaData.coordinate - newObject.transform.position;
		// make sure we don't move on the y axis
		deltaPosition.y = 0.0f;
		// move to new position
		newObject.transform.position += deltaPosition;
		// set the material of this object
		newObject.GetComponent<Persona>().SetMaterial(materialToInstantiate);
		// set a random starting orientation
		newObject.transform.Rotate(0.0f, UnityEngine.Random.Range(0.0f,360.0f), 0.0f);
		// set all the walkTo positions
		newObject.GetComponent<Persona>().SetCoordinates(personaData.coordinates);

	}


	////////////////// Text Parsing


	void ParseDialogs() {

		// clear any previous dialog data
		dialogs.Clear();
		
		ParseDialogNames();
		ParseDialogContent();

	}


	void ParseDialogNames() {

		// google uses namespaces in xml tags (TODO: extract namespace data directly from xmls)
		string gsxNamespace = "http://schemas.google.com/spreadsheets/2006/extended";
		//string gdNamespace = "http://schemas.google.com/g/2005";

		// load xml file from local storage
		XmlDocument doc = new XmlDocument();

#if UNITY_EDITOR

		TextAsset xmlText = (TextAsset)Resources.Load("dialog_names", typeof(TextAsset));
		doc.LoadXml(xmlText.text);

#elif UNITY_STANDALONE_OSX

		string filepath = Application.dataPath + @"/Data/Xml/dialog_names.xml";
		if (File.Exists(filepath)) {
			doc.Load(filepath);
		} else {
			TextAsset xmlText = (TextAsset)Resources.Load("dialog_names", typeof(TextAsset));
			doc.LoadXml(xmlText.text);
		}

#elif UNITY_STANDALONE_WIN

		string filepath = Application.dataPath + @"\Data\Xml\dialog_names.xml";
		if (File.Exists(filepath)) {
			doc.Load(filepath);
		} else {
			TextAsset xmlText = (TextAsset)Resources.Load("dialog_names", typeof(TextAsset));
			doc.LoadXml(xmlText.text);
		}

#endif

		// go through each entry
		XmlNodeList entries = doc.GetElementsByTagName("entry");
		foreach(XmlNode entry in entries) {

			// if empty, forget the rest
			if (!entry.HasChildNodes) continue;

			string dialogID = entry["dialogid",gsxNamespace].InnerText;
			string playerName = entry["playername",gsxNamespace].InnerText;
			string personaName = entry["personaname",gsxNamespace].InnerText;

			DialogData dialog = new DialogData(dialogID,playerName,personaName);
			dialogs[dialogID] = dialog;
			//dialogs.Add(dialog);

		}

	}


	void ParseDialogContent() {

		// go through each dialog
		foreach(KeyValuePair<string,DialogData> dialogEntry in dialogs) {
		//foreach(DialogData dialogData in dialogs) {

			// extract id
			string dialogID = dialogEntry.Key;
			// extract data
			DialogData dialogData = dialogEntry.Value;

			XmlDocument doc = new XmlDocument();

#if UNITY_EDITOR

			TextAsset xmlText = (TextAsset)Resources.Load(dialogID, typeof(TextAsset));
			doc.LoadXml(xmlText.text);

#elif UNITY_STANDALONE_OSX

			string filepath = Application.dataPath + @"/Data/Xml/" + dialogID + ".xml";
			if (File.Exists(filepath)) {
				doc.Load(filepath);
			} else {
				TextAsset xmlText = (TextAsset)Resources.Load(dialogID, typeof(TextAsset));
				doc.LoadXml(xmlText.text);
			}

#elif UNITY_STANDALONE_WIN

			string filepath = Application.dataPath + @"\Data\Xml\" + dialogID + ".xml";
			if (File.Exists(filepath)) {
				doc.Load(filepath);
			} else {
				TextAsset xmlText = (TextAsset)Resources.Load(dialogID, typeof(TextAsset));
				doc.LoadXml(xmlText.text);
			}

#endif

			//string filepath = System.IO.Path.Combine(@"./Assets/Xml/Ressources/", dialogID + ".xml");
			//doc.Load(dialogID + ".xml");

			string gsxNamespace = "http://schemas.google.com/spreadsheets/2006/extended";

			// start with some random index values
			int lastIndex = -666;
			// when current index != lastIndex we know we've created a new Utterance
			int currentIndex = lastIndex;
			// player speech acts can span over several entries. It has to be declared here
			SpeechAct playerSpeechAct = new SpeechAct();

			// go through each entry
			XmlNodeList entries = doc.GetElementsByTagName("entry");
			foreach(XmlNode xmlEntry in entries) {

				// if empty, forget the rest
				if (!xmlEntry.HasChildNodes) continue;

				// extract all the data from this xml xmlEntry
				string index = xmlEntry["index",gsxNamespace].InnerText;
				string saysPersona = xmlEntry["sayspersona",gsxNamespace].InnerText;
				string repliesPlayer = xmlEntry["repliesplayer",gsxNamespace].InnerText;
				string actionPersona = xmlEntry["actionpersona",gsxNamespace].InnerText;
				string actionPlayer = xmlEntry["actionplayer",gsxNamespace].InnerText;
				string condition = xmlEntry["condition",gsxNamespace].InnerText;
				string next = xmlEntry["next",gsxNamespace].InnerText;
				string result = xmlEntry["result",gsxNamespace].InnerText;
				// string comment = xmlEntry["comment",gsxNamespace].InnerText;

				// if this is an empty line, move on to next
				if (   ""==index 
					&& ""==saysPersona 
					&& ""==repliesPlayer 
					&& ""==actionPersona 
					&& ""==actionPlayer 
					&& ""==condition 
					&& ""==next 
					&& ""==result) continue;

				// if there is a string value for index
				if (index != "") currentIndex = System.Convert.ToInt32(index);
				// if changed index
				if (currentIndex != lastIndex) {
					// then this is a new index, add a new utterance
					dialogData.utterances[currentIndex] = new Utterance();
					// remember for next time
					lastIndex = currentIndex;
					// create a new player speech act
					playerSpeechAct = new SpeechAct();
				} // if (currentIndex
				
				// if we have new persona phrases
				if (saysPersona != "") {
					// break into a list of sub-phrases if there is more than one possibility
					List<string> personaPhrases = splitSubstrings(saysPersona);
					// for each phrase
					foreach(string phrase in personaPhrases) {
						// create a speech act
						SpeechAct personaSpeechAct = new SpeechAct();
						// add this phrase to the speech act (while removing any quotes)
						personaSpeechAct.phrase = removeQuotes(phrase);
						// if there's an action, apply it
						if (actionPersona != "") personaSpeechAct.action = actionPersona;
						// add to list of phrases the Persona can speak
						dialogData.utterances[currentIndex].persona.Add(personaSpeechAct);
					} // foreach (string
				} // if (saysPersona

				// if the player replies something
				if (repliesPlayer != "" || (saysPersona != "" && repliesPlayer == "" && next != "")) {
					// create a speech act
					playerSpeechAct = new SpeechAct();
					// add this phrase to the speech act (while removing any quotes)
					if (repliesPlayer != "") playerSpeechAct.phrase = removeQuotes(repliesPlayer);
					// if there an action associate with this phrase, apply it
					if (actionPlayer != "") playerSpeechAct.action = actionPlayer;
					// add to the list of phrases the player can speak
					dialogData.utterances[currentIndex].player.Add(playerSpeechAct);
				} // if (repliesPlayer

				// TODO: player SpeechAct is sometimes empty
				// in such a case, add next consequences anyway

				// if there are consequences to this speech
				if (condition != "" || next != "" || result != "") {
					// create a consequence
					Consequence consequence = new Consequence();
					// if there is some sort of condition attached to this consequence
					if (condition != "") consequence.condition = condition;
					// if there is a resulting state/action from this consequence
					if (result != "") consequence.result = result;
					// in case there are several nexts
					if (next != "") {
						// split up into individual ints
						List<string> nextStrings = splitSubstrings(next, ';');
						// go through each
						foreach(string nextString in nextStrings) {
							// add to list as an int value
							consequence.AddNext(System.Convert.ToInt32(nextString));
						} // foreach(string
					} // if (next
					// add to list of consequences
					playerSpeechAct.AddConsequence(consequence);
				} // if (condition

			} // foreach(XMLNode

		} // foreach(KeyValuePair

	} // void ParseDialogContent()
	
	

	Vector3 ParseCoordinate(string coordinateString) {

		// break using comma
		char[] delimiters = new char[] { ',' };
		string[] splitString = coordinateString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
		// if not enough elements, there's soething wrong
		if (splitString.Length < 2) return Vector3.zero;
		// ok, get the coordinate
		Vector3 coordinate = new Vector3(float.Parse(splitString[0]), 0.0f, float.Parse(splitString[1]));
		// send back to sender
		return coordinate;

	}



	void ParsePersonae() {

		// remove any previous personnages
		RemovePersonnages();

		string gsxNamespace = "http://schemas.google.com/spreadsheets/2006/extended";

		// create an xml document
		XmlDocument doc = new XmlDocument();
		// TextAsset xmlText = (TextAsset)Resources.Load("dialog_personae", typeof(TextAsset));
		// doc.LoadXml(xmlText.text);

#if UNITY_EDITOR

		TextAsset xmlText = (TextAsset)Resources.Load("dialog_personae", typeof(TextAsset));
		doc.LoadXml(xmlText.text);

#elif UNITY_STANDALONE_OSX

		string filepath = Application.dataPath + @"/Data/Xml/dialog_personae.xml";
		if (File.Exists(filepath)) {
			doc.Load(filepath);
		} else {
			TextAsset xmlText = (TextAsset)Resources.Load("dialog_personae", typeof(TextAsset));
			doc.LoadXml(xmlText.text);
		}

#elif UNITY_STANDALONE_WIN

		string filepath = Application.dataPath + @"\Data\Xml\dialog_personae.xml";
		if (File.Exists(filepath)) {
			doc.Load(filepath);
		} else {
			TextAsset xmlText = (TextAsset)Resources.Load("dialog_personae", typeof(TextAsset));
			doc.LoadXml(xmlText.text);
		}

#endif

		// go through each entry
		XmlNodeList entries = doc.GetElementsByTagName("entry");
		foreach(XmlNode entry in entries) {

			// if empty, forget the rest
			if (!entry.HasChildNodes) continue;

			// create a new persona data holder for this entry
			PersonaData personaData = new PersonaData();

			// populate data using xml data
			personaData.name = entry["name",gsxNamespace].InnerText;
			personaData.prefab = entry["prefab",gsxNamespace].InnerText;
			personaData.faction = entry["faction",gsxNamespace].InnerText;

			// get the coordinate
			string coordinateString = entry["coordinate",gsxNamespace].InnerText;
			// parse the coordinate (string,string) into usable values (float,float)
			personaData.coordinate = ParseCoordinate(coordinateString);
			// if some sort of error, do not record this personaData
			if (personaData.coordinate == Vector3.zero) continue;
			// ok, add as well to the possible coordinates we can walk to
			personaData.coordinates.Add(personaData.coordinate);

			// these are the (possible multiple) coordiantes we can walk to
			string coordinatesString = entry["coordinates",gsxNamespace].InnerText;
			char[] delimiters = new char[] { ';' };
			string[] splitString = coordinatesString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
			foreach(string split in splitString) {
				Vector3 thisCoordinate = ParseCoordinate(split);
				// make sure there aren't any empty coordinates
				if (thisCoordinate == Vector3.zero) continue;
				personaData.coordinates.Add(thisCoordinate);
			}

			InstantiatePersona(personaData);

		}

	}


	////////////////// Text Parsing Tools


	// TODO: This should have been dealt with in the Regex
	string removeQuotes(string phrase) {

		if (phrase == null) return "";

		char[] delimiters = new char[] { '"' };
		string[] result = phrase.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

		if (result.Length > 0) return result[0];
		return "";

	}



	List<string> splitSubstrings(string phrase) {

		char[] delimiters = new char[]{ '/' };
		string[] results = phrase.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

		List<string> listStrings = new List<string>();

		for(int i=0; i<results.Length; i++) {
			listStrings.Add(results[i]);
		}

		return listStrings;

	}



	List<string> splitSubstrings(string phrase, char delimiter) {

		char[] delimiters = new char[]{ delimiter };
		string[] results = phrase.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

		List<string> listStrings = new List<string>();

		for(int i=0; i<results.Length; i++) {
			listStrings.Add(results[i]);
		}

		return listStrings;

	}

}
