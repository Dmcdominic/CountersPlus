using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;



namespace CountersPlus {
    public enum Song { song0, song1, song2 };
    public enum TrialType { Pretest, Posttest, DelayedTest, Delayed2Test, Showcase };

    public static class DataSavingUtil {
        static string DataFilePath = @"ResearchData/BeatSaberData.csv"; // If you change this, make sure to adjust the writeData method accordingly
        static string BackupDataPath = @"ResearchData/BackupBeatSaberData/";

        // Task Data Headers -- NO IN-STRING COMMAS ALLOWED (And NO DASHES)
        static string[] InfoHeader = { "ID", "Version", "DOB", "Age", "Sex" };

        // Revised data headers
        static string[] BeatSaberSongHeader = {
            "Date",                     "Order",
            "Congruent_Correct",        "Congruent_Errors",         "Congruent_Accuracy",
            "Incongruent_Correct",      "Incongruent_Errors",       "Incongruent_Accuracy"
        };

        static readonly int numSongs = Enum.GetNames(typeof(Song)).Length;
        static int[] headerIndices = new int[numSongs];

        // Don't change this - CSV uses commas by definition
        static char[] fieldSeparator = { ',' };

        public static TrialType CurrentTrialType;

        public static string version = "";
        public static string birthdate = "";
        public static string sex = "";


        // Call this to save trial data into the .csv file
        public static void SaveData(string ID, string[] newData, Song song) {
            Debug.Log("SaveData called");
            if (CurrentTrialType == TrialType.Showcase) {
                return;
            }

            if (string.Equals(ID, "")) {
                ID = "(No ID entered)";
            }

            string[] newInfoArray = createInfoArray(ID);

            // Save backup of the data
            SaveBackup(ID, newInfoArray, newData, song);

            // Read the current data and make sure the header is correct
            string[][] AllData = readCSVFile(DataFilePath);
            makeDataHeader().CopyTo(AllData, 0);

            // Get ID
            int lineIndex = findID(AllData, ID);

            // If ID not found in the previous data, save in a new line
            if (lineIndex < 0) {
                string[][] newAllData = new string[AllData.Length + 1][];
                AllData.CopyTo(newAllData, 0);

                string[] newLine = new string[AllData[0].Length];
                lineIndex = newAllData.Length - 1;
                newAllData[lineIndex] = newLine;
                newInfoArray.CopyTo(newAllData[lineIndex], 0);

                AllData = newAllData;
            }


            // Find the column index for the data, and check if the newData is the right length
            int colIndex = headerIndices[(int)song];
            if (newData.Length != BeatSaberSongHeader.Length) {
                Debug.LogError("Unexpected data array length for BeatSaberSongHeader data saving");
            }

            // Copy into the data array
            newData.CopyTo(AllData[lineIndex], colIndex);

            // Write it to the file
            string dataString = convert2DToString(AllData);
            writeData(dataString);
        }


        // Read the data from a .csv and return it as a string[][]
        private static string[][] readCSVFile(string path) {
            string[][] allData;
            if (File.Exists(path)) {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length > 1) {
                    allData = new string[lines.Length][];

                    for (int i = 0; i < lines.Length; i++) {
                        string[] fields = lines[i].Split(fieldSeparator);
                        allData[i] = new string[fields.Length];
                        fields.CopyTo(allData[i], 0);
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
                if (data[i][0] == ID) {
                    return i;
                }
            }

            return -1;
        }

        // Create the info array
        private static string[] createInfoArray(string ID) {
            string[] infoArray = new string[InfoHeader.Length];

            int i = 0;
            infoArray[i] = ID; i++;
            infoArray[i] = version; i++;
            infoArray[i] = birthdate; i++;

            DateTime result;
            if (DateTime.TryParse(birthdate, out result)) {
                // Compute age
                DateTime today = DateTime.Today;
                TimeSpan dayDiff = today.Subtract(result);
                double years = dayDiff.TotalDays / 365.25;
                infoArray[i] = years.ToString("##.###");
            } else {
                infoArray[i] = "";
            }
            i++;

            infoArray[i] = sex; i++;

            // Reset the global strings
            version = "";
            birthdate = "";
            sex = "";

            // Make SURE there are no commas
            for (int j = 0; j < infoArray.Length; j++) {
                Debug.Log(infoArray);
                infoArray[j] = infoArray[j].Replace(',', ' ');
            }

            return infoArray;
        }

        // Set up the header for the data CSV
        private static string[][] makeDataHeader() {
            string[][] header = new string[1][];
            int lineLength = InfoHeader.Length + (BeatSaberSongHeader.Length) * numSongs;
            header[0] = new string[lineLength];

            // Adding prefixes to headers --- No dashes or commas allowed
            // Then copy the headers at their correct indices, and store the indices along the way
            int index = 0;
            InfoHeader.CopyTo(header[0], index); index += InfoHeader.Length;

            string[] songs = Enum.GetNames(typeof(Song));
            for (int i = 0; i < songs.Length; i++) {
                headerIndices[i] = index;
                addPrefix(BeatSaberSongHeader, songs[i]).CopyTo(header[0], index);
                index += BeatSaberSongHeader.Length;
            }

            // Output the final result
            return header;
        }

        // Add a prefix to every string in a string[]
        private static string[] addPrefix(string[] array, string prefix) {
            string[] newArray = new string[array.Length];
            for (int i = 0; i < array.Length; i++) {
                newArray[i] = prefix + array[i];
            }
            return newArray;
        }

        // Convert string[][] to string
        private static string convert2DToString(string[][] TwoDArray) {
            string output = "";
            foreach (string[] line in TwoDArray) {
                output = output + string.Join(",", line) + "\n";
            }
            return output;
        }

        // Write the input string to the DataFilePath
        private static void writeData(string dataString) {
            if (!Directory.Exists(@"ResearchData")) {
                Debug.Log("@\"ResearchData/\" folder does not yet exist. Creating it...");
                Directory.CreateDirectory(@"ResearchData");
            }

            if (!File.Exists(@"ResearchData/BeatSaberData.csv")) {
                Debug.Log("@\"ResearchData/BeatSaberData.csv\" file does not yet exist. Creating it...");
                FileStream fs = File.Create(@"ResearchData/BeatSaberData.csv");
                fs.Close();
            }

            File.WriteAllText(DataFilePath, dataString);
            Debug.Log("Trial data successfully saved");
        }

        // Save the new line of data to the backup folder for the participant
        private static void SaveBackup(string ID, string[] infoLine, string[] newData, Song song) {
            string infoString = string.Join(",", infoLine);
            string dataString = string.Join(",", newData);

            dataString = string.Concat(infoString, ",", song.ToString(), ",", CurrentTrialType.ToString(), ",", dataString, "\n");

            string path = BackupDataPath + ID + ".csv";
            if (!File.Exists(path)) {
                Directory.CreateDirectory(BackupDataPath);
            }
            File.AppendAllText(path, dataString);
            Debug.Log("Backup cognitive tasks data saved");
        }
    }
}