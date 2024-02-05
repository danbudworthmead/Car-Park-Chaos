using System;
using UnityEngine;

namespace _Game.Car
{
    public class Wheel : MonoBehaviour
    {
        [SerializeField] private WheelCollider _wheel;

        private void LateUpdate()
        {
            _wheel.GetWorldPose(out var pos, out var quat);
            transform.position = pos;
            transform.rotation = quat;
        }
    }
}
