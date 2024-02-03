using Unity.Netcode;
using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private Transform camParent;
        private Transform _target;

        private void LateUpdate()
        {
            if (_target == null)
            {
                var cam = UnityEngine.Camera.main;
                
                var playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
                if (playerObject == null 
                    || cam == null 
                    || camParent == null)
                {
                    return;
                }
                    
                var camTransform = cam.transform;
                camTransform.SetParent(camParent);
                camTransform.position = camParent.position;
                camTransform.rotation = camParent.rotation;
                _target = playerObject.transform;
            }
            else
            {
                // follow the target but keep upright
                transform.position = _target.position;
                var newRot = Quaternion.Euler(0, _target.eulerAngles.y, 0);
                transform.rotation = newRot;
            }
        }
    }
}
