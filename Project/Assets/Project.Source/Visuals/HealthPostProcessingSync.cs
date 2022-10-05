using UniDi;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.Source.Visuals
{
    public class HealthPostProcessingSync : MonoBehaviour
    {
        public Volume Volume;
        public AnimationCurve GrayoutCurve;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            var healthRatio = Mathf.Clamp01(gameContext.CurrentHealth / gameContext.MaxHealth);
            var weight = Mathf.Clamp01(GrayoutCurve.Evaluate(1 - healthRatio));

            Volume.weight = weight;
        }
    }
}