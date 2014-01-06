/* 
 * Google Doc Spreadsheet Reader
 * (cc) Douglas Edric Stanley / Atelier Hypermédia École supérieure d'art d'Aix en Provence
 *
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic; // <List>

// for stream writer
using System;
using System.IO;
using System.Xml;

// Google Doc dll libraries were downlaoded from:
// https://code.google.com/p/google-gdata/downloads/detail?name=Google_Data_API_Setup_2.2.0.0.msi
using Google.GData.Client;
using Google.GData.Extensions;
using Google.GData.Spreadsheets;
// Note: IReplaced buggy Newtonsoft.Json dll Google furnished with the 2.0 equivalent downloaded from:
// http://json.codeplex.com
// Newer versions such as 3.5 or 4.0 caused other conflicts, notably with A* Pathfinding

////////////////////////

public class GoogleDocs : MonoBehaviour {

	bool connected = false;
	SpreadsheetsService spreadsheetService = null;
	SpreadsheetEntry spreadsheet = null;

	Data data;

	///////////////


	void Awake () {

		data = gameObject.GetComponent<Data>();
		
	}


	///////////////
	
	
	void Update () {
		
		// List all available spreadsheet documents on GoogleDocs
		if (Input.GetKeyDown(KeyCode.L)) {
			DumpListOfSpreadsheets();
		}

		// re-download spreadsheets from Google Docs
		if (Input.GetKeyDown(KeyCode.G)) {
			// down and save into local xml files
			bool imported = ImportGoogleDocs();
			// if error
			if (!imported) {
				print("Couldn't import from Google Docs");
			} else {
				// reload the new local xml files
				data.LoadXml();
			}

		}
		
	}


	///////////////
	
	
	bool ImportGoogleDocs() {

		// get the list of dialog names from Google
		bool loaded = LoadFromGoogleIntoXml("Dialog Names", "dialog_names.xml");
		// if some error, return false
		if (!loaded) return false;
		// update the list of available dialogs
		Dictionary<string,DialogData> dialogs = data.DialogNames();

		// get all the remote google dialogs
		foreach(KeyValuePair<string,DialogData> dialogEntry in dialogs) {
		//foreach(DialogData dialog in dialogs) {
			string dialogId = dialogEntry.Key;
			// save each one locally
			LoadFromGoogleIntoXml(dialogId, dialogId + ".xml");
		}

		// get the remote list of personae and save it locally
		LoadFromGoogleIntoXml("Dialog Personae", "dialog_personae.xml");

		print("Finished Reloading Google Docs Data");

		return true;

	}	


	///////////////


	bool LoadFromGoogleIntoXml(string spreadsheetName, string xmlFilename) {

		// make sure we're connected to Google's servers
		if (!connected) Connect();
		// if we couldn't connect, abort
		if (!connected) return false;

		if (!loadSpreadsheet(spreadsheetName)) return false;

		// get the feed containing all the worksheets in this spreadsheet
		WorksheetFeed worksheetFeed = spreadsheet.Worksheets;
		// Get the first worksheet of the first spreadsheet.
		WorksheetEntry worksheetEntry = (WorksheetEntry)worksheetFeed.Entries[0];

		// Define the URL to request the list feed of the worksheet.
		AtomLink listFeedLink = worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

		// Fetch the list feed of the worksheet.
		ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
		ListFeed listFeed = spreadsheetService.Query(listQuery);

#if UNITY_EDITOR

		string filepath = Application.dataPath + @"/Dialogues/Resources/" + xmlFilename;

#elif UNITY_STANDALONE_OSX

		string folderpath = Application.dataPath + @"/Data/Xml";
		if (!System.IO.Directory.Exists(folderpath)) System.IO.Directory.CreateDirectory(folderpath);
		string filepath = folderpath + @"/" + xmlFilename;

#elif UNITY_STANDALONE_WIN

		string folderpath = Application.dataPath + @"\Data\Xml";
		if (!System.IO.Directory.Exists(folderpath)) System.IO.Directory.CreateDirectory(folderpath);
		string filepath = folderpath + @"\" + xmlFilename;

#endif

		using (FileStream stream = File.Open(filepath, FileMode.Create)) {
			listFeed.SaveToXml(stream);
			//print("Saving " + filepath);
		}

		return true;

	}


	bool loadSpreadsheet(string spreadsheetName) {

		// reset current spreadsheet
		spreadsheet = null;

		// Create query to get all spreadsheets.
		SpreadsheetQuery getSpreadsheetsQuery = new SpreadsheetQuery();
		// Call to API to get all spreadsheets.
		SpreadsheetFeed spreadsheets = spreadsheetService.Query(getSpreadsheetsQuery);
		// Iterate through returned spreadsheets and find sheet of interest.
		foreach(SpreadsheetEntry sheet in spreadsheets.Entries) {
			// if this is it 
			if(string.Compare(sheet.Title.Text, spreadsheetName) == 0) {
				spreadsheet = sheet;
				return true;
			}
		}

		// Sheet not found
		Debug.LogError("Could not find spreadsheet named " + spreadsheetName);
		return false;

	}


	bool DumpListOfSpreadsheets() {

		// connect to Google Data
		if (!connected) Connect();
		// if we couldn't connect, abort
		if (!connected) return false;

		// Create query to get all spreadsheets.
		SpreadsheetQuery getSpreadsheetsQuery = new SpreadsheetQuery();
		// Call to API to get all spreadsheets.
		SpreadsheetFeed spreadsheets = spreadsheetService.Query(getSpreadsheetsQuery);

		// Iterate through returned spreadsheets and find sheet of interest.
		foreach(SpreadsheetEntry spreadsheet in spreadsheets.Entries) {
			Debug.Log(spreadsheet.Title.Text);
		}

		return true;

	}


	void SaveList(string filepath, string output) {
		//System.IO.File.WriteAllText(path, output);
	}


	///////////////


	void Connect() {

		string login = GetLogin();
		string pass = GetPassword();

		if (login == "" || login == null) {
			Debug.Log("Error: invalid/empty login.");
			//CreateLoginFile();
			return;
		}

		if (pass == "" || pass == null) {
			Debug.Log("Error: invalid/empty password");
			//CreatePasswordFile();
			return;
		}

		SecurityCertificatePolicy.Instate();
        spreadsheetService = new SpreadsheetsService("ACrossingIndustry");
		spreadsheetService.setUserCredentials(login, pass);

		// if we couldn't get the service, abort
		if (spreadsheetService == null) {
			Debug.Log("Couldn't connect to Google Services");
			return;
		}

		connected = true;

	}


	// To generate application-specific passwords:
	// cf. https://support.google.com/accounts/answer/185833?hl=en

	string GetLogin() {
	
		string login = "";
		string defaultString = "your_name_here@gmail.com";

#if UNITY_EDITOR

		login = GetFileContents(@"../Google/", "login.txt", defaultString);

#elif UNITY_STANDALONE_OSX

		string folderpath = Application.dataPath + @"/Data/Google/";
		if (!System.IO.Directory.Exists(folderpath)) {
			CreateLoginFile();
			CreatePasswordFile();
		}

		if (System.IO.File.Exists(@"../../Google/login.txt"))  {
			login = GetFileContents(@"../../Google/", "login.txt", defaultString);
		} else {
			login = GetFileContents(folderpath, "login.txt", defaultString);
		}

		/*if (!System.IO.Directory.Exists(folderpath))  {
			string login = GetFileContents(@"./Google/", "login.txt", "your_name_here@gmail.com");
		}*/

