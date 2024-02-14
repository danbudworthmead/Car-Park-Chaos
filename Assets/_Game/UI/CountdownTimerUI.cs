using _Game.Game_States_Logic;
using UnityEngine;

namespace _Game.UI
{
    public class CountdownTimerUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text timerText;
        
        private int _lastTime = 4;
        
        private void Update()
        {
            var round = MatchManager.Singleton.CurrentRound;
            if (round != null && round.RoundState == RoundManager.RoundStates.Countdown)
            {
                timerText.enabled = true;
                var time = Mathf.CeilToInt(round.CountdownTimer);

                if (_lastTime != time)
                {
                    _lastTime = time;
                    NewTime();
                }
            }
            else
            {
                timerText.enabled = false;
            }
        }

        private void NewTime()
        {
            timerText.text = _lastTime.ToString();
        }
    }
}
