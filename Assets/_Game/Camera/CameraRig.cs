using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private float cameraMoveSpeed = 4f;
        private Transform _target;

        private void Update()
        {
            if (!_target) return;
            
            // follow the target but keep upright
            transform.position = _target.position;
            var newRot = Quaternion.Euler(0, _target.eulerAngles.y, 0);
            // smoothing is a bit choppy
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * cameraMoveSpeed);
        }
        
        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}
