using System;
using TMPro;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class Timer : MonoBehaviour
    {
        public TMP_Text text;

        private float timer;

        void Update()
        {
            if (GameContext.Instance.CurrentHealth > 0)
            {
                timer += Time.deltaTime;
                text.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss\.fff");
            }
        }
    }
}