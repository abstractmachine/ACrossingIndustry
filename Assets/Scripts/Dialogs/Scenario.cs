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

	public List<string> GetPhrases(string dialogID, bool activeOrPassive) {

		checkID(dialogID);

		if (activeOrPassive) return GetPhrasesActive(dialogID);
		else return GetPhrasesPassive(dialogID);

	}


	List<string> GetPhrasesActive(string dialogID) {

		switch(Index(dialogID)) {

			case 0 : return new List<string>(){"Hello"};
			case 1 : return new List<string>(){"I'm fine. And you?", "Asshole, Go Fuck Off!", "I dunno. I think I broke my leg."};
			case 2 : return new List<string>(){"That's good."};
			default: return new List<string>(){"..."};

		}

	}


	List<string> GetPhrasesPassive(string dialogID) {

		switch(Index(dialogID)) {

			case 0	: return new List<string>(){"Oh hey it's you. Hello", "Hello", "Hi There!", "Hey..."};
			case 1	: return new List<string>(){"It's a nice day isn't it. How are you?"};
			case 2	: return new List<string>(){"I'm doing ok."};
			default	: return new List<string>(){"..."};

		}

	}


	public void Choose(string dialogID, int index) {

		print("Multiple choice answer:" + index + "\n" + GetPhrasesActive(dialogID)[index]);

		SetIndex(dialogID, Index(dialogID)+1);

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
