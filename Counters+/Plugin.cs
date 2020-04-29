using CountersPlus.UI;
using CountersPlus.Harmony;
using IPA;
using IPA.Loader;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using CountersPlus.Utils;
using System.Reflection;
using Harmony;

namespace CountersPlus
{
    public enum LogInfo { Info, Warning, Notice, Error, Fatal };
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static SemVer.Version PluginVersion { get; private set; } = new SemVer.Version("0.0.0"); //Default.
        public static SemVer.Version WebVersion { get; internal set; } = new SemVer.Version("0.0.0"); //Default.

        internal static bool UpToDate
        {
            get { return PluginVersion >= WebVersion; }
        }

        private static HarmonyInstance harmonyInstance;
        public const string harmonyId = "com.caeden117.beatsaber.countersplus";

        
        public void Init(IPALogger log, PluginMetadata metadata)
        {
            Utils.Logger.Init(log);   
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
            MethodInfo handleCut = typeof(GameNoteController).GetMethod("HandleCut", BindingFlags.Instance | BindingFlags.NonPublic);
            var prefix = typeof(Plugin).GetMethod("PreCut", BindingFlags.Static | BindingFlags.Public);
            var postfix = typeof(Plugin).GetMethod("PostCut", BindingFlags.Static | BindingFlags.Public);
            

            
            harmonyInstance.Patch(handleCut, new HarmonyMethod(prefix), new HarmonyMethod((postfix)));

        }
        public static void PreCut(Saber saber,Vector3 cutPoint,Quaternion orientation,Vector3 cutDirVec,bool allowBadCut)
        {
            cutDirVec *= -1;
        }

        public static void PostCut()
        {
            
        }

        public void OnApplicationQuit()
        {
            if (harmonyInstance != null) harmonyInstance?.UnpatchAll(harmonyId);
        }

        public void OnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "GameCore" &&
                CountersController.settings.Enabled ) CountersController.LoadCounters();
            CountersController.LoadedCounters.Clear();
        }

        public void OnSceneLoaded(Scene arg, LoadSceneMode hiBrian) { }
        public void OnEnable() { }
        public void OnSceneUnloaded(Scene scene) { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public static void Log(string m, LogInfo l = LogInfo.Info, string suggestedAction = null)
        {
            Utils.Logger.Log(m, l, suggestedAction);
        }
    }
}
