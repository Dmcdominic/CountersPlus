using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;


public enum Task {DayNight, GoNoGo, Flanker};
public enum TrialType {Pretest, Posttest, DelayedTest, Delayed2Test, Showcase};


public static class DataSavingUtil {
	static string DataFilePath = @"ResearchData/PrePostTestData.csv"; // If you change this, make sure to adjust the writeData method accordingly
	static string BackupCogTasksDataPath = @"ResearchData/BackupCogTasksData/";

	// Static strings declared here to ensure consistency for GainsHeader calculations
	static string Heartrate = "Heartrate";
    static string GNG_TotalAccuracy = "TotalAccuracy";
    static string DN_Incongruent_Accuracy = "Incongruent_Accuracy";
	static string F_Incongruent_Accuracy = "Incongruent_Accuracy";

	//static string changesInHR = "changesInHR";
	//static string DN_gains = "DN_gains";
	//static string GNG_gains = "GNG_gains";
	//static string F_gains = "F_gains";

	// Task Data Headers -- NO IN-STRING COMMAS ALLOWED (And NO DASHES)
	static string[] InfoHeader = {  "ID",  "Version",  "DOB",  "Age",  "Sex",  "Computer Distance"  };


	// Revised data headers
	static string[] DayNightHeader = {	
		"Date",						"Order",
		"Congruent_Correct",		"Congruent_Errors",			"Congruent_Accuracy",		"Congruent_RT",
		"Incongruent_Correct",		"Incongruent_Errors",       DN_Incongruent_Accuracy,	"Incongruent_RT"
	};

    static string[] GoNoGoHeader = {
        "Date",
        "TotalCorrect",             "TotalErrors",
        "GoTrialsTotal",            "NoGoTrialsTotal",
        "GoCorrect",                "NoGoCorrect",
        "GoTotalErrors",            "NoGoTotalErrors",
        "GoAccuracy",               "NoGoAccuracy",
        GNG_TotalAccuracy,
        "RT",                       Heartrate
    };

    static string[] FlankerHeader = {	
		/* Hey Carter, I commented out heartrate from the flanker header (and data array). */
		//Heartrate,
		"Date",
		"Correct",					 "Errors",
		"Congruent_Accuracy",		 F_Incongruent_Accuracy,
		"Congruent_RT",				 "Incongruent_RT",
        "Total_Incongruent_Trials",  "Total_Congruent_Trials",
        "Total_Errors_Incongruent",  "Total_Errors_Congruent",
        "Total_Correct_Incongruent", "Total_Correct_Congruent"
	};

	// If you change this, make sure to adjust the computeGainsData method accordingly
	//static string[] GainsHeader = {
	//	changesInHR,
	//	//DN_gains,                   GNG_gains,              F_gains
	//	DN_gains,					F_gains
	//};

	// Don't change this - CSV uses commas by definition
	static char[] fieldSeparator = { ',' };

	// Column pre/post/delayed test indices
	static int DNPreIndex;
    static int GNGPreIndex;
    static int FPreIndex;

	static int DNPostIndex;
    static int GNGPostIndex;
    static int FPostIndex;

	//static int GainsIndex;

	static int DNDelayIndex;
    static int GNGDelayIndex;
    static int FDelayIndex;

	static int DNDelay2Index;
    static int GNGDelay2Index;
    static int FDelay2Index;

	public static TrialType CurrentTrialType;

	public static string version = "";
	public static string birthdate = "";
	public static string sex = "";
	public static string computerDistance = "";


