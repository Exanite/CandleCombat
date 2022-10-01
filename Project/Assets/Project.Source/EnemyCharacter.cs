using System.Collections;
using UnityEngine;

namespace Project.Source
{
    public class EnemyCharacter : MonoBehaviour
    {
        [Header("Dependencies")]
        public Character Character;
        public SkinnedMeshRenderer MeshRenderer;
        public Animator Animator;

        [Header("Jump Animation")]
        public float JumpDistance = 5;
        public string JumpTrigger = "Jump";

        public float JumpLeftGroundTime = 0.349f;
        public float JumpLandedTime = 0.983f;

        [Header("Death Animation")]
        public int DeathBlendShapeIndex;
        public float DeathAnimationTime = 1;
        public AnimationCurve DeathAnimationCurve;

        [Header("Runtime")]
        public Transform Target;

        private bool isJumping;

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

            if (Target && !isJumping)
            {
                StartCoroutine(JumpTowardsTarget());
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

        private IEnumerator JumpTowardsTarget()
        {
            isJumping = true;
            Animator.SetTrigger(JumpTrigger);

            yield return new WaitForSeconds(JumpLeftGroundTime);

            var timer = 0f;
            var startPosition = transform.position;
            var targetPosition = transform.position + Vector3.ClampMagnitude(Target.transform.position - transform.position, JumpDistance);

            var airTime = JumpLandedTime - JumpLeftGroundTime;

            while (timer < airTime)
            {
                timer += Time.deltaTime;

                var ratio = timer / airTime;
                var position = Vector3.Lerp(startPosition, targetPosition, ratio);

                transform.position = new Vector3(position.x, transform.position.y, position.z);

                yield return null;
            }

            transform.position = targetPosition;
            isJumping = false;
        }
    }
}