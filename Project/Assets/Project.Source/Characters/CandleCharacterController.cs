using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Source.Characters
{
    public class CandleCharacterController : MonoBehaviour
    {
        [Header("Dependencies")]
        public Character Character;
        public SkinnedMeshRenderer MeshRenderer;
        public Animator Animator;
        public GunController GunController;

        [Header("Attacks")]
        public float AttackRange = 1;

        [Header("Jump")]
        public float JumpDistance = 5;
        public string JumpAnimationTrigger = "Jump";

        public float JumpLeftGroundTime = 0.349f;
        public float JumpLandedTime = 0.983f;

        public float JumpSpreadDistance = 1f;
        public float JumpLookAheadTime = 1f;

        public float CharacterRadius = 0.25f;

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
            GunController.SetCharacter(Character);
        }

        private void OnEnable()
        {
            Character.Dead += OnDead;
            Character.Possessed += OnPossessed;
        }

        private void OnDisable()
        {
            Character.Dead -= OnDead;
            Character.Possessed -= OnPossessed;
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

                Vector3 currentPosition = transform.position;
                Vector3 targetPosition = target.transform.position;
                
                if (target && !isJumping)
                {
                    var offset = targetPosition - currentPosition;
                    float distanceToTarget = offset.magnitude;
                    if (distanceToTarget > AttackRange + 0.5f)
                    {
                        jumpCoroutine = StartCoroutine(JumpTowardsTarget());
                    }
                    else
                    {
                        //Firing logic
                        
                        if (!GunController) return;

                        Vector3 directionToTarget = offset.normalized;
                        transform.rotation = quaternion.LookRotation(directionToTarget,  transform.up);
                        GunController.Fire();
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

        private void OnPossessed(Character character)
        {
            if (GunController != null)
            {
                GunController.Cleanup();   
                Destroy(GunController);
            }
        }

        private IEnumerator JumpTowardsTarget()
        {
            isJumping = true;
            Animator.SetTrigger(JumpAnimationTrigger);

            yield return new WaitForSeconds(JumpLeftGroundTime);

            var timer = 0f;
            var airTime = JumpLandedTime - JumpLeftGroundTime;

            var startPosition = transform.position;
            var targetPosition = SelectJumpPosition();

            var jumpOffset = targetPosition - startPosition;
            var jumpDistance = jumpOffset.magnitude;
            var jumpDirection = jumpOffset.normalized;

            while (timer < airTime)
            {
                timer += Time.deltaTime;

                var distance = Time.deltaTime * jumpDistance;
                if (Physics.Raycast(transform.position + Vector3.up, jumpDirection, out var hit, distance + CharacterRadius))
                {
                    distance = Mathf.Clamp(hit.distance - CharacterRadius, 0, distance);
                }

                transform.position += distance * jumpDirection;

                yield return null;
            }

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