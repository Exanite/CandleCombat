using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float movementSpeed;
        [SerializeField] private float movementSmoothTime = 0.05f;
        [SerializeField] private float dodgeSpeed = 50f;
        [SerializeField] private float dodgeTime = 0.2f;
        [SerializeField] private float timeBetweenDodge = 2f;

        public int DodgeAbilityIndex = 2;

        private Character character;
        private Vector2 moveDirection;

        private bool isDodging;
        private float elapsedDodgeTime;
        private float elapsedTimeSinceDodge;
        private float startingMoveSpeed;

        // Not a typo
        private Vector3 smoothedVelocityVelocity;

        [Inject]
        private GameContext gameContext;

        public float MovementSpeed
        {
            get => movementSpeed;
            set => movementSpeed = value;
        }

        private void Update()
        {
            var ability = gameContext.Abilities[DodgeAbilityIndex];
            ability.CooldownDuration = timeBetweenDodge;
            ability.CurrentCooldown = timeBetweenDodge - elapsedTimeSinceDodge;
            ability.HealthCost = 0;
        }

        private void FixedUpdate()
        {
            if (isDodging)
            {
                if (elapsedDodgeTime > dodgeTime)
                {
                    StopDodging();
                }
                else
                {
                    elapsedDodgeTime += Time.deltaTime;
                }
            }
            else
            {
                elapsedTimeSinceDodge += Time.deltaTime;
            }

            if (!character || character.IsDead || !character.Rigidbody)
            {
                return;
            }

            var rb = character.Rigidbody;

            var targetVelocity = new Vector3(moveDirection.x, 0, moveDirection.y);
            targetVelocity *= MovementSpeed;

            rb.velocity = Vector3.SmoothDamp(rb.velocity, targetVelocity, ref smoothedVelocityVelocity, movementSmoothTime);
        }

        public void SetCharacter(Character character)
        {
            this.character = character;
        }

        public void SetMovementDirection(Vector2 direction)
        {
            moveDirection = direction;
        }

        public void Dodge()
        {
            if (elapsedTimeSinceDodge < timeBetweenDodge)
            {
                return;
            }

            startingMoveSpeed = MovementSpeed;
            MovementSpeed = dodgeSpeed;
            isDodging = true;
            character.IsInvulnerable = true;
            elapsedDodgeTime = 0;
            elapsedTimeSinceDodge = 0;
        }

        private void StopDodging()
        {
            isDodging = false;
            character.IsInvulnerable = false;
            MovementSpeed = startingMoveSpeed;
        }
    }
}