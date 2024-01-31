using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
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
            transform.rotation = newRot;
        }
    }
}
