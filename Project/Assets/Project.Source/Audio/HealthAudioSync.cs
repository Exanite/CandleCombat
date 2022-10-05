using UniDi;
using UnityEngine;
using UnityEngine.Audio;

namespace Project.Source.Audio
{
    public class HealthAudioSync : MonoBehaviour
    {
        public AudioMixer Mixer;
        public AudioMixerSnapshot NormalSnapshot;
        public AudioMixerSnapshot LowHpSnapshot;

        public AnimationCurve Curve;

        private AudioMixerSnapshot[] snapshots;
        private float[] snapshotWeights;

        [Inject]
        private GameContext gameContext;

        private void Start()
        {
            snapshots = new AudioMixerSnapshot[] { NormalSnapshot, LowHpSnapshot };
            snapshotWeights = new float[] { 1, 0 };
        }

        private void Update()
        {
            var healthRatio = Mathf.Clamp01(gameContext.CurrentHealth / gameContext.MaxHealth);
            var inverseHealthRatio = 1 - healthRatio;

            snapshotWeights[0] = Mathf.Clamp01(1 - Curve.Evaluate(inverseHealthRatio));
            snapshotWeights[1] = Mathf.Clamp01(Curve.Evaluate(inverseHealthRatio));

            Mixer.TransitionToSnapshots(snapshots, snapshotWeights, 0.1f);
        }
    }
}