using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class HealthDisplay : GameUiBehaviour
    {
        public Image Image;

        private void Update()
        {
            if (GameContext == null)
            {
                return;
            }
            
            var healthRatio = Mathf.Clamp01(GameContext.CurrentHealth / GameContext.MaxHealth);

            Image.fillAmount = healthRatio;
        }
    }
}