using System;
using UnityEngine;

namespace _Game.Camera
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] Transform target;

        private void Start()
        {
            transform.rotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
        }

        private void LateUpdate()
        {
            // follow the target but keep upright
            transform.position = target.position;
            var newRot = Quaternion.Euler(0, target.eulerAngles.y, 0);
            transform.rotation = newRot;
        }
    }
}
