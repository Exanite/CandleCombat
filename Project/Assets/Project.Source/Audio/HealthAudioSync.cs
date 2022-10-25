using Project.Source.UserInterface;
using UnityEngine;
using UnityEngine.Audio;

namespace Project.Source.Audio
{
    public class HealthAudioSync : GameUiBehaviour
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
            var healthRatio = GameContext ? Mathf.Clamp01(GameContext.CurrentHealth / GameContext.MaxHealth) : 1;

            SetHealthRatio(healthRatio);
        }

        private void SetHealthRatio(float healthRatio)
        {
            var inverseHealthRatio = 1 - healthRatio;

            snapshotWeights[0] = Mathf.Clamp01(1 - Curve.Evaluate(inverseHealthRatio));
            snapshotWeights[1] = Mathf.Clamp01(Curve.Evaluate(inverseHealthRatio));

            Mixer.TransitionToSnapshots(snapshots, snapshotWeights, 0.1f);
        }
    }
}