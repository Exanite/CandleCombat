using UniDi;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class HealthDisplay : MonoBehaviour
    {
        public Image Image;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            var healthRatio = Mathf.Clamp01(gameContext.CurrentHealth / gameContext.MaxHealth);

            Image.fillAmount = healthRatio;
        }
    }
}