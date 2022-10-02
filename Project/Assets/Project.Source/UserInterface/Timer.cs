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
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (GameContext.Instance.CurrentHealth > 0)
            {
                text.text = Time.time.ToString("#.00");
            }

        }
    }
}