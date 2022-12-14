using Project.Source.Gameplay.Guns;
using TMPro;

namespace Project.Source.UserInterface
{
    public class AmmoDisplay : GameUiBehaviour
    {
        public TMP_Text Text;

        private void Update()
        {
            if (GameContext == null)
            {
                return;
            }
            
            switch (GameContext.PlayerGunController.GunState)
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
                    var currentAmmo = GameContext.PlayerGunController.GetCurrentAmmo();
                    var maxAmmo = GameContext.PlayerGunController.GetMaxAmmo();

                    Text.text = $"{currentAmmo} / {maxAmmo}";

                    break;
                }
            }
        }
    }
}