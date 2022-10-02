using TMPro;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class AmmoDisplay : MonoBehaviour
    {
        public TMP_Text Text;

        private void Update()
        {
            var isLoading = GameContext.Instance.PlayerGunController.IsReloading();
            var currentAmmo = GameContext.Instance.PlayerGunController.GetCurrentAmmo();
            var maxAmmo = GameContext.Instance.PlayerGunController.GetMaxAmmo();

            if (isLoading)
            {
                Text.text = "Reloading!";
            }
            else
            {
                Text.text = $"{currentAmmo} / {maxAmmo}";
            }
        }
    }
}