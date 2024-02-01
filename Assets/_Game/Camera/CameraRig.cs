using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private float cameraMoveSpeed = 4f;
        private Transform _target;

        private void LateUpdate()
        {
            if (!_target)
            {
                _target = FindObjectOfType<CarController>()?.transform;
                return;
            }
            
            // follow the target but keep upright
            transform.position = _target.position;
            var newRot = Quaternion.Euler(0, _target.eulerAngles.y, 0);
            // smoothing is a bit choppy
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * cameraMoveSpeed);
        }
    }
}
