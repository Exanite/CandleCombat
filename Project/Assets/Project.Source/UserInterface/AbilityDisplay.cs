using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class AbilityDisplay : GameUiBehaviour
    {
        public int AbilityIndex;
        public string KeyTextContent;

        public TMP_Text KeyText;
        public Image IconImage;
        public Image CooldownImage;
        
        private void Update()
        {
            var ability = GameContext.Abilities[AbilityIndex];
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
            
            // Key text
            KeyText.text = KeyTextContent;
        }
    }
}