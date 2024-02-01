using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        private Transform _target;

        private void Update()
        {
            if (!_target) return;
            
            // follow the target but keep upright
            transform.position = _target.position;
            var newRot = Quaternion.Euler(0, _target.eulerAngles.y, 0);
            transform.rotation = newRot;
        }
        
        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
