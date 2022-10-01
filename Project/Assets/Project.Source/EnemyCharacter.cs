using System.Collections;
using UnityEngine;

namespace Project.Source
{
    public class EnemyCharacter : MonoBehaviour
    {
        [Header("Dependencies")]
        public Character Character;
        public SkinnedMeshRenderer MeshRenderer;

        [Header("Configuration")]
        public float MovementSpeed = 5;

        [Header("Death Animation")]
        public int DeathBlendShapeIndex;
        public AnimationCurve DeathAnimationCurve;
        public float DeathAnimationTime = 1;

        [Header("Runtime")]
        public Transform Target;

        private void OnEnable()
        {
            Character.Dead += OnDead;
        }

        private void OnDisable()
        {
            Character.Dead -= OnDead;
        }

        private void Update()
        {
            if (Character.IsDead)
            {
                return;
            }

            var rb = Character.Rigidbody;

            if (Target)
            {
                var direction = Vector3.ClampMagnitude(Target.transform.position - transform.position, 1);

                rb.velocity = direction * MovementSpeed;
            }
        }

        private void OnDead(Character character)
        {
            StartCoroutine(PlayDeathAnimation());
        }

        private IEnumerator PlayDeathAnimation()
        {
            var timer = 0f;

            while (timer < DeathAnimationTime)
            {
                timer += Time.deltaTime;

                var weight = DeathAnimationCurve.Evaluate(timer / DeathAnimationTime) * 100;
                MeshRenderer.SetBlendShapeWeight(DeathBlendShapeIndex, weight);

                yield return null;
            }
        }
    }
}