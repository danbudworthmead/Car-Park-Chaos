using UnityEngine;

namespace _Game.Car
{
    public class Wheel : MonoBehaviour
    {
        [SerializeField] private WheelCollider target;
        
        public void SetCollider(WheelCollider wheelCollider)
        {
            target = wheelCollider;
        }
        
        private void Update()
        {
            target.GetWorldPose(out var position, out var rotation);
            // transform.position = position;
            transform.rotation = rotation;
        }
    }
}
