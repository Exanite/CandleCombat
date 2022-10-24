using UnityEngine;

namespace Project.Source.UserInterface
{
    public class DeathMessageDisplay : GameUiBehaviour
    {
        public GameObject Target;

        private void Update()
        {
            var isDead = GameContext.IsDead;

            Target.SetActive(isDead);
        }
    }
}