using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
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
        public float JumpLookAheadMaxDistance = 5f;

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

        private Vector3 smoothedVelocityVelocity;

        private NavMeshPath path;
        private readonly Vector3[] pathBuffer = new Vector3[8];

        private void Awake()
        {
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
                Character.Rigidbody.velocity = Vector3.SmoothDamp(Character.Rigidbody.velocity, Vector3.zero, ref smoothedVelocityVelocity, 0.5f);
                
                target = GameContext.Instance.CurrentPlayer;

                var currentPosition = transform.position;
                var targetPosition = target.transform.position;

                if (target && !isJumping)
                {
                    var offset = targetPosition - currentPosition;
                    var distanceToTarget = offset.magnitude;
                    if (distanceToTarget > AttackRange + 0.5f)
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
                        transform.rotation = quaternion.LookRotation(directionToTarget, transform.up);
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

        // private void OnRenderObject()
        // {
        //     var resultCount = path.GetCornersNonAlloc(pathBuffer);
        //     if (resultCount > 1)
        //     {
        //         using (var handle = GameContext.Instance.DrawingService.BeginDrawing())
        //         {
        //             handle.Topology = MeshTopology.Lines;
        //             handle.Color = Color.cyan;
        //
        //             for (var i = 1; i < resultCount; i++)
        //             {
        //                 handle.AddVertex(pathBuffer[i - 1]);
        //                 handle.AddVertex(pathBuffer[i]);
        //             }
        //
        //             handle.DrawSphere(pathBuffer[1], Quaternion.identity, Vector3.one);
        //         }
        //     }
        // }

        private Vector3 SelectJumpPosition()
        {
            var targetPosition = target.transform.position + target.Rigidbody.velocity * JumpLookAheadTime;

            var filter = new NavMeshQueryFilter
            {
                areaMask = ~0,
            };

            if (NavMesh.CalculatePath(transform.position, targetPosition, filter, path))
            {
                var resultCount = path.GetCornersNonAlloc(pathBuffer);
                if (resultCount > 1)
                {
                    void SetTargetPosition(Vector3 cornerPosition)
                    {
                        var directionFromSelf = (cornerPosition - transform.position).normalized;
                        targetPosition = cornerPosition + directionFromSelf * 2f;
                    }
                    
                    SetTargetPosition(pathBuffer[1]);
                    for (var i = 2; i < resultCount; i++)
                    {
                        if (!Physics.Raycast(transform.position, pathBuffer[i]))
                        {
                            SetTargetPosition(pathBuffer[i]);
                        }
                    }
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