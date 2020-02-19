﻿using CountersPlus.UI;
using Harmony;
using IPA;
using IPA.Loader;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using CountersPlus.Utils;

namespace CountersPlus
{
    public enum LogInfo { Info, Warning, Notice, Error, Fatal };
    public class Plugin : IBeatSaberPlugin
    {
        public static SemVer.Version PluginVersion { get; private set; } = new SemVer.Version("0.0.0"); //Default.
        public static SemVer.Version WebVersion { get; internal set; } = new SemVer.Version("0.0.0"); //Default.

        internal static bool UpToDate
        {
            get { return PluginVersion >= WebVersion; }
        }

        private static HarmonyInstance harmonyInstance;
        public const string harmonyId = "com.caeden117.beatsaber.countersplus";

        public void Init(IPALogger log, PluginLoader.PluginMetadata metadata)
        {
            Logger.Init(log);   
            PluginVersion = metadata?.Version;
        }

        public void OnApplicationStart()
        {
            try
            {
                harmonyInstance = HarmonyInstance.Create(harmonyId);
                harmonyInstance.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Log($"{ex.Message}", LogInfo.Fatal, "Unable to apply Harmony patches. Did you even install BSIPA correctly?");
            }

            Log($"OnLoad() now getting called for CountersController");
            CountersController.OnLoad();
            Log($"OnLoad() now getting called for InhibitoryDataSaving");
            InhibitoryDataSaving.OnLoad();
            Log($"Both OnLoad() calls returned");
            MenuUI.CreateUI();
        }

        public void OnApplicationQuit()
        {
            if (harmonyInstance != null) harmonyInstance?.UnpatchAll(harmonyId);
        }

        public void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "GameCore" &&
                CountersController.settings.Enabled &&
                (!UnityEngine.Resources.FindObjectsOfTypeAll<PlayerDataModelSO>()
                    .FirstOrDefault()?
                    .playerData.playerSpecificSettings.noTextsAndHuds ?? true)
                ) CountersController.LoadCounters();
            CountersController.LoadedCounters.Clear();
        }

        public void OnSceneLoaded(Scene arg, LoadSceneMode hiBrian) { }
        public void OnEnable() { }
        public void OnSceneUnloaded(Scene scene) { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public static void Log(string m, LogInfo l = LogInfo.Info, string suggestedAction = null)
        {
            Logger.Log(m, l, suggestedAction);
        }
    }
}