	// Call this to save trial data into the .csv file
	public static void SaveData(string ID, string[] newData, Task task) {
		if (CurrentTrialType == TrialType.Showcase) {
			return;
		}

		if (string.Equals (ID, "")) {
			ID = "(No ID entered)";
		}

		string[] newInfoArray = createInfoArray (ID);

		// Save backup of the data
		SaveBackup (ID, newInfoArray, newData, task);

		// Read the current data and make sure the header is correct
		string[][] AllData = readCSVFile (DataFilePath);
		makeDataHeader ().CopyTo (AllData, 0);

		// Get ID
		int lineIndex = findID (AllData, ID);

		// If ID not found in the previous data, save in a new line
		if (lineIndex < 0) {
			string[][] newAllData = new string[AllData.Length + 1][];
			AllData.CopyTo (newAllData, 0);

			string[] newLine = new string[ AllData[0].Length ];
			lineIndex = newAllData.Length - 1;
			newAllData [lineIndex] = newLine;
			newInfoArray.CopyTo (newAllData [lineIndex], 0);

			AllData = newAllData;
		}


		// Find the column index for the data, and check if the newData is the right length
		int colIndex = 0;
		if (task == Task.DayNight) {
			if (newData.Length != DayNightHeader.Length) {
				Debug.LogError ("Unexpected data array length for DayNight task data saving");
			}

			if (CurrentTrialType == TrialType.Pretest) {
				colIndex = DNPreIndex;
			} else if (CurrentTrialType == TrialType.Posttest) {
				colIndex = DNPostIndex;
			} else if (CurrentTrialType == TrialType.DelayedTest) {
				colIndex = DNDelayIndex;
			} else if (CurrentTrialType == TrialType.Delayed2Test) {
				colIndex = DNDelay2Index;
			}
        } else if (task == Task.GoNoGo) {
            if (newData.Length != GoNoGoHeader.Length) {
                Debug.LogError("Unexpected data array length for GoNoGo task data saving");
            }

            if (CurrentTrialType == TrialType.Pretest) {
                colIndex = GNGPreIndex;
            } else if (CurrentTrialType == TrialType.Posttest) {
                colIndex = GNGPostIndex;
            } else if (CurrentTrialType == TrialType.DelayedTest) {
                colIndex = GNGDelayIndex;
            } else if (CurrentTrialType == TrialType.Delayed2Test) {
                colIndex = GNGDelay2Index;
            }
        } else if (task == Task.Flanker) {
			if (newData.Length != FlankerHeader.Length) {
				Debug.LogError ("Unexpected data array length for Flanker task data saving");
			}

			if (CurrentTrialType == TrialType.Pretest) {
				colIndex = FPreIndex;
			} else if (CurrentTrialType == TrialType.Posttest) {
				colIndex = FPostIndex;
			} else if (CurrentTrialType == TrialType.DelayedTest) {
				colIndex = FDelayIndex;
			} else if (CurrentTrialType == TrialType.Delayed2Test) {
				colIndex = FDelay2Index;
			}
		}

		// Copy into the data array
		newData.CopyTo (AllData [lineIndex], colIndex);

        // Once the posttest is done, compute gains since the pretest
        //if (CurrentTrialType == TrialType.Posttest) {
        //    AllData[lineIndex] = computeGainsData(AllData[lineIndex], task);
        //}

        // Write it to the file
        string dataString = convert2DToString(AllData);
		writeData(dataString);
	}


	// Read the data from a .csv and return it as a string[][]
	private static string[][] readCSVFile (string path) {
		string[][] allData;
		if (File.Exists (path)) {
			string[] lines = File.ReadAllLines (path);
			if (lines.Length > 1) {
				allData = new string[lines.Length][];

				for (int i=0; i < lines.Length; i++) {
					string[] fields = lines [i].Split (fieldSeparator);
					allData [i] = new string[fields.Length];
					fields.CopyTo (allData [i], 0);
				}
				return allData;
			}
		}

		// File has not been initialized
		allData = new string[1][];
		return allData;
	}

	// Search for a string[] with the same ID at index 0
	private static int findID(string[][] data, string ID) {
		for (int i = 1; i < data.Length; i++) {
			if (data [i] [0] == ID) {
				return i;
			}
		}

		return -1;
	}

	// Create the info array
	private static string[] createInfoArray (string ID) {
		string[] infoArray = new string[InfoHeader.Length];

		int i = 0;
		infoArray [i] = ID;					i++;
		infoArray [i] = version;			i++;
		infoArray [i] = birthdate;			i++;

		DateTime result;
		if (DateTime.TryParse (birthdate, out result)) {
			// Compute age
			DateTime today = DateTime.Today;
			TimeSpan dayDiff = today.Subtract (result);
			double years = dayDiff.TotalDays / 365.25;
			infoArray [i] = years.ToString ("##.###");
		} else {
			infoArray [i] = "";
		}
		i++;

		infoArray [i] = sex;				i++;
		infoArray [i] = computerDistance;	i++;

		// Reset the global strings
		version = "";
		birthdate = "";
		sex = "";
		computerDistance = "";

		// Make SURE there are no commas
		for (int j = 0; j < infoArray.Length; j++) {
			Debug.Log(infoArray);
			infoArray[j] = infoArray[j].Replace(',', ' ');
		}

		return infoArray;
	}

	/* // Update the subject info if necessary
	private static void updateInfoLine (string[] newData) {
		
	} */

