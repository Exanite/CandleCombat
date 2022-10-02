using UnityEngine;
using UnityEngine.Audio;

namespace Project.Source
{
    public class HealthAudioSync : MonoBehaviour
    {
        public AudioMixer Mixer;
        public AudioMixerSnapshot NormalSnapshot;
        public AudioMixerSnapshot LowHpSnapshot;

        public AnimationCurve Curve;

        private AudioMixerSnapshot[] snapshots;
        private float[] snapshotWeights;

        private void Start()
        {
            snapshots = new AudioMixerSnapshot[] { NormalSnapshot, LowHpSnapshot };
            snapshotWeights = new float[] { 1, 0 };
        }

        private void Update()
        {
            var healthRatio = Mathf.Clamp01(GameContext.Instance.CurrentHealth / GameContext.Instance.MaxHealth);
            var inverseHealthRatio = 1 - healthRatio;

            snapshotWeights[0] = Mathf.Clamp01(1 - Curve.Evaluate(inverseHealthRatio));
            snapshotWeights[1] = Mathf.Clamp01(Curve.Evaluate(inverseHealthRatio));

            Mixer.TransitionToSnapshots(snapshots, snapshotWeights, 0.1f);
        }
    }
}