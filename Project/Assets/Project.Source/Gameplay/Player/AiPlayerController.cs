using Project.Source.Gameplay.Characters;
using Project.Source.Input;
using UniDi;
using UnityEngine;

namespace Project.Source.Gameplay.Player
{
    public class AiPlayerController : PlayerController
    {
        private float randomValue;

        private Vector3 smoothedTargetPosition;
        private Vector3 smoothedTargetPositionVelocity;

        [Inject]
        private GameContext gameContext;

        [Inject]
        private PhysicsScene physicsScene;

        private void Start()
        {
            randomValue = Random.Range(0f, 1f);
        }

        protected override void GatherInput(PlayerInputData input)
        {
            var player = gameContext.CurrentPlayer;

            Character closestEnemy = null;
            var closestDistance = float.PositiveInfinity;

            foreach (var character in gameContext.AllCharacters)
            {
                if (character && !character.IsPlayer && !character.IsDead)
                {
                    var offsetToCharacter = character.transform.position - player.transform.position;
                    var canSeeCharacter = physicsScene.Raycast(player.transform.position + Vector3.up, offsetToCharacter.normalized, 
                            out var hit, offsetToCharacter.magnitude, queryTriggerInteraction: QueryTriggerInteraction.Ignore)
                        && hit.collider.TryGetComponent(out Character hitCharacter)
                        && hitCharacter == character;

                    if (!canSeeCharacter)
                    {
                        continue;
                    }

                    var distance = Vector3.Distance(player.transform.position, character.transform.position);
                    if (distance < closestDistance)
                    {
                        closestEnemy = character;
                        closestDistance = distance;
                    }
                }
            }

            input.IsShootPressed = closestEnemy != null;

            if (closestEnemy != null)
            {
                var offsetToEnemy = closestEnemy.transform.position - player.transform.position;
                var directionToEnemy = offsetToEnemy.normalized;

                var targetPosition = closestEnemy.transform.position;
                smoothedTargetPosition = Vector3.SmoothDamp(smoothedTargetPosition, targetPosition, ref smoothedTargetPositionVelocity, 0.1f);

                input.TargetPosition = smoothedTargetPosition;
                input.MovementDirection = new Vector2(-directionToEnemy.x, -directionToEnemy.z);

                input.IsSoulTransferPressed = offsetToEnemy.magnitude < 2;
            }

            input.MovementDirection += new Vector2(
                Mathf.Cos((Mathf.PerlinNoise(randomValue, Time.time) + 1) * Mathf.PI * 2),
                Mathf.Sin((Mathf.PerlinNoise(randomValue, Time.time) + 1) * Mathf.PI * 2));
        }
    }
}