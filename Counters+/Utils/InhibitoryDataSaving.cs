using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using BS_Utils.Utilities;


namespace CountersPlus {
    public enum SceneState { Unknown, Menu, Game }

    // Source - BeatSaberDrinkWater
    // https://github.com/Shoko84/BeatSaberDrinkWater/blob/30a175b464d5e8c350ca381808972c8623165252/BeatSaberDrinkWater/1.6.0/IngameInformationsCounter.cs
    public class InhibitoryDataSaving : MonoBehaviour {
        public static InhibitoryDataSaving Instance;

        //public TimeSpan IngameTimeSpent { get; private set; }
        public int CurrentPlaycount { get; private set; }
        public SceneState CurrentSceneState { get; private set; } = SceneState.Menu;

        public static void OnLoad() {
            Plugin.Log("(Plugin.Log test) OnLoad() called in InhibitoryDataSaving", LogInfo.Info);
            Debug.Log("(Plugin.Log test) OnLoad() called in InhibitoryDataSaving");
            if (Instance != null) return;
            new GameObject("InhibitoryDataSaving").AddComponent<InhibitoryDataSaving>();
        }

        public void Awake() {
            Plugin.Log("(Plugin.Log test) Awake called in InhibitoryDataSaving", LogInfo.Info);
            Debug.Log("(Debug.Log test) Awake called in InhibitoryDataSaving");
            if (Instance == null) {
                Instance = this;

                BSEvents.menuSceneActive += OnMenuSceneActive;
                BSEvents.gameSceneActive += OnGameSceneActive;

                BSEvents.levelCleared += OnLevelCleared;
                BSEvents.levelFailed += OnLevelFailed;
                BSEvents.levelQuit += OnLevelQuit;
                BSEvents.levelRestarted += OnLevelRestarted;

                DontDestroyOnLoad(gameObject);
                CurrentPlaycount = 0;
                //IngameTimeSpent = new TimeSpan(0);
            } else {
                Destroy(this);
            }
        }

        private void OnDestroy() {
            BSEvents.menuSceneActive -= OnMenuSceneActive;
            BSEvents.gameSceneActive -= OnGameSceneActive;
        }

        private void Update() {
            //if (CurrentSceneState == SceneState.Game)
            //    IngameTimeSpent = IngameTimeSpent.Add(TimeSpan.FromSeconds(Time.deltaTime));
        }

        public void OnMenuSceneActive() {
            Plugin.Log("(Plugin.Log test) OnMenuSceneActive() called", LogInfo.Info);
            Debug.Log("(Debug.Log test) OnMenuSceneActive() called");
            if (CurrentSceneState == SceneState.Menu) return;
            CurrentSceneState = SceneState.Menu;
        }

        public void OnGameSceneActive() {
            Plugin.Log("(Plugin.Log test) OnGameSceneActive() called", LogInfo.Info);
            Debug.Log("(Debug.Log test) OnGameSceneActive() called");
            if (CurrentSceneState == SceneState.Game) return;
            CurrentSceneState = SceneState.Game;
        }

        private void OnLevelCleared(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2) {
            Debug.Log("OnLevelCleared() called [triggered by BSEvents.levelCleared]");
            testSaveData();
            // TODO - save data stuff here?
        }

        private void OnLevelFailed(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2) {
            Debug.Log("OnLevelFailed() called [triggered by BSEvents.levelFailed]");
            // TODO - save data stuff here?
        }

        private void OnLevelQuit(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2) {
            Debug.Log("OnLevelQuit() called [triggered by BSEvents.levelQuit]");
            // TODO - save data stuff here?
        }

        private void OnLevelRestarted(StandardLevelScenesTransitionSetupDataSO arg1, LevelCompletionResults arg2) {
            Debug.Log("OnLevelRestarted() called [triggered by BSEvents.levelRestarted]");
            // TODO - reset data stuff here?
        }

        public void PlayerHasFinishedMap() {
            CurrentPlaycount += 1;
        }

        public void ResetTimeSpent() {
            //IngameTimeSpent = new TimeSpan(0);
        }

        public void ResetPlaycount() {
            CurrentPlaycount = 0;
        }

        // Tries to save a test data set using DataSavingUtil
        private void testSaveData() {
            /* 
                "Date",                     "Order",
                "Congruent_Correct",        "Congruent_Errors",         "Congruent_Accuracy",
                "Incongruent_Correct",      "Incongruent_Errors",       "Incongruent_Accuracy"
            */
            string[] testData = {
                DateTime.Today.ToShortDateString(), "No Order",
                "3", "4", "500%",
                "6", "7", "800%"
            };
            DataSavingUtil.SaveData("testID", testData, Song.song2);
        }
    }
}