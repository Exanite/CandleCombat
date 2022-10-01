using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class AbilityDisplay : MonoBehaviour
    {
        public int AbilityIndex;

        public Image IconImage;
        public Image CooldownImage;

        private void Update()
        {
            var ability = GameContext.Instance.Abilities[AbilityIndex];
            if (ability == null)
            {
                return;
            }

            // Cooldown
            var cooldownRatio = ability.CurrentCooldown / ability.CooldownDuration;
            cooldownRatio = Mathf.Clamp01(cooldownRatio);

            CooldownImage.fillAmount = cooldownRatio;
            
            // Icon
            IconImage.sprite = ability.Icon;
        }
    }
}