using TMPro;
using UniDi;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class AmmoDisplay : MonoBehaviour
    {
        public TMP_Text Text;
        
        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            var isLoading = gameContext.PlayerGunController.IsReloading();
            var currentAmmo = gameContext.PlayerGunController.GetCurrentAmmo();
            var maxAmmo = gameContext.PlayerGunController.GetMaxAmmo();

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