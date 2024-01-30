using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] Transform target;

        private void LateUpdate()
        {
            // follow the target but keep upright
            transform.position = target.position;
            transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
        }
    }
}
