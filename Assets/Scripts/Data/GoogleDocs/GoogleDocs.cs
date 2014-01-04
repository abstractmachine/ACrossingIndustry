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
// Note: I replaced the buggy Newtonsoft.Json dll that Google furnished with the 2.0 equivalent downloaded from:
// http://json.codeplex.com
// Unfortunately, newer versions such as 3.5 or 4.0 caused other conflicts

////////////////////////

public class GoogleDocs : MonoBehaviour {

	bool connected = false;
	SpreadsheetsService spreadsheetService = null;
	SpreadsheetEntry spreadsheet = null;

	XmlData xml;

	///////////////


	void Awake () {

		xml = gameObject.GetComponent<XmlData>();
		
	}


	///////////////
	
	
	void Update () {
		
		// List all available spreadsheet documents on GoogleDocs
		if (Input.GetKeyDown(KeyCode.L)) {
			GetListOfSpreadsheets();
		}

		// re-download spreadsheets from Google Docs
		if (Input.GetKeyDown(KeyCode.G)) {
			// down and save into local xml files
			ImportGoogleDocs();
			// reload the new local xml files
			xml.LoadXml();
		}
		
	}


	///////////////
	
	
	void ImportGoogleDocs() {

		// get the list of dialog names from Google
		LoadFromGoogleIntoXml("Dialog Names", "dialog_names.xml");
		// update the list of available dialogs
		Dictionary<string,DialogData> dialogs = xml.DialogNames();

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

	}	


	///////////////


	void LoadFromGoogleIntoXml(string spreadsheetName, string xmlFilename) {

		// make sure we're connected to Google's servers
		if (!connected) Connect();
		if (!connected) return;

		if (!loadSpreadsheet(spreadsheetName)) return;

		// get the feed containing all the worksheets in this spreadsheet
		WorksheetFeed worksheetFeed = spreadsheet.Worksheets;
		// Get the first worksheet of the first spreadsheet.
		WorksheetEntry worksheetEntry = (WorksheetEntry)worksheetFeed.Entries[0];

		// Define the URL to request the list feed of the worksheet.
		AtomLink listFeedLink = worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

		// Fetch the list feed of the worksheet.
		ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
		ListFeed listFeed = spreadsheetService.Query(listQuery);

		string filepath = Application.dataPath + "/Dialogues/Resources/" + xmlFilename;
		using (FileStream stream = File.Open(filepath, FileMode.Create)) {
			listFeed.SaveToXml(stream);
		}

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


	void GetListOfSpreadsheets() {

		if (!connected) Connect();
		if (!connected) return;

		// Create query to get all spreadsheets.
		SpreadsheetQuery getSpreadsheetsQuery = new SpreadsheetQuery();
		// Call to API to get all spreadsheets.
		SpreadsheetFeed spreadsheets = spreadsheetService.Query(getSpreadsheetsQuery);

		// Iterate through returned spreadsheets and find sheet of interest.
		foreach(SpreadsheetEntry spreadsheet in spreadsheets.Entries) {
			Debug.Log(spreadsheet.Title.Text);
		}

	}


	void SaveList(string filepath, string output) {
		//System.IO.File.WriteAllText(path, output);
	}


	///////////////


	void Connect() {

		string login = GetLogin();
		string pass = GetPassword();

		if (login == "" || pass == "") {
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
	
		string login = GetFileContents(@"../Google/", "login.txt", "your_name_here@gmail.com");
		return login;
	
	}


	string GetPassword() {

		string defaultString = "app_specific_password cf: https://support.google.com/accounts/answer/185833?hl=en";

		string pass = GetFileContents(@"../Google/", "pass.txt", defaultString);
		return pass;

	}


	string GetFileContents(string folderPath, string filename, string defaultValue) {

#if !UNITY_WEBPLAYER

		// if it doesn't exist
		if (!System.IO.Directory.Exists(folderPath)) {
			// create it
			System.IO.Directory.CreateDirectory(folderPath);
			print("Please create Google credentials files 'login.txt' and 'pass.txt' in '../Google/' folder.");
			// return empty string
			return "";
		}

		// the path to our login
		string filepath = System.IO.Path.Combine(folderPath, filename);
		// if it doesn't exist
		if (!System.IO.File.Exists(filepath)) {
			// create the file, fill it with default contents
			System.IO.File.WriteAllText(folderPath,defaultValue);
		}

		return System.IO.File.ReadAllText(filepath);

#else

		return null;

#endif

	}


}

