using System.Collections;
using UnityEngine;

namespace Project.Source
{
    public class EnemyCharacter : MonoBehaviour
    {
        [Header("Dependencies")]
        public Character Character;
        public SkinnedMeshRenderer MeshRenderer;

        [Header("Jump Animation")]
        public float JumpHeight = 2;
        public float JumpDistance = 5;
        public float JumpDelay = 0.5f;
        public float JumpTime = 1;
        public AnimationCurve JumpAnimationCurve;

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
            
            var timer = 0f;
            var startPosition = transform.position;
            var targetPosition = transform.position + Vector3.ClampMagnitude(Target.transform.position - transform.position, JumpDistance);
            
            while (timer < JumpTime)
            {
                timer += Time.deltaTime;

                var ratio = timer / JumpTime;
                var height = JumpAnimationCurve.Evaluate(ratio) * JumpHeight;
                var position = Vector3.Lerp(startPosition, targetPosition, ratio);

                transform.position = position + Vector3.up * height;

                yield return null;
            }

            transform.position = new Vector3(transform.position.x, 0, transform.position.z);

            yield return new WaitForSeconds(JumpDelay);

            isJumping = false;
        }
    }
}