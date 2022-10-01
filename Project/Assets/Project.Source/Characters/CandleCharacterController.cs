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

        [Header("Jump Animation")]
        public float JumpDistance = 5;
        public string JumpAnimationTrigger = "Jump";

        public float JumpLeftGroundTime = 0.349f;
        public float JumpLandedTime = 0.983f;

        public float JumpSpreadActivationDistance = 2f;
        public float JumpSpreadDistance = 1f;

        [Header("Death Animation")]
        public string IsDeadAnimationBool = "IsDead";
        
        private bool isJumping;
        private Coroutine jumpCoroutine;

        private Character target;

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
            Animator.SetBool(IsDeadAnimationBool, Character.IsDead);
            
            if (Character.IsDead)
            {
                return;
            }

            if (!Character.IsPlayer)
            {
                target = GameContext.Instance.CurrentPlayer;
                
                if (target && !isJumping)
                {
                    jumpCoroutine = StartCoroutine(JumpTowardsTarget());
                }
            }
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
            return transform.position + Vector3.ClampMagnitude(target.transform.position - transform.position, JumpDistance);
        }
    }
}