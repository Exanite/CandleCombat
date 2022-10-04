using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/SoulTransfer")]
    public class SoulTransferAbility : Ability
    {
        [SerializeField] private int soulGunIndex = 2;
        
        public override void Execute()
        {
            // Debug.Log("SoulTransferAbility");

            var player = GameContext.Instance.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            // TODO: Don't reach into Ability directly.
            var gunController = GameContext.Instance.gameObject.GetComponent<GunController>();
            int previousGun = gunController.EquippedGunIndex;
            gunController.SwitchGun(soulGunIndex);
            gunController.Fire();
            gunController.SwitchGun(previousGun);
        }
    }
}