	// Set up the header for the data CSV
	private static string[][] makeDataHeader() {
		string[][] header = new string[1][];
		//int lineLength = InfoHeader.Length + GainsHeader.Length + (DayNightHeader.Length + GoNoGoHeader.Length + FlankerHeader.Length) * 4;
		int lineLength = InfoHeader.Length + (DayNightHeader.Length + GoNoGoHeader.Length + FlankerHeader.Length) * 4;
		header[0] = new string[lineLength];

		// Adding prefixes to headers --- No dashes or commas allowed
		string[] DN_PreHeader = addPrefix (DayNightHeader, "DN_Pre_");
		string[] DN_PostHeader = addPrefix (DayNightHeader, "DN_Post_");
		string[] DN_DelayHeader = addPrefix (DayNightHeader, "DN_Delay_");
		string[] DN_Delay2Header = addPrefix(DayNightHeader, "DN_Delay2_");

        string[] GNG_PreHeader = addPrefix(GoNoGoHeader, "GNG_Pre_");
        string[] GNG_PostHeader = addPrefix(GoNoGoHeader, "GNG_Post_");
        string[] GNG_DelayHeader = addPrefix(GoNoGoHeader, "GNG_Delay_");
        string[] GNG_Delay2Header = addPrefix(GoNoGoHeader, "GNG_Delay2_");

        string[] F_PreHeader = addPrefix (FlankerHeader, "F_Pre_");
		string[] F_PostHeader = addPrefix (FlankerHeader, "F_Post_");
		string[] F_DelayHeader = addPrefix (FlankerHeader, "F_Delay_");
		string[] F_Delay2Header = addPrefix(FlankerHeader, "F_Delay2_");

		// Copy the headers at their correct indices, and store the indices along the way
		int index = 0;
		
		InfoHeader.CopyTo (header [0], index);				index += InfoHeader.Length;				DNPreIndex = index;

		DN_PreHeader.CopyTo (header [0], index);			index += DayNightHeader.Length;			GNGPreIndex = index;
		GNG_PreHeader.CopyTo (header [0], index);			index += GoNoGoHeader.Length;			FPreIndex = index;
		F_PreHeader.CopyTo (header [0], index);				index += FlankerHeader.Length;			DNPostIndex = index;

		DN_PostHeader.CopyTo (header [0], index);			index += DayNightHeader.Length;			GNGPostIndex = index;
		GNG_PostHeader.CopyTo (header [0], index);			index += GoNoGoHeader.Length;		    FPostIndex = index;
		F_PostHeader.CopyTo (header [0], index);			index += FlankerHeader.Length;			//GainsIndex = index;

		/*GainsHeader.CopyTo(header[0], index);				index += GainsHeader.Length;*/			DNDelayIndex = index;

		DN_DelayHeader.CopyTo (header [0], index);			index += DayNightHeader.Length;			GNGDelayIndex = index;
		GNG_DelayHeader.CopyTo (header [0], index);			index += GoNoGoHeader.Length;		    FDelayIndex = index;
		F_DelayHeader.CopyTo (header [0], index);			index += FlankerHeader.Length;			DNDelay2Index = index;

		DN_Delay2Header.CopyTo(header[0], index);			index += DayNightHeader.Length;			GNGDelay2Index = index;
		GNG_Delay2Header.CopyTo(header[0], index);			index += GoNoGoHeader.Length;		    FDelay2Index = index;
		F_Delay2Header.CopyTo(header[0], index);			index += FlankerHeader.Length;

		// Output the final result
		return header;
	}

	// Add a prefix to every string in a string[]
	private static string[] addPrefix (string[] array, string prefix) {
		string[] newArray = new string[array.Length];
		for (int i = 0; i < array.Length; i++) {
			newArray[i] = prefix + array[i];
		}
		return newArray;
	}

	// Compute the gains data for a given task (between pretest and posttest)
	// Returns the updated string[] data row
	//private static string[] computeGainsData(string[] dataRow, Task task) {
	//	if (task == Task.DayNight) {
	//		// Calculate and save DayNight Incongruent Accuracy gains
	//		int dn_gains_index = GainsIndex + Array.IndexOf(GainsHeader, DN_gains);

	//		int DN_header_index_of = Array.IndexOf(DayNightHeader, DN_Incongruent_Accuracy);
	//		int DN_Pre_Incongruent_Accuracy_Index = DNPreIndex + DN_header_index_of;
	//		int DN_Post_Incongruent_Accuracy_Index = DNPostIndex + DN_header_index_of;

	//		string pre_string = dataRow[DN_Pre_Incongruent_Accuracy_Index];
	//		string post_string = dataRow[DN_Post_Incongruent_Accuracy_Index];
	//		pre_string = pre_string.Substring(0, pre_string.Length - 1);
	//		post_string = post_string.Substring(0, post_string.Length - 1);

	//		float pre_data = 0;
	//		float post_data = 0;
	//		bool parsed = true;
	//		parsed = parsed && float.TryParse(pre_string, out pre_data);
	//		parsed = parsed && float.TryParse(post_string, out post_data);

	//		if (parsed) {
	//			dataRow[dn_gains_index] = (post_data - pre_data).ToString() + "%";
	//		}

	//	//} else if (task == Task.GoNoGo) {
	//	//	// Calculate and save GoNoGo Accuracy gains
	//	//	int gng_gains_index = GainsIndex + Array.IndexOf(GainsHeader, GNG_gains);

