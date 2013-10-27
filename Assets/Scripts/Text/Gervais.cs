using UnityEngine;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

public class Gervais : MonoBehaviour {



	public static string obscurify(string inputString) {

		StringBuilder outputString = new StringBuilder(inputString);

		int wordCount = Regex.Matches(inputString, @"[\S]+").Count;
		int randomWordIndex = (int)Random.Range(0,wordCount);

		int wordIndex = 0;

		for(int i=0; i<inputString.Length; i++) {

			char c = outputString[i];

			// keep digits as is
			if (char.IsDigit(c)) continue;

			// count words
			if (char.IsSeparator(c)) {
				wordIndex++;
				continue;
			}

			// randomly let one word through
			if (randomWordIndex == wordIndex) continue;

			switch((int)Random.Range(0.0f,10.0f)) {
				case 0 :	outputString[i] = '#';	break;
				case 1 :	outputString[i] = '#';	break;
				case 2 :	outputString[i] = '.';	break;
				case 3 :	outputString[i] = ';';	break;
				case 4 :	outputString[i] = ':';	break;
				case 5 :	outputString[i] = '%';	break;
				case 6 :	outputString[i] = '*';	break;
				case 7 :	outputString[i] = '$';	break;
				case 8 :	outputString[i] = '*';	break;
				case 9 :	outputString[i] = '&';	break;
			}
		}

		return outputString.ToString();

	}



	public static string colorize(string inputString) {

		string outputString = "";

		foreach (char str in inputString) {
	    	outputString += "<color=#FF0000>" + str + "</color>";
		}

		return outputString;

	}

}
