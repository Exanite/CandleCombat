using Project.Source.UserInterface;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.Source.Visuals
{
    public class HealthPostProcessingSync : GameUiBehaviour
    {
        public Volume Volume;
        public AnimationCurve GrayoutCurve;
        
        private void Update()
        {
            var healthRatio = GameContext ? Mathf.Clamp01(GameContext.CurrentHealth / GameContext.MaxHealth) : 1;
            var weight = Mathf.Clamp01(GrayoutCurve.Evaluate(1 - healthRatio));

            Volume.weight = weight;
        }
    }
}