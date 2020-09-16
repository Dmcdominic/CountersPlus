﻿using CountersPlus.ConfigModels;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static CountersPlus.Utils.Accessors;

namespace CountersPlus.Counters
{
    internal class ProgressCounter : Counter<ProgressConfigModel>, ITickable
    {
        private readonly Vector3 ringSize = Vector3.one * 1.175f;
        private readonly string multiplierImageSpriteName = "Circle";

        [Inject] private AudioTimeSyncController atsc;
        [Inject(Optional = true)] private CoreGameHUDController coreGameHUD; // For getting multiplier image
        [Inject] private GameplayCoreSceneSetupData gcssd; // I hope this works

        private TMP_Text timeText;
        private Image progressRing;
        private float length = 0;
        private float songBPM = 100;

        public override void CounterInit()
        {
            timeText = CanvasUtility.CreateTextFromSettings(Settings);
            timeText.fontSize = 4;

            length = atsc.songLength;
            songBPM = gcssd.difficultyBeatmap.level.beatsPerMinute;

            if (coreGameHUD != null)
            {
                GameObject baseGameProgress = SongProgressPanelGO(ref coreGameHUD);
                UnityEngine.Object.Destroy(baseGameProgress); // I'm sorry, little one.
            }

            if (Settings.Mode != ProgressMode.Percent)
            {
                var canvas = CanvasUtility.GetCanvasFromID(Settings.CanvasID);
                if (canvas != null)
                {
                    Image backgroundImage = CreateRing(canvas).GetComponent<Image>();
                    backgroundImage.rectTransform.anchoredPosition = timeText.rectTransform.anchoredPosition;
                    backgroundImage.CrossFadeAlpha(0.05f, 1f, false);
                    backgroundImage.transform.localScale = ringSize / 10;
                    backgroundImage.type = Image.Type.Simple;

                    progressRing = CreateRing(canvas).GetComponent<Image>();
                    progressRing.rectTransform.anchoredPosition = timeText.rectTransform.anchoredPosition;
                    progressRing.transform.localScale = ringSize / 10;
                }
            }
        }

        public void Tick()
        {
            var time = atsc.songTime;
            if (Settings.ProgressTimeLeft) time = length - time;
            if (time <= 0f) return;
            if (Settings.Mode == ProgressMode.Original || Settings.Mode == ProgressMode.TimeInBeats)
            {
                if (Settings.Mode == ProgressMode.TimeInBeats)
                {
                    float beats = Mathf.Round(songBPM / 60 * time / 0.25f) * 0.25f;
                    timeText.text = beats.ToString("F2");
                }
                else
                {
                    timeText.text = $"{Math.Floor(time / 60):N0}:{Math.Floor(time % 60):00}";
                }
                if (Settings.IncludeRing)
                {
                    progressRing.fillAmount = time / length;
                }
                else
                {
                    progressRing.fillAmount = atsc.songTime / length;
                }
            }
            else
            {
                timeText.text = $"{time / length * 100:00}%";
            }
        }

        private Image CreateRing(Canvas canvas)
        {
            // Unfortunately, there is no garauntee that I have the CoreGameHUDController, since No Text and Huds
            // completely disables it from spawning. So, to be safe, we recreate this all from scratch.
            Image newImage = new GameObject("Ring Image", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            newImage.sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name == multiplierImageSpriteName);
            newImage.transform.SetParent(canvas.transform, false);
            newImage.type = Image.Type.Filled;
            newImage.fillClockwise = true;
            newImage.fillOrigin = 2;
            newImage.fillMethod = Image.FillMethod.Radial360;
            return newImage;
        }
    }
}
