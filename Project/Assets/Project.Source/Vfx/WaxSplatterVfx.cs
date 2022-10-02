using UnityEngine;

namespace Project.Source.Vfx
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