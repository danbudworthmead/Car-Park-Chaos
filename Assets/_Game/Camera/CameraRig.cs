using Unity.Netcode;
using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private Transform camParent;
        [SerializeField] private Transform secondCamParent;
        private Transform _target;

        private void FixedUpdate()
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
                var cam = UnityEngine.Camera.main;
                if (cam == null)
                {
                    return;
                }
                
                var rb = _target.GetComponent<Rigidbody>();
                var speed = rb.velocity.magnitude;
                var ratio = Mathf.InverseLerp(0, 10, speed);
                ratio = Mathf.Clamp01(ratio);
                
                cam.transform.position = Vector3.Lerp(camParent.position, secondCamParent.position, ratio);
                cam.transform.rotation = Quaternion.Lerp(camParent.rotation, secondCamParent.rotation, ratio);
                
                // follow the target but keep upright
                transform.position = _target.position;
                var newRot = Quaternion.Euler(0, _target.eulerAngles.y, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, newRot, Time.deltaTime * 2.5f);
            }
        }
    }
}
