using TMPro;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class AmmoDisplay : MonoBehaviour
    {
        public TMP_Text Text;

        private void Update()
        {
            var currentAmmo = GameContext.Instance.PlayerGunController.GetCurrentAmmo();
            var maxAmmo = GameContext.Instance.PlayerGunController.GetMaxAmmo();

            Text.text = $"{currentAmmo} / {maxAmmo}";
        }
    }
}