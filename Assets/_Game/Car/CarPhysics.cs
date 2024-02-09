using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class CarPhysics : NetworkBehaviour
    {
        [SerializeField] private float acceleration = 500f;
        [SerializeField] private float breakingForce = 300f;
        [SerializeField] private float maxTurnAngle = 15f;

        [SerializeField] public new Rigidbody rigidbody;
        
        [SerializeField] public Car car;

        private float _currentAcceleration = 0f;
        private float _currentBrakeForce = 0f;
        private float _currentTurnAngle = 0f;
        
        public float Speed { get; private set; }

        public NetworkVariable<float> EngineRPM = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        public void SetAcceleration(float accelerator)
        {
            _currentAcceleration = accelerator * acceleration;
        }

        public void SetBrake(float brake)
        {
            _currentBrakeForce = brake * breakingForce;

            if (car && car.rearHeadlights)
            {
                car.rearHeadlights.SetOn(brake > 0);   
            }
        }

        public void SetSteering(float steering)
        {
            _currentTurnAngle = steering * maxTurnAngle;
        }

        private void FixedUpdate()
        { 
            if (!car)
            {
                car = GetComponentInChildren<Car>();
                return;
            }
            
            var accel = _currentAcceleration - _currentBrakeForce;
        
            // apply acceleration to the front wheels
            car.wheels.frontLeft.motorTorque = accel;
            car.wheels.frontRight.motorTorque = accel;
        
            // apply braking to all wheels
            var brakeForce = accel > 0 ? _currentBrakeForce : _currentAcceleration;
        
            car.wheels.frontLeft.brakeTorque = brakeForce;
            car.wheels.frontRight.brakeTorque = brakeForce;
            car.wheels.rearLeft.brakeTorque = brakeForce;
            car.wheels.rearRight.brakeTorque = brakeForce;
        
            // handle turning
            car.wheels.frontLeft.steerAngle = _currentTurnAngle;
            car.wheels.frontRight.steerAngle = _currentTurnAngle;
        
            Speed = rigidbody.velocity.magnitude;
            
            if (IsOwner) EngineRPM.Value = Speed;
            
        }

        private void Update()
        {
            if (car == null) return;
            
            // UpdateWheel(frontLeft, _car.frontLeftWheel);
            // UpdateWheel(frontRight, _car.frontRightWheel);
            // UpdateWheel(rearLeft, _car.rearLeftWheel);
            // UpdateWheel(rearRight, _car.rearRightWheel);
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

        [ClientRpc]
        public void UnfreezeClientRpc()
        {
            rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}
