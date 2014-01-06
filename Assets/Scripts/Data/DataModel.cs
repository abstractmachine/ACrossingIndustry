using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <List>
using System.Text.RegularExpressions; // Regex

/*

 * PersonaData
 	" name
 	" prefab
 	" faction
 	v coordinate
 	<v> coordinates

* DialogHistory
	" index
	f time
	f timeDelay

 * DialogData
 	" id
 	" player
 	" persona
    <i,Utterance> utterances
    			  <SpeechAct> personaSpeechActs
    						  " action
    						  " phrase
    			  <SpeechAct> playerSpeechActs
    						  " action
    						  " phrase
        					  <Consequence> consequences
        									" condition
        									<"> nexts
        									" result

*/


/////////////////////////


public class DataHistory {

	int index;
	float time;
	public float delay = 120.0f; // should be two minutes

	public bool IsTooOld { get { return Time.time - time > delay; } }
	public bool IsNeutral { get { return index == 0; } }
	
	public int Index { 
		get { 
			if (IsTooOld) Reset();
			else ResetTimer();
			return index; 
		} 
		set { 
			index = value; 
			ResetTimer();
		} 
	}

	public DataHistory() {
		Reset();
	}

	public void Start() {
		index = 1;
	}

	public void Reset() {
		index = 0;
		ResetTimer();
	}

	void ResetTimer() {
		time = Time.time;
	}

}


/////////////////////////


public class PersonaData {

	public string name;										// the gameObject.name of the Persona
	public string prefab;									// the prefab used to construct a new Persona
	public string faction;									// which "camp/group/party" the Persona belongs to 
	public Vector3 coordinate; 								// the starting coordinate for the Persona
	public List<Vector3> coordinates = new List<Vector3>(); // where the Persona walks to/from

}


/////////////////////////


public class DialogData {

	public string id;
	public string persona;
	public string player;

	public Dictionary<int,Utterance> utterances = new Dictionary<int,Utterance>();

	public DialogData(string _id, string _player, string _persona) { 
		id = _id;
		player = _player;
		persona = _persona; 
	}
	public DialogData(string _player, string _persona) { 
		player = _player;
		persona = _persona;
		id = player + "-" + persona; 
	}

	// Accessor

	public bool ContainsIndex(int index) { return utterances.ContainsKey(index); }

	// Data extraction

	public Utterance GetUtterance(int index) {
		// if this key doesn't exist, send back null
		if (!utterances.ContainsKey(index)) return null;
		// ok
		return utterances[index];
	}

	// Data Dump

	public void DumpData() {

		DumpDialogs();

	}

	public void DumpDialogs() {

		// start by dumping our name + id
		Debug.Log(id + "\t" + player + "\t" + persona);
		// go through each utterance
		foreach(KeyValuePair<int,Utterance> entry in utterances) {
			// extract index
			int index = entry.Key;
			Debug.Log("index: " + index);
			// dump data
			DumpUtterance(entry.Value);
		}

	}


	public void DumpUtterance(Utterance utterance) {

		utterance.Dump();

	}

}


/////////////////////////


public class Utterance {

	public List<SpeechAct> persona = new List<SpeechAct>();
	public List<SpeechAct> player = new List<SpeechAct>();

	////////////////////////////////////

	List<SpeechAct> Whom(string whom) {
		if (whom == "persona") return persona;
		else return player;
	}

	public SpeechAct PersonaSpeechAct(int index){return GetSpeechAct("persona",index);}
	public SpeechAct PlayerSpeechAct(int index)	{return GetSpeechAct("player",index);}

	public SpeechAct GetSpeechAct(string whom, int index) {
		if (Whom(whom).Count <= index) return null;
		else return Whom(whom)[index];
	}

	// get a list of all the phrases the persona can speak
	public List<string> PersonaPhrases(){return Phrases("persona");}
	public List<string> PlayerPhrases()	{return Phrases("player");}

