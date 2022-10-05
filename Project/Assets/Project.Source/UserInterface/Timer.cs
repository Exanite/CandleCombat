using System;
using TMPro;
using UniDi;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class Timer : MonoBehaviour
    {
        public TMP_Text text;

        private float timer;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            if (gameContext.CurrentHealth > 0)
            {
                timer += Time.deltaTime;
                text.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\.fff");
            }
        }
    }
}