using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic; // required for List
using System.Text.RegularExpressions; // used for Regex
using System.Linq;  // used for Regex


public class Conversation {

	public class SpeechAct {
		public List<string> personaPhrases = new List<string>();
		public Dictionary<string,string> playerPhrases = new Dictionary<string,string>();
	}

	Dictionary<int,SpeechAct> speechActs = new Dictionary<int,SpeechAct>();

	// the two conversing Personae
	public string id { get; set; }
	public string playerId { get; set; }
	public string personaId { get; set; }

	// the last timestamp of the conversation
	public float lastTime { get; set; }
	public float timeoutLength = 120.0f; // 2 minutes

	// the state of their conversation
	int index = 0;
	public int Index {
		get { 
			lastTime = Time.time;
			return index;
		}
		set { 
			lastTime = Time.time;
			index = value;
		}
	}

	public bool TimedOut() {
		if (Mathf.Abs(Time.time - lastTime) > timeoutLength) return true;
		else return false;
	}

	public void reset() {
		Index = 0;
	}

	public Conversation(string _playerId, string _personaId) {

		playerId = _playerId;
		personaId = _personaId;

		id = playerId + "-" + personaId;

		ParsePhrases(playerId,personaId);

	}


	public List<string> GetPhrases(bool isPlayer) {

		// if we're at zero, return empty list
		if (0 == Index) return new List<string>(){};
		// if no key for this, return empty list
		if (!speechActs.ContainsKey(Index)) return new List<string>(){};

		SpeechAct speechAct = speechActs[Index];

		// if this is just the Persona, return that
		if (!isPlayer) {
			return speechAct.personaPhrases;
		}

		// extract all the possible phrases from player dictionary
		List<string> phrases = new List<string>();
		// go through dictionary
		foreach(KeyValuePair<string,string> pair in speechAct.playerPhrases) {
			phrases.Add(pair.Key);
		}

		return phrases;

	}


	public int getNextSpeechActIndex(string chosenPhrase="") {

		// zero always returns zero
		if (Index == 0) return 0;

		// first, get the id
		if (!speechActs.ContainsKey(Index)) {
			Debug.Log("Error: speechActs does not contain index " + Index);
			return 0;
		}

		// get the speech act at this index
		SpeechAct speechAct = speechActs[Index];

		// if there are no player phrases, return 0
		if (0 == speechAct.playerPhrases.Count) return 0;

		// go through all the player phrases
		foreach(KeyValuePair<string,string> playerPhrase in speechAct.playerPhrases) {
			// if this was the phrase, return its key
			if (playerPhrase.Key == chosenPhrase) {
				// get all the possible results
				List<string> possibleIndexes = splitSubstrings(playerPhrase.Value, ';');
				// if none
				if (possibleIndexes.Count == 0) {
					Debug.Log("Count error");
					return 0;
				}

				// choose one at random
				int randomIndex = (int)UnityEngine.Random.Range(0,possibleIndexes.Count);
				string chosenIndex = possibleIndexes[randomIndex];
				return System.Convert.ToInt32(chosenIndex);
			}
		}

		// if all else fails
		return 0;

	}

	////// Text Parsing ////////

	void ParsePhrases(string player, string persona) {

		// figure out the filename by concatenating the two names
		string filename = player + "-" + persona + "_csv";
		// load this dialog tree
		TextAsset txt = (TextAsset)Resources.Load(filename, typeof(TextAsset));
		// error handling
		if (txt == null) {
			Debug.Log("Can't read file " + filename);
			return;
		}
		// create a grid out of this CSV file
		string[,] grid = SplitCsvGrid(txt.text);

		// TODO: make a better "startingIndex" system
		// start with some dubious
		int lastIndex = -666;
		int currentIndex = -666;

		// create a speech act
		SpeechAct speechAct = new SpeechAct();

		// go through the data
		for(int i=1; i<grid.GetUpperBound(1); i++) {

			// if empty line, move to next
			if (""==grid[0,i] && ""==grid[1,i] && ""==grid[2,i] && ""==grid[3,i] && ""==grid[4,i] && ""==grid[5,i] && ""==grid[6,i] && ""==grid[7,i] && ""==grid[8,i]) continue;


			// if there is a string value
			string index = grid[0,i];
			if (index != "") currentIndex = System.Convert.ToInt32(index);
			// if changed index
			if (currentIndex != lastIndex) {
				// is this the first time?
				if (lastIndex != -666) {
					// othewise, save the previous line
					speechActs.Add(lastIndex,speechAct);
					// reset values to empty
					speechAct = new SpeechAct();
				}
				// remember for next time
				lastIndex = currentIndex;
			}
			
			// get the field for Persona phrases
			string thisPersonaPhrase = removeQuotes(grid[1,i]);
			// if we have a new persona phrase
			if (thisPersonaPhrase != "") {
				speechAct.personaPhrases = splitSubstrings(thisPersonaPhrase);
			}

			// make sure there is a next tag
			string nextIndexes = grid[6,i];
			if (nextIndexes == "") continue;

			// get the player phrase
			string thisPlayerPhrase = removeQuotes(grid[2,i]);
			if (thisPlayerPhrase == "" || thisPlayerPhrase == null) continue;
			// currently ignore all conditions that are not attached to a phrase
			// TODO: Add conditions
			speechAct.playerPhrases.Add(thisPlayerPhrase,nextIndexes);

		}

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



	List<string> splitSubstrings(string phrase) {

		char[] delimiters = new char[]{ '/' };
		string[] results = phrase.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

		List<string> listStrings = new List<string>();

		for(int i=0; i<results.Length; i++) {
			listStrings.Add(results[i]);
		}

		return listStrings;

	}


	// TODO: This should have been dealt with in the Regex
	string removeQuotes(string phrase) {

		if (phrase == null) return "";

		char[] delimiters = new char[] { '"' };
		string[] result = phrase.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);

		if (result.Length > 0) return result[0];
		return "";

	}
 
	// outputs the content of a 2D array, useful for checking the importer
	void DebugOutputGrid(string[,] grid) {

		string textOutput = ""; 
		for (int y = 0; y < grid.GetUpperBound(1); y++) {	
			for (int x = 0; x < grid.GetUpperBound(0); x++) {
 
				textOutput += grid[x,y]; 
				textOutput += "\t | "; 
			}
			textOutput += "\n"; 
		}
		Debug.Log(textOutput);
	}
 
	// splits a CSV file into a 2D string array
	string[,] SplitCsvGrid(string csvText) {

		string[] lines = csvText.Split("\n"[0]); 
 
		// finds the max width of row
		int width = 0; 
		for (int i = 0; i < lines.Length; i++)
		{
			string[] row = SplitCsvLine( lines[i] ); 
			width = Mathf.Max(width, row.Length); 
		}
 
		// creates new 2D string grid to output to
		string[,] outputGrid = new string[width + 1, lines.Length + 1]; 
		for (int y = 0; y < lines.Length; y++)
		{
			string[] row = SplitCsvLine( lines[y] ); 
			for (int x = 0; x < row.Length; x++) 
			{
				outputGrid[x,y] = row[x]; 
 
				// This line was to replace "" with " in my output. 
				// Include or edit it as you wish.
				outputGrid[x,y] = outputGrid[x,y].Replace("\"\"", "\"");
			}
		}
 
		return outputGrid; 
	}
 
	// splits a CSV row 
	string[] SplitCsvLine(string line) {

		string pattern = @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)";

		return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
		pattern, 
		System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
		select m.Groups[1].Value).ToArray();
	}

}
