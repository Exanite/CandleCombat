using UnityEngine;
using UnityEngine.VFX;

namespace Project.Source.Visuals
{
    public class WindVfxVelocitySync : MonoBehaviour
    {
        public VisualEffect WindEffect;
        public Rigidbody Rigidbody;

        private void Update()
        {
            WindEffect.SetVector3("Velocity Vector", Rigidbody.velocity);
        }
    }
}