	public List<string> Phrases(string whom) {
		List<string> phrases = new List<string>();
		foreach(SpeechAct speechAct in Whom(whom)) {
			if (speechAct.phrase != null) phrases.Add(speechAct.phrase);
		}
		return phrases;
	}

	// raondomly choose a phrase from the available list
	public string PersonaPhrase()	{return Phrase("persona");}
	public string PlayerPhrase()	{return Phrase("player");}

	string Phrase(string whom) {
		int randomIndex = (int)UnityEngine.Random.Range(0,Whom(whom).Count);
		return Phrase(whom,randomIndex);
	}

	// get a specific phrase from the list
	public string PersonaPhrase(int index) 	{return Phrase("persona",index);}
	public string PlayerPhrase(int index) 	{return Phrase("player",index);}

	string Phrase(string whom, int index) {
		if (Whom(whom).Count <= index) return null;
		return Whom(whom)[index].phrase;
	}

	// get the consequences of a speech act
	public List<Consequence> PersonaConsequences(int index){return Consequences("persona",index);}
	public List<Consequence> PlayerConsequences(int index) {return Consequences("player",index);}

	List<Consequence> Consequences(string whom, int index) {
		if (Whom(whom).Count <= index) return null;
		return Whom(whom)[index].consequences;
	}

	public void Dump() {

		string str = "";

		foreach(SpeechAct speechAct in persona) {
			str += "persona phrase:" + speechAct.phrase + "\n";
			str += "persona action:" + speechAct.action + "\n";
		}

		foreach(SpeechAct speechAct in player) {
			str += "player phrase:" + speechAct.phrase + "\n";
			if (speechAct.action != null) {
				str += "player action:" + speechAct.action + "\n";
			}
			str += "player consequences:\n";
			foreach(Consequence consequence in speechAct.consequences) {
				str += consequence.GetDump();
			} // foreach(Consequence
		}

		Debug.Log(str);

	}

}


/////////////////////////


public class SpeechAct {

	public string action = null;
	public string phrase = null;

	public List<Consequence> consequences = new List<Consequence>();
	public void AddConsequence(Consequence consequence) {
		consequences.Add(consequence);
	}
	// get the part before the parentheses
	public string ActionFunction() {
		if (action == null) return null;
		string beforeParentheses = Regex.Match(action,@"^[^\(]*").Value;
		return beforeParentheses;
	}
	// get the interior of parenthesis
	public string ActionArguments() {
		if (action == null) return null;
		string insideParentheses = Regex.Match(action, @"(?<=\().+?(?=\))").Value;
		return insideParentheses;
	}

}


/////////////////////////


public class Consequence {
	
	// consequence holds condition-associated next indexes and resulting changes
	public string condition = null;
	public List<int> nexts = null;
	public string result = null;
	// add a next index
	public void AddNext(int next) {
		if (nexts == null) nexts = new List<int>();
		nexts.Add(next);
	}
	// get a random next
	public int RandomNext() {
		if (nexts == null || nexts.Count == 0) return -1;
		return nexts[(int)UnityEngine.Random.Range(0,nexts.Count)];
	}
	// get the part before the parentheses
	public string ConditionFunction() {
		if (condition == null) return null;
		string beforeParentheses = Regex.Match(condition,@"^[^\(]*").Value;
		return beforeParentheses;
	}
	// get the interior of parenthesis
	public string ConditionArguments() {
		if (condition == null) return null;
		string insideParentheses = Regex.Match(condition, @"(?<=\().+?(?=\))").Value;
		return insideParentheses;
	}
	// get the printout of the contents
	public string GetDump() {
		string str = "";
		if (condition != null) {
			str += "player condition: " + condition + "\n";
		}
		foreach(int next in nexts) {
			str += "player next:" + next + "\n";
		} // foreach(string
		if (result != null) {
			str += "player result: " + result + "\n";
		}
		return str;
	}
}

