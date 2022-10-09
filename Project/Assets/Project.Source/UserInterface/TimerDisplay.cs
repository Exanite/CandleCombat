using System;
using TMPro;
using UniDi;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class TimerDisplay : MonoBehaviour
    {
        public TMP_Text text;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            text.text = TimeSpan.FromSeconds(gameContext.TimeAlive).ToString(@"mm\:ss\.fff");
        }
    }
}