using UnityEngine;
using UnityEngine.Rendering;

namespace Project.Source.Visuals
{
    public class HealthGrayoutDisplay : MonoBehaviour
    {
        public Volume Volume;
        public AnimationCurve GrayoutCurve;

        private void Update()
        {
            var healthRatio = Mathf.Clamp01(GameContext.Instance.CurrentHealth / GameContext.Instance.MaxHealth);
            var weight = Mathf.Clamp01(GrayoutCurve.Evaluate(1 - healthRatio));

            Volume.weight = weight;
        }
    }
}