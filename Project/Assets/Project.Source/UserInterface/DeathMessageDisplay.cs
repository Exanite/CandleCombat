using UniDi;
using UnityEngine;

namespace Project.Source.UserInterface
{
    public class DeathMessageDisplay : MonoBehaviour
    {
        public GameObject Target;

        [Inject]
        private GameContext gameContext;

        private void Update()
        {
            var isDead = gameContext.IsDead;

            Target.SetActive(isDead);
        }
    }
}