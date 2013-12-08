using UnityEngine;
using System.Collections;
using System.Collections.Generic; // used for Dictionary

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



	// Access

	public string GetPhrase(string dialogID, bool activeOrPassive) {

		checkID(dialogID);

		if (activeOrPassive) return GetPhraseActive(dialogID);
		else return GetPhrasePassive(dialogID);

	}


	public void Next(string dialogID) {

		checkID(dialogID);

		// TODO: check to see if this is at the end of the index

		SetIndex(dialogID, Index(dialogID)+1);

	}


	public void Reset(string dialogID) {

		checkID(dialogID);

		SetIndex(dialogID,0);

	}



	// Internals

	void checkID(string dialogID) {

		// if we've never started this conversation
		if (!conversations.ContainsKey(dialogID)) {
			AddConversation(dialogID);
		}

	}


	void AddConversation(string dialogID) {
		
		// generate a dictionary entry for this instance
		// start the index at 0 (beginning of phrasebook)
		conversations.Add(dialogID, new Conversation(dialogID));

	}


	string GetPhraseActive(string dialogID) {

		switch(Index(dialogID)) {

			case 0 : return "Hello";
			case 1 : return "I'm fine. And you?";
			case 2 : return "That's good.";
			default: return "...";

		}

	}


	string GetPhrasePassive(string dialogID) {

		switch(Index(dialogID)) {

			case 0	: return "Oh hey it's you. Hello";
			case 1	: return "How are you?";
			case 2	: return "I'm doing ok.";
			default	: return "...";

		}

	}



	int Index(string dialogID) {

		// has this conversation timed out?
		if (conversations[dialogID].TimedOut()) {
			Reset(dialogID);
		}

		return conversations[dialogID].Index;

	}


	void SetIndex(string dialogID, int index) {

		//conversations[dialogID] = index;

		conversations[dialogID].Index = index;

	}

}