	//	//	int GNG_header_index_of = Array.IndexOf(GoNoGoHeader, GNG_TotalAccuracy);
	//	//	int GNG_Pre_Accuracy_Index = GNGPreIndex + GNG_header_index_of;
	//	//	int GNG_Post_Accuracy_Index = GNGPostIndex + GNG_header_index_of;

	//	//	string pre_string = dataRow[GNG_Pre_Accuracy_Index];
	//	//	string post_string = dataRow[GNG_Post_Accuracy_Index];
	//	//	pre_string = pre_string.Substring(0, pre_string.Length - 1);
	//	//	post_string = post_string.Substring(0, post_string.Length - 1);

	//	//	float pre_data = 0;
	//	//	float post_data = 0;
	//	//	bool parsed = true;
	//	//	parsed = parsed && float.TryParse(pre_string, out pre_data);
	//	//	parsed = parsed && float.TryParse(post_string, out post_data);

	//	//	if (parsed) {
	//	//		dataRow[gng_gains_index] = (post_data - pre_data).ToString() + "%";
	//	//	}

	//	//	// Calculate and save Heartrate gains
	//	//	int HR_index = GainsIndex + Array.IndexOf(GainsHeader, changesInHR);

	//	//	int HR_header_index_of = Array.IndexOf(GoNoGoHeader, Heartrate);
	//	//	int HR_Pre_Index = GNGPreIndex + HR_header_index_of;
	//	//	int HR_Post_Index = GNGPostIndex + HR_header_index_of;

	//	//	int pre_data_int = 0;
	//	//	int post_data_int = 0;
	//	//	parsed = true;
	//	//	parsed = parsed && int.TryParse(dataRow[HR_Pre_Index], out pre_data_int);
	//	//	parsed = parsed && int.TryParse(dataRow[HR_Post_Index], out post_data_int);

	//	//	if (parsed) {
	//	//		dataRow[HR_index] = (post_data_int - pre_data_int).ToString();
	//	//	}

	//	} else if (task == Task.Flanker) {
	//		// Calculate and save DayNight Incongruent Accuracy gains
	//		int f_gains_index = GainsIndex + Array.IndexOf(GainsHeader, F_gains);

	//		int F_header_index_of = Array.IndexOf(FlankerHeader, F_Incongruent_Accuracy);
	//		int F_Pre_Incongruent_Accuracy_Index = FPreIndex + F_header_index_of;
	//		int F_Post_Incongruent_Accuracy_Index = FPostIndex + F_header_index_of;

	//		string pre_string = dataRow[F_Pre_Incongruent_Accuracy_Index];
	//		string post_string = dataRow[F_Post_Incongruent_Accuracy_Index];
	//		pre_string = pre_string.Substring(0, pre_string.Length - 1);
	//		post_string = post_string.Substring(0, post_string.Length - 1);

	//		float pre_data = 0;
	//		float post_data = 0;
	//		bool parsed = true;
	//		parsed = parsed && float.TryParse(pre_string, out pre_data);
	//		parsed = parsed && float.TryParse(post_string, out post_data);

	//		if (parsed) {
	//			dataRow[f_gains_index] = (post_data - pre_data).ToString() + "%";
	//		}

	//	}

	//	return dataRow;
	//}

	// Convert string[][] to string
	private static string convert2DToString(string[][] TwoDArray) {
		string output = "";
		foreach (string[] line in TwoDArray) {
			output = output + string.Join (",", line) + "\n";
		}
		return output;
	}

	// Write the input string to the DataFilePath
	private static void writeData (string dataString) {
		if (!Directory.Exists (@"ResearchData")) {
			Debug.Log ("@\"ResearchData/\" folder does not yet exist. Creating it...");
			Directory.CreateDirectory (@"ResearchData");
		}

		if (!File.Exists (@"ResearchData/PrePostTestData.csv")) {
			Debug.Log ("@\"ResearchData/PrePostTestData.csv\" file does not yet exist. Creating it...");
			FileStream fs = File.Create (@"ResearchData/PrePostTestData.csv");
			fs.Close ();
		}

		File.WriteAllText (DataFilePath, dataString);
		Debug.Log ("Trial data successfully saved");
	}

	// Save the new line of data to the backup folder for the kiddo
	private static void SaveBackup (string ID, string[] infoLine, string[] newData, Task task) {
		string infoString = string.Join (",", infoLine);
		string dataString = string.Join (",", newData);

		dataString = string.Concat (infoString, ",", task.ToString(), ",", CurrentTrialType.ToString(), ",", dataString, "\n");

		string path = BackupCogTasksDataPath + ID + ".csv";
		if (!File.Exists (path)) {
			Directory.CreateDirectory (BackupCogTasksDataPath);
		}
		File.AppendAllText (path, dataString);
		Debug.Log ("Backup cognitive tasks data saved");
	}
}
