using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic; // required for List
using System.Text.RegularExpressions; // used for Regex
using System.Linq;  // used for Regex

public class Parser : MonoBehaviour {

	// Instance

    private static Parser instance;

    public static Parser Instance {
        get { 
            return instance ?? (instance = new GameObject("Parser").AddComponent<Parser>()); 
        }
    }

/*
    class SpeechAct {
   		public int index;
   		public List<SpeechItem> speeches;
   		public List<ConditionItem> conditions;
    }

   	class SpeechItem {
   		public List<SpeechItem> repliesTo = new List<SpeechItem>();	// if Personae, repliesTo.Length == 0
   		public string phrase;
   		public string action;
   		public bool isPersonae() { return (repliesTo.Count == 0); }
   		public bool isPlayer() { return (repliesTo.Count > 0); }
   	}

   	class ConditionItem {
   		public SpeechItem conditionOf;
   		public string condition;
   		public List<int> next;
   	}
*/


    public void LoadDialogues() {
 
		ParsePhrases("Ouvrier", "Taxi");

	}


	void ParsePhrases(string player, string persona) {

		// figure out the filename by concatenating the two names
		string filename = player + "-" + persona;
		// load this dialog tree
		TextAsset txt = (TextAsset)Resources.Load(filename, typeof(TextAsset));
		// error handling
		if (txt == null) {
			print("Can't read file " + filename);
			return;
		}
		// create a grid out of this CSV file
		string[,] grid = SplitCsvGrid(txt.text);

		// how many lines is that?
		int numberOfLines = grid.GetUpperBound(1);
		print("Number of lines = " + numberOfLines);

		// the line numbers continue over several lines
		int currentIndex = 0;
		/*string currentPersonaSays = "";
		string currentPlayerSays = "";
		string currentPersonaAction = "";
		string currentPlayerAction = "";
		string currentCondition = "";
		string currentNextIndex = "";*/

		// go through the data
		for(int i=1; i<grid.GetUpperBound(1); i++) {
			
			// if there is a string value
			string index = grid[0,i];
			if (index != "") currentIndex = System.Convert.ToInt32(index);

			string personaSays = parseString(grid[1,i]);
//			string playerSays = parseString(grid[2,i]);
//			string personaAction = grid[3,i];
//			string playerAction = grid[4,i];
//			string condition = grid[5,i];
//			string nextIndex = grid[6,i];

			if (personaSays == "") {

			}

		}

	}


	// TODO: This should have been dealt with in the Regex
	string parseString(string phrase) {

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
