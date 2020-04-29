﻿using System;
using UnityEngine;
using TMPro;
using CountersPlus.Config;
using CountersPlus.Utils;

namespace CountersPlus.Counters
{
    class MissedCounter : Counter<MissedConfigModel>
    {
        private BeatmapObjectManager beatmapObjectManager;
        private TMP_Text missedText;
        private TMP_Text label;
        private int counter;

        internal override void Counter_Start() { }

        internal override void Init(CountersData data, Vector3 position)
        {
            beatmapObjectManager = data.BOM;
            TextHelper.CreateText(out missedText, position - new Vector3(0, 0.4f, 0));
            missedText.text = "0";
            missedText.fontSize = 4;
            missedText.color = Color.white;
            missedText.alignment = TextAlignmentOptions.Center;

            GameObject labelGO = new GameObject("Counters+ | Missed Label");
            labelGO.transform.parent = transform;
            TextHelper.CreateText(out label, position);
            label.text = "Misses";
            label.fontSize = 3;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;

            if (settings.CustomMissTextIntegration && PluginUtility.IsPluginPresent("CustomMissText"))
                UpdateCustomMissText();
            else if (!PluginUtility.IsPluginPresent("CustomMissText"))
            {
                settings.CustomMissTextIntegration = false;
                settings.Save();
            }

            if (beatmapObjectManager != null)
            {
                beatmapObjectManager.noteWasMissedEvent += OnNoteMiss;
                beatmapObjectManager.noteWasCutEvent += OnNoteCut;
            }
        }

        internal override void Counter_Destroy()
        {
            beatmapObjectManager.noteWasMissedEvent -= OnNoteMiss;
            beatmapObjectManager.noteWasCutEvent -= OnNoteCut;
        }

        private void OnNoteCut(INoteController data, NoteCutInfo info)
        {
            if (!info.allIsOK && data.noteData.noteType != NoteType.Bomb) IncrementCounter();
        }

        private void OnNoteMiss(INoteController data)
        {
            if (data.noteData.noteType != NoteType.Bomb)
            {
                IncrementCounter();
                if (settings.CustomMissTextIntegration) UpdateCustomMissText();
            }
        }

        private void UpdateCustomMissText()
        {
            try
            {
                // Couldn't find anywhere to download CustomMissText so this is the easiest way to get rid of the compiler errors
                //if (PluginUtility.IsPluginPresent("CustomMissText"))
                //    label.text = String.Join(" ", CustomMissText.Plugin.allEntries[UnityEngine.Random.Range(0, CustomMissText.Plugin.allEntries.Count)]);
            }
            catch { }
        }

        private void IncrementCounter()
        {
            counter++;
            missedText.text = counter.ToString();
        }
    }
}
