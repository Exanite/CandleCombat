using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class UiScaleHandler : MonoBehaviour
    {
        private static readonly string ScaleName = "Project_UI_Scale";

        public CanvasScaler Scaler;

        private void Start()
        {
            Scaler.scaleFactor = PlayerPrefs.HasKey(ScaleName) ? PlayerPrefs.GetInt(ScaleName) : 1;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.KeypadPlus) || UnityEngine.Input.GetKeyDown(KeyCode.Plus) || UnityEngine.Input.GetKeyDown(KeyCode.Equals))
            {
                PlayerPrefs.SetInt(ScaleName, 2);
                Scaler.scaleFactor = 2;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.KeypadMinus) || UnityEngine.Input.GetKeyDown(KeyCode.Minus))
            {
                PlayerPrefs.SetInt(ScaleName, 1);
                Scaler.scaleFactor = 1;
            }
        }
    }
}