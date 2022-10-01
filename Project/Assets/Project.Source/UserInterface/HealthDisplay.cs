using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class HealthDisplay : MonoBehaviour
    {
        public Image Image;

        private void Update()
        {
            var healthRatio = GameContext.Instance.CurrentHealth / GameContext.Instance.MaxHealth;
            healthRatio = Mathf.Clamp01(healthRatio);
            
            Image.fillAmount = healthRatio;
        }
    }
}