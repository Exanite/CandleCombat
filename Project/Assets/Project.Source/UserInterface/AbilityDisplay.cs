using TMPro;
using UniDi;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Source.UserInterface
{
    public class AbilityDisplay : MonoBehaviour
    {
        public int AbilityIndex;
        public string KeyTextContent;

        public TMP_Text KeyText;
        public Image IconImage;
        public Image CooldownImage;
        
        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            var ability = gameContext.Abilities[AbilityIndex];
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