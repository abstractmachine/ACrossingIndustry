using UnityEngine;
using System.Collections;

public class Condition : MonoBehaviour {


		public bool Check(string rawString) {
		
				// first, figure out how many conditions there are
				string[] conditions = Converter.SeparateStrings('&', rawString);
				
				// make sure there's at least one condition to compare
				if (conditions.Length == 0)
						return true;
		
				bool result = false;
		
				foreach(string condition in conditions) {
					
						string key = Converter.StringToFunction(condition);
						string value = Converter.StringToArgument(condition);
					
						// check this key
						bool thisResult = Check(key, value);
					
						// if any one of these false, return false
						if (!thisResult)
								return false;
						
						// check for true in the others
						result = true;
					
				}
				
				return result;
		
		}
		
		
		public bool Check(string key, string value) {
		
				// figure out if we're using a negation operator
				bool isNot = false;
				if (key.Length > 0 && key[0] == '!') {
						isNot = true;
						// remove negation
						key.TrimStart(new char[]{ '!' });
				}
		
				// the result of the comparison
				bool result = false;
				
				value = value.Trim(new char[]{ ' ', '"' });
				key = key.ToLower();
		
				switch(key) {
				
						case "amount":
								result = Amount(value);
						break;
				
						case "close":
								result = Close(value);
						break;
						
						case "east":
								result = East(value);
						break;
				
						case "far":
								result = Far(value);
						break;
				
						case "have":
								result = Have(value);
						break;
				
						case "inside":
								result = Inside(value);
						break;
				
						case "know":
								result = Know(value);
						break;
				
						case "near":
								result = Near(value);
						break;
				
						case "north":
								result = North(value);
						break;
				
						case "south":
								result = South(value);
						break;
						
						case "west":
								result = West(value);
						break;
						
				}
		
				// re-calculate result based on not operator
				return isNot ? result : !result;
		
		}
		
		
		
		bool HasQuotes(string value) {
		
				if (value.Length > 0 && value[0] == '"') {
						return true;
				}
		
				return false;
		
		}
		
		
		bool Amount(string value) {
		
				return false;			
		}
		
		
		bool Close(string value) {
		
				return false;			
		}
		
		
		bool East(string value) {
		
				if (Direction(value) == "East")
						return true;
				else
						return false;
		
		}
		
		
		bool Far(string value) {
				return false;
		}
		
		
		bool Have(string value) {
				return false;			
		}
		
		
		bool Inside(string value) {
				return false;			
		}
		
		
		bool Know(string value) {
				return false;			
		
		}
		
		
		bool Near(string value) {
		
				return false;			
		}
		
		
		bool North(string value) {
		
				if (Direction(value) == "North")
						return true;
				else
						return false;
		
		}
		
		
		bool South(string value) {
		
				if (Direction(value) == "South")
						return true;
				else
						return false;
		
		}
		
		
		bool West(string value) {
		
				if (Direction(value) == "West")
						return true;
				else
						return false;
		
		}
		
		string Direction(string value) {
				
				// first, get a pointer to this object
				GameObject target = GameObject.Find(value);
				
				if (target == null) {
						print("No GameObject with name " + value);
						return "";
				}
				
				// get the position of the object
				Vector3 targetPosition = target.transform.position;
				
				print(targetPosition);
				print(transform.position);
				
				float horizontal = targetPosition.x - transform.position.x;
				float vertical = targetPosition.z - transform.position.z;
				
				// are we on the vertical axis?
				if (Mathf.Abs(horizontal) < Mathf.Abs(vertical)) {
						if (vertical > 0)
								return "North";
						else
								return "South";
				} else { // ok, must be the horizontal axis
						if (horizontal > 0)
								return "East";
						else
								return "West";
				}
				
		}
	
}
