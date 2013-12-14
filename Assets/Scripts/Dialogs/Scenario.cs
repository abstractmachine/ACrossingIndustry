using UnityEngine;
using System.Collections;
using System.Collections.Generic; // used for Dictionary
using System.Text.RegularExpressions; // used for Regex
using System.Linq;

public class Scenario : MonoBehaviour {


	// Instance

    private static Scenario instance;

    public static Scenario Instance {
        get { 
            return instance ?? (instance = new GameObject("Scenario").AddComponent<Scenario>()); 
        }
    }



    // Database

	Dictionary<string,Conversation> conversations = new Dictionary<string,Conversation>();


    public void LoadDialogues() {

    	AddConversation("Ouvrier", "Pivert");
    	AddConversation("Ouvrier", "ColonAPied");

    }



	// Access

	public List<string> GetPhrases(string dialogID, bool isPlayer) {

		// if there is nothing with this key
		if (!checkID(dialogID)) {
			print("Error: unknown key in Dictionary (" + dialogID + ")");
			return new List<string>();
		}

		Conversation conversation = conversations[dialogID];

		return conversation.GetPhrases(isPlayer);

	}


	public void Choose(string dialogID, string chosenPhrase) {

		Conversation conversation = conversations[dialogID];
		int index = conversation.getNextSpeechActIndex(chosenPhrase);
		SetIndex(dialogID, index);

		//SetIndex(dialogID, Index(dialogID)+1);

	}


	public bool IsNeutral(string dialogID) {

		return (Index(dialogID) == 0);
	
	}



	public void Reset(string dialogID) {

		SetIndex(dialogID,0);

	}


	public void StartConversation(string dialogID) {

		SetIndex(dialogID,1);

	}



	// Internals

	bool checkID(string dialogID) {

		// ok, found the key
		if (conversations.ContainsKey(dialogID)) return true;

		// if we've never started this conversation
		return false;

	}


	void AddConversation(string playerId, string personaId) {

		string dialogID = playerId + "-" + personaId;

		// generate a dictionary entry for this instance
		conversations.Add(dialogID, new Conversation(playerId,personaId));

	}



	int Index(string dialogID) {

		// has this conversation timed out?
		if (conversations[dialogID].TimedOut()) {
			Reset(dialogID);
		}

		return conversations[dialogID].Index;

	}


	void SetIndex(string dialogID, int index) {

		conversations[dialogID].Index = index;

	}

}
