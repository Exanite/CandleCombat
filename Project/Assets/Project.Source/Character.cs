using UnityEngine;

namespace Project.Source
{
    public class Character : MonoBehaviour
    {
        public float CurrentHealth = 100;
        public float MaxHealth = 100;

        public Rigidbody Rigidbody;

        private void Start()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }
    }
}