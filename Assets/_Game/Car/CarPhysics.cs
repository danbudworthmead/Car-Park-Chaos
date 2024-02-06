using System;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class CarPhysics : NetworkBehaviour
    {
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [SerializeField] private float acceleration = 500f;
        [SerializeField] private float breakingForce = 300f;
        [SerializeField] private float maxTurnAngle = 15f;

        [SerializeField] private new Rigidbody rigidbody;
        
        private Cars.Car _car;

        private float _currentAcceleration = 0f;
        private float _currentBrakeForce = 0f;
        private float _currentTurnAngle = 0f;
        
        public float Speed { get; private set; }
        
        public float SpeedDotProduct { get; private set; }

        public NetworkVariable<float> EngineRPM = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        public void SetAcceleration(float accelerator)
        {
            _currentAcceleration = accelerator * acceleration;
        }

        public void SetBrake(float brake)
        {
            _currentBrakeForce = brake * breakingForce;

            if (_car && _car.rearHeadlights)
            {
                _car.rearHeadlights.SetOn(brake > 0);   
            }
        }

        public void SetSteering(float steering)
        {
            _currentTurnAngle = steering * maxTurnAngle;
        }

        private void FixedUpdate()
        { 
            if (!_car)
            {
                _car = GetComponentInChildren<Cars.Car>();
                return;
            }
            
            var accel = _currentAcceleration - _currentBrakeForce;
        
            // apply acceleration to the front wheels
            frontLeft.motorTorque = accel;
            frontRight.motorTorque = accel;
        
            // apply braking to all wheels
            var brakeForce = accel > 0 ? _currentBrakeForce : _currentAcceleration;
        
            frontLeft.brakeTorque = brakeForce;
            frontRight.brakeTorque = brakeForce;
            rearLeft.brakeTorque = brakeForce;
            rearRight.brakeTorque = brakeForce;
        
            // handle turning
            frontLeft.steerAngle = _currentTurnAngle;
            frontRight.steerAngle = _currentTurnAngle;
        
            Speed = rigidbody.velocity.magnitude;

            if (IsOwner) EngineRPM.Value = Speed;
            
            SpeedDotProduct = Math.Abs(Vector3.Dot(rigidbody.transform.right, rigidbody.velocity))/5f;
        }

        private void Update()
        {
            if (_car == null) return;
            
            UpdateWheel(frontLeft, _car.frontLeftWheel);
            UpdateWheel(frontRight, _car.frontRightWheel);
            UpdateWheel(rearLeft, _car.rearLeftWheel);
            UpdateWheel(rearRight, _car.rearRightWheel);
        }

        private static void UpdateWheel(WheelCollider col, Transform t)
        {
            col.GetWorldPose(out var pos, out var rot);
            t.position = pos;
            t.rotation = Quaternion.Lerp(t.rotation, rot, Time.deltaTime * 11f);
        }
        
        [ClientRpc]
        public void SetPositionRotationClientRpc(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}
