using _Game.Game_States_Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    private RoundManager roundManager;

    private void Update()
    {
        float? time = MatchManager.Singleton.CurrentRound?.timer;
        if (time.HasValue)
        {
            var timespan = TimeSpan.FromSeconds((double)time);
            timerText.text = timespan.ToString(@"mm\:ss");
        }
        else
            timerText.text = string.Empty;
    }
}
