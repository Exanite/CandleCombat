using UnityEngine;

namespace Project.Source.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/BurningShot")]
    public class BurningShotAbility : Ability
    {
        public override void Execute()
        {
            var player = GameContext.Instance.CurrentPlayer;
            if (player == null)
            {
                return;
            }

            // TODO: Don't reach into directly.
            var gunController = GameContext.Instance.gameObject.GetComponent<GunController>();
            gunController.SwitchAmmo(1);
        }
    }
}