#elif UNITY_STANDALONE_WIN

		string folderpath = Application.dataPath + @"\Data\Google\";
		if (!System.IO.Directory.Exists(folderpath)) {
			CreateLoginFile();
			CreatePasswordFile();
		}

		if (System.IO.File.Exists(@"..\..\Google\login.txt"))  {
			login = GetFileContents(@"..\..\Google\", "login.txt", defaultString);
		} else {
			login = GetFileContents(folderpath, "login.txt", defaultString);
		}
		/*if (!System.IO.Directory.Exists(folderpath))  {
			string login = GetFileContents(@".\Google\", "login.txt", "your_name_here@gmail.com");
		}*/

#endif

		return login;
	
	}



	void CreateLoginFile() {

#if UNITY_EDITOR


#elif UNITY_STANDALONE_OSX

		string folderpath = Application.dataPath + @"/Data/Google";
		if (!System.IO.Directory.Exists(folderpath)) System.IO.Directory.CreateDirectory(folderpath);
		string filepath = folderpath + @"/" + "login.txt";
		System.IO.File.WriteAllText(filepath, "google_login_here");

#elif UNITY_STANDALONE_WIN

		string folderpath = Application.dataPath + @"\Data\Google";
		if (!System.IO.Directory.Exists(folderpath)) System.IO.Directory.CreateDirectory(folderpath);
		string filepath = folderpath + @"\" + "login.txt";
		System.IO.File.WriteAllText(filepath, "google_login_here");

#endif
		
	}



	string GetPassword() {
	
		string pass = "";
		string defaultString = "app_specific_password cf: https://support.google.com/accounts/answer/185833?hl=en";
	
#if UNITY_EDITOR

		pass = GetFileContents(@"../Google/", "pass.txt", defaultString);

#elif UNITY_STANDALONE_OSX

		string folderpath = Application.dataPath + @"/Data/Google/";
		if (!System.IO.Directory.Exists(folderpath)) {
			CreateLoginFile();
			CreatePasswordFile();
		}

		if (System.IO.File.Exists(@"../../Google/pass.txt"))  {
			pass = GetFileContents(@"../../Google/", "pass.txt", defaultString);
		} else {
			pass = GetFileContents(folderpath, "pass.txt", defaultString);
		}

#elif UNITY_STANDALONE_WIN

		string folderpath = Application.dataPath + @"\Data\Google\";
		if (!System.IO.Directory.Exists(folderpath)) {
			CreateLoginFile();
			CreatePasswordFile();
		}

		if (System.IO.File.Exists(@"..\..\Google\pass.txt"))  {
			pass = GetFileContents(@"..\..\Google\", "pass.txt", defaultString);
		} else {
			pass = GetFileContents(folderpath, "pass.txt", defaultString);
		}

#endif

		return pass;

	}



	void CreatePasswordFile() {
	
#if UNITY_EDITOR


#elif UNITY_STANDALONE_OSX

		string folderpath = Application.dataPath + @"/Data/Google";
		if (!System.IO.Directory.Exists(folderpath)) System.IO.Directory.CreateDirectory(folderpath);
		string filepath = folderpath + @"/" + "pass.txt";
		System.IO.File.WriteAllText(filepath, "google_app-specific_password_here");

#elif UNITY_STANDALONE_WIN

		string folderpath = Application.dataPath + @"\Data\Google";
		if (!System.IO.Directory.Exists(folderpath)) System.IO.Directory.CreateDirectory(folderpath);
		string filepath = folderpath + @"\" + "pass.txt";
		System.IO.File.WriteAllText(filepath, "google_app-specific_password_here");

#endif
		
	}



	string GetFileContents(string folderPath, string filename, string defaultValue) {

#if !UNITY_WEBPLAYER

		// if it doesn't exist
		if (!System.IO.Directory.Exists(folderPath)) {
			// create it
			//System.IO.Directory.CreateDirectory(folderPath);
			print("Please create Google credentials files 'login.txt' and 'pass.txt' in '../Google/' folder.");
			// return empty string
			return "";
		}

		// the path to our login
		string filepath = System.IO.Path.Combine(folderPath, filename);
		// if it doesn't exist
		if (!System.IO.File.Exists(filepath)) {
			// create the file, fill it with default contents
			print("Please create Google credentials files 'login.txt' and 'pass.txt' in '../Google/' folder.");
			//System.IO.File.WriteAllText(folderPath,defaultValue);
		}

		return System.IO.File.ReadAllText(filepath);

#else

		Debug.Log("Cannot import while on WebPlayer build. Please switch Build Settings to a different player.");

		return null;

#endif

	}


}

