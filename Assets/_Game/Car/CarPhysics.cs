using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class CarPhysics : NetworkBehaviour
    {
        [SerializeField] private Headlights rearHeadlights;
    
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [SerializeField] private Transform frontLeftWheel;
        [SerializeField] private Transform frontRightWheel;
        [SerializeField] private Transform rearLeftWheel;
        [SerializeField] private Transform rearRightWheel;

        [SerializeField] private float acceleration = 500f;
        [SerializeField] private float breakingForce = 300f;
        [SerializeField] private float maxTurnAngle = 15f;

        [SerializeField] private new Rigidbody rigidbody;

        private float _currentAcceleration = 0f;
        private float _currentBrakeForce = 0f;
        private float _currentTurnAngle = 0f;

        public float Speed { get; private set; }

        public void SetAcceleration(float accelerator)
        {
            _currentAcceleration = accelerator * acceleration;
        }

        public void SetBrake(float brake)
        {
            _currentBrakeForce = brake * breakingForce;
            rearHeadlights.SetOn(brake > 0);
        }

        public void SetSteering(float steering)
        {
            _currentTurnAngle = steering * maxTurnAngle;
        }

        private void FixedUpdate()
        {
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
        }

        private void Update()
        {
            UpdateWheel(frontLeft, frontLeftWheel);
            UpdateWheel(frontRight, frontRightWheel);
            UpdateWheel(rearLeft, rearLeftWheel);
            UpdateWheel(rearRight, rearRightWheel);
        }

        private static void UpdateWheel(WheelCollider col, Transform t)
        {
            col.GetWorldPose(out var pos, out var rot);
            t.position = pos;
            t.rotation = rot;
        }
        
        [ClientRpc]
        public void SetPositionClientRpc(Vector3 position)
        {
            transform.position = position;
        }

        [ClientRpc]
        public void SetRotationClientRpc(Quaternion rotation)
        {
            transform.rotation = rotation;
        }
    }
}
