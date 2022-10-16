using Project.Source.Gameplay.Guns;
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
            switch (gameContext.PlayerGunController.GunState)
            {
                case GunState.Reloading:
                {
                    Text.text = "Reloading!";

                    break;
                }
                case GunState.Switching:
                {
                    Text.text = "Switching!";

                    break;
                }
                case GunState.Ready:
                default:
                {
                    var currentAmmo = gameContext.PlayerGunController.GetCurrentAmmo();
                    var maxAmmo = gameContext.PlayerGunController.GetMaxAmmo();

                    Text.text = $"{currentAmmo} / {maxAmmo}";

                    break;
                }
            }
        }
    }
}