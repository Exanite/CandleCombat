using System.Collections;
using UnityEngine;

namespace Project.Source.Characters
{
    public class CandleCharacterController : MonoBehaviour
    {
        [Header("Dependencies")]
        public Character Character;
        public SkinnedMeshRenderer MeshRenderer;
        public Animator Animator;

        [Header("Attacks")]
        public float AttackRange = 1;

        [Header("Jump")]
        public float JumpDistance = 5;
        public string JumpAnimationTrigger = "Jump";

        public float JumpLeftGroundTime = 0.349f;
        public float JumpLandedTime = 0.983f;

        public float JumpSpreadDistance = 1f;
        public float JumpLookAheadTime = 1f;

        [Header("Death")]
        public string IsDeadAnimationBool = "IsDead";

        [Header("Health Melting")]
        public string HealthRatioAnimationFloat = "HealthRatio";
        public float HealthRatioSmoothTime = 0.1f;

        private bool isJumping;
        private Coroutine jumpCoroutine;

        private Character target;

        private float smoothedHealthRatio;
        private float smoothedHealthRatioVelocity;

        private void Start()
        {
            smoothedHealthRatio = Mathf.Clamp01(Character.CurrentHealth / Character.MaxHealth);
        }

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
            UpdateAnimator();

            if (Character.IsDead)
            {
                return;
            }

            if (!Character.IsPlayer)
            {
                target = GameContext.Instance.CurrentPlayer;

                if (target && !isJumping)
                {
                    var offset = target.transform.position - transform.position;
                    if (offset.magnitude > AttackRange + 0.5f)
                    {
                        jumpCoroutine = StartCoroutine(JumpTowardsTarget());
                    }
                }
            }
        }

        private void UpdateAnimator()
        {
            Animator.SetBool(IsDeadAnimationBool, Character.IsDead);

            var targetHealthRatio = Mathf.Clamp01(Character.CurrentHealth / Character.MaxHealth);
            smoothedHealthRatio = Mathf.SmoothDamp(smoothedHealthRatio, targetHealthRatio, ref smoothedHealthRatioVelocity, HealthRatioSmoothTime);

            Animator.SetFloat(HealthRatioAnimationFloat, smoothedHealthRatio);
        }

        private void OnDead(Character character)
        {
            if (jumpCoroutine != null)
            {
                StopCoroutine(jumpCoroutine);
            }
        }

        private IEnumerator JumpTowardsTarget()
        {
            isJumping = true;
            Animator.SetTrigger(JumpAnimationTrigger);

            yield return new WaitForSeconds(JumpLeftGroundTime);

            var timer = 0f;
            var startPosition = transform.position;
            var targetPosition = SelectJumpPosition();

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

        private Vector3 SelectJumpPosition()
        {
            var targetPosition = target.transform.position + target.Rigidbody.velocity * JumpLookAheadTime;
            var offset = targetPosition - transform.position;
            
            var targetJumpPosition = transform.position + Vector3.ClampMagnitude(offset, Mathf.Min(JumpDistance, offset.magnitude - AttackRange));

            var angleRadians = Random.Range(0, 2 * Mathf.PI);
            var distance = Random.Range(0, JumpSpreadDistance);
            targetJumpPosition += new Vector3(distance * Mathf.Cos(angleRadians), 0, distance * Mathf.Sin(angleRadians));

            return targetJumpPosition;
        }
    }
}