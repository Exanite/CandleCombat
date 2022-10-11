using Cinemachine;
using Project.Source.Gameplay.Characters;
using UniDi;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Source.Gameplay.Player
{
    public class PlayerLook : MonoBehaviour
    {
        public Transform LookAt => targetGroup.transform;

        // Using separate CinemachineTargetGroup.
        [Header("Dependencies")]
        [SerializeField]
        private CinemachineTargetGroup targetGroupPrefab;
        [FormerlySerializedAs("endObjectPrefab")]
        [SerializeField]
        private Transform lookAtTargetPrefab;

        [Header("Settings")]
        [SerializeField]
        private float maxRadius = 5f;
        [Range(0, 1)]
        [SerializeField]
        private float playerWeight = 1f;
        [Range(0, 1)]
        [SerializeField]
        private float pointerWeight = 0.2f;

        private CinemachineTargetGroup targetGroup;
        private Transform lookAtTarget;

        private Character character;

        private Vector3 targetPosition;
        
        [Inject]
        private IInstantiator instantiator;

        private void Awake()
        {
            targetGroup = instantiator.InstantiatePrefabForComponent<CinemachineTargetGroup>(targetGroupPrefab);
            lookAtTarget = instantiator.InstantiatePrefabForComponent<Transform>(lookAtTargetPrefab);
            lookAtTarget.position = transform.position;
            targetGroup.AddMember(lookAtTarget, pointerWeight, 0);
        }

        private void FixedUpdate()
        {
            if (character == null)
            {
                return;
            }

            lookAtTarget.position = character.transform.position + Vector3.ClampMagnitude(targetPosition - character.transform.position, maxRadius);

            if (!character.IsDead)
            {
                character.transform.rotation = Quaternion.LookRotation(GetLookDirection(targetPosition));
            }
        }

        public void SetCharacter(Character newCharacter)
        {
            if (character != null)
            {
                if (targetGroup.FindMember(character.transform) > -1)
                {
                    targetGroup.RemoveMember(character.transform);
                }
            }

            targetGroup.AddMember(newCharacter.transform, playerWeight, 0);
            character = newCharacter;
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }

        private Vector3 GetLookDirection(Vector3 targetPosition)
        {
            var offset = targetPosition - character.transform.position;

            return new Vector3(offset.x, 0, offset.z).normalized;
        }
    }
}