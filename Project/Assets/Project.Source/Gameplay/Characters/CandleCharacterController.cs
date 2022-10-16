using System.Collections;
using Project.Source.Gameplay.Player;
using UniDi;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Source.Gameplay.Characters
{
    public class CandleCharacterController : MonoBehaviour
    {
        [Header("Dependencies")]
        public Character Character;
        public SkinnedMeshRenderer MeshRenderer;
        public Animator Animator;
        public GunController GunController;

        [Header("Sounds")]
        [SerializeField] private AudioClip jumpLandingClip;
        [Range(0, 1)]
        [SerializeField] private float jumpLandingAudioScale;
        private AudioSource audioSource;

        [Header("Attacks")]
        public float AttackRange = 1;

        [Header("Jump")]
        public float JumpDistance = 5;
        public string JumpAnimationTrigger = "Jump";

        public float JumpLeftGroundTime = 0.349f;
        public float JumpLandedTime = 0.983f;

        public float JumpSpreadDistance = 1f;
        public float JumpLookAheadTime = 1f;
        public float JumpLookAheadMaxDistance = 5f;

        public float JumpSpeed = 1;
        public string JumpSpeedAnimatorFloat = "JumpSpeed";

        public float CharacterRadius = 0.25f;
        public float JumpCollisionForceMultiplier = 5f;

        [Header("Death")]
        public string IsDeadAnimationBool = "IsDead";

        [Header("Collision Damage")]
        public float CollisionDamageAmount = 10f;
        public float CollisionForce = 5f;
        public float CollisionForceDuration = 5f;

        [Header("Health Melting")]
        public string HealthRatioAnimationFloat = "HealthRatio";
        public float HealthRatioSmoothTime = 0.1f;

        private bool isJumping;
        private Coroutine jumpCoroutine;

        private Character target;

        private float smoothedHealthRatio;
        private float smoothedHealthRatioVelocity;

        private Vector3 smoothedVelocityVelocity;

        private NavMeshPath path;
        private readonly Vector3[] pathBuffer = new Vector3[8];

        [Inject]
        private GameContext gameContext;

        [Inject]
        private PhysicsScene physicsScene;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            path = new NavMeshPath();
        }

        private void Start()
        {
            smoothedHealthRatio = Mathf.Clamp01(Character.CurrentHealth / Character.MaxHealth);
            GunController.SetCharacter(Character);
        }

        private void OnEnable()
        {
            Character.Dead += OnDead;
            gameContext.Possessed += OnPossessed;
        }

        private void OnDisable()
        {
            Character.Dead -= OnDead;
            gameContext.Possessed -= OnPossessed;
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
                Character.Rigidbody.velocity = Vector3.SmoothDamp(Character.Rigidbody.velocity, Vector3.zero, ref smoothedVelocityVelocity, 0.5f);

                target = gameContext.CurrentPlayer;

                if (target && !isJumping && !Animator.GetCurrentAnimatorStateInfo(0).IsName("Jumping"))
                {
                    var currentPosition = transform.position;
                    var targetPosition = target.transform.position;

                    var offset = targetPosition - currentPosition;
                    var distanceToTarget = offset.magnitude;

                    var canSeePlayer = physicsScene.Raycast(transform.position + Vector3.up, offset.normalized, out var hit, offset.magnitude)
                        && hit.collider.TryGetComponent(out Character character)
                        && character.IsPlayer;

                    if (distanceToTarget > AttackRange + 0.5f || !canSeePlayer)
                    {
                        jumpCoroutine = StartCoroutine(JumpTowardsTarget());
                    }
                    else
                    {
                        //Firing logic
                        if (!GunController)
                        {
                            return;
                        }

                        var directionToTarget = offset.normalized;
                        transform.rotation = Quaternion.LookRotation(directionToTarget, transform.up);
                        GunController.Fire();
                    }
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Character.IsPlayer)
            {
                return;
            }

            if (collision.collider.TryGetComponent(out Character otherCharacter))
            {
                if (otherCharacter.IsPlayer)
                {
                    var direction = (otherCharacter.transform.position - transform.position).normalized;

                    otherCharacter.TakeDamage(CollisionDamageAmount);
                    StartCoroutine(ApplyKnockback(direction, otherCharacter, CollisionForceDuration));
                }
            }
        }

        private IEnumerator ApplyKnockback(Vector3 direction, Character character, float duration)
        {
            var timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;

                if (!character)
                {
                    yield break;
                }

                character.Rigidbody.AddForce(direction * CollisionForce, ForceMode.Acceleration);

                yield return null;
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
            if (Character != character)
            {
                return;
            }

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
            Animator.SetFloat(JumpSpeedAnimatorFloat, JumpSpeed);

            yield return new WaitForSeconds(JumpLeftGroundTime / JumpSpeed);

            if (!target)
            {
                isJumping = false;

                yield break;
            }

            var timer = 0f;
            var airTime = JumpLandedTime - JumpLeftGroundTime;

            var startPosition = transform.position;
            var targetPosition = SelectJumpPosition();

            var jumpOffset = targetPosition - startPosition;
            var jumpDistance = jumpOffset.magnitude;
            var jumpDirection = jumpOffset.normalized;

            while (timer < airTime)
            {
                timer += Time.deltaTime * JumpSpeed;

                var distance = Time.deltaTime * jumpDistance * JumpSpeed;
                if (physicsScene.SphereCast(transform.position + Vector3.up, CharacterRadius, jumpDirection, out var hit,
                    distance + CharacterRadius))
                {
                    distance = Mathf.Clamp(hit.distance, 0, distance);

                    if (hit.collider.attachedRigidbody)
                    {
                        var force = Character.Rigidbody.mass * JumpSpeed * JumpCollisionForceMultiplier * jumpDirection;
                        hit.collider.attachedRigidbody.AddForceAtPosition(force, hit.point);
                    }
                }

                transform.position += distance * jumpDirection;

                yield return null;
            }

            if (jumpLandingClip != null)
            {
                audioSource.PlayOneShot(jumpLandingClip, jumpLandingAudioScale);
            }

            isJumping = false;
        }

        private Vector3 SelectJumpPosition()
        {
            var targetPosition = target.transform.position + target.Rigidbody.velocity * JumpLookAheadTime;

            var filter = new NavMeshQueryFilter
            {
                areaMask = NavMesh.AllAreas,
            };

            if (NavMesh.CalculatePath(transform.position, targetPosition, filter, path))
            {
                var resultCount = path.GetCornersNonAlloc(pathBuffer);
                if (resultCount > 1)
                {
                    var directionFromSelf = (pathBuffer[1] - transform.position).normalized;
                    targetPosition = pathBuffer[1] + directionFromSelf * 2f;
                }
            }

            var offset = targetPosition - transform.position;

            var maxJumpDistance = Mathf.Min(JumpDistance, JumpLookAheadMaxDistance);
            var targetJumpPosition = transform.position + Vector3.ClampMagnitude(offset, maxJumpDistance);

            var angleRadians = Random.Range(0, 2 * Mathf.PI);
            var distance = Random.Range(0, JumpSpreadDistance);
            targetJumpPosition += new Vector3(distance * Mathf.Cos(angleRadians), 0, distance * Mathf.Sin(angleRadians));

            return targetJumpPosition;
        }
    }
}