using UnityEngine;
using System.Collections;
using System.Collections.Generic; // used for Dictionary
using System.Text.RegularExpressions; // used for Regex
using System.Linq;

public class Scenario : MonoBehaviour {

    // state
    bool loaded = false;
    // Database
	Dictionary<string,Conversation> conversations = new Dictionary<string,Conversation>();


	// Singleton
    private static Scenario instance;
    public static Scenario Instance {
        get { 
            return instance ?? (instance = new GameObject("Scenario").AddComponent<Scenario>()); 
        }
    }


    public void LoadDialogues() {

    	// if we've already loaded, forget it
    	if (loaded) return;

    	AddConversation("Ouvrier", "Pivert");
    	AddConversation("Ouvrier", "ColonAPied");
    	AddConversation("Ouvrier", "PivertIndicateur");

    	loaded = true;

    }



	// Access

	public List<string> GetPhrases(string dialogID, string instanceID, bool isPlayer) {

		// if there is nothing with this key
		if (!checkID(dialogID,instanceID)) {
			print("Error: unknown key in Dictionary (" + dialogID + ")");
			return new List<string>();
		}

		Conversation conversation = conversations[dialogID];

		return conversation.GetPhrases(isPlayer);

	}


	public void Choose(string dialogID, string instanceID, string chosenPhrase) {

		Conversation conversation = conversations[dialogID];
		int index = conversation.getNextSpeechActIndex(chosenPhrase);
		SetIndex(dialogID,instanceID,index);

		//SetIndex(dialogID, Index(dialogID)+1);

	}


	public bool IsNeutral(string dialogID, string instanceID) {

		return (Index(dialogID,instanceID) == 0);
	
	}



	public void Reset(string dialogID, string instanceID) {

		SetIndex(dialogID,instanceID,0);

	}


	public void StartConversation(string dialogID, string instanceID) {

		SetIndex(dialogID,instanceID,1);

	}



	// Internals

	bool checkID(string dialogID, string instanceID) {

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



	int Index(string dialogID, string instanceID) {

		// TODO: replace dialogID in conversations[dialogID] with indexes[instanceId];

		// has this conversation timed out?
		if (conversations[dialogID].TimedOut()) {
			Reset(dialogID,instanceID);
		}

		return conversations[dialogID].Index;

	}


	void SetIndex(string dialogID, string instanceID, int index) {

		// TODO: replace dialogID in conversations[dialogID] with indexes[instanceId];

		conversations[dialogID].Index = index;

	}

}
