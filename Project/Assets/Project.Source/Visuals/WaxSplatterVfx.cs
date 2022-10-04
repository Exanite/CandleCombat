using UnityEngine;

namespace Project.Source.Visuals
{
    public class WaxSplatterVfx : MonoBehaviour
    {
        public float DestroyDelay = 2f;

        private void Start()
        {
            Destroy(gameObject, DestroyDelay);
        }
    }
}