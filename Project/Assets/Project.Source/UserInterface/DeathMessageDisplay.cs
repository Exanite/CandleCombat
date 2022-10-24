using UnityEngine;

namespace Project.Source.UserInterface
{
    public class DeathMessageDisplay : GameUiBehaviour
    {
        public GameObject Target;

        private void Update()
        {
            if (GameContext == null)
            {
                return;
            }
            
            var isDead = GameContext.IsDead;

            Target.SetActive(isDead);
        }
    }
}