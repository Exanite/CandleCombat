using UnityEngine;

namespace Project.Source.UserInterface
{
    public class DeathMessageDisplay : MonoBehaviour
    {
        public GameObject Target;

        private void Update()
        {
            var isDead = GameContext.Instance.IsDead;

            Target.SetActive(isDead);
        }
    }
}