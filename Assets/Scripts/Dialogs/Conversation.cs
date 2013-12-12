using UnityEngine;
using System.Collections;

public class Conversation {

	// the two conversing Personae
	public string id { get; set; }

	// the last timestamp of the conversation
	public float lastTime { get; set; }
	public float timeoutLength = 300.0f; // 5 minutes

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

	public Conversation(string _id) { 
		setup(_id, timeoutLength);
	}

	public Conversation(string _id, float _timeoutLength) {
		setup(_id, _timeoutLength);
	}

	void setup(string _id, float _timeoutLength) {
		id = _id;
		timeoutLength = _timeoutLength;
	}

}
