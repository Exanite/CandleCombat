using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.Source
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