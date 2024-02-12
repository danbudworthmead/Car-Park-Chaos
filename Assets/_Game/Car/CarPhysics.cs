using System;
using System.Collections.Generic;
using _Game.Netcode;
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

        private VehicleInput _input;

        public float Speed { get; private set; }

        public NetworkVariable<float> EngineRPM = new (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        // netcode general
        private NetworkTimer _networkTimer;
        const float ServerTickRate = 60;
        const int BufferSize = 1024;
        
        // netcode client
        private CircularBuffer<StatePayload> _clientStateBuffer;
        private CircularBuffer<InputPayload> _clientInputBuffer;
        private StatePayload _lastServerState;
        private StatePayload _lastProcessedState;
        
        // netcode server
        private CircularBuffer<StatePayload> _serverStateBuffer;
        private Queue<InputPayload> _serverInputQueue;
        
        [Header("Netcode")]
        [SerializeField] private float reconciliationThreshold = 10f;

        private void Awake()
        {
            _networkTimer = new NetworkTimer(ServerTickRate);
            _clientStateBuffer = new CircularBuffer<StatePayload>(BufferSize);
            _clientInputBuffer = new CircularBuffer<InputPayload>(BufferSize);
            
            _serverStateBuffer = new CircularBuffer<StatePayload>(BufferSize);
            _serverInputQueue = new Queue<InputPayload>();
        }

        private void Update()
        {
            _networkTimer.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        { 
            if (!car)
            {
                car = GetComponentInChildren<Car>();
                return;
            }

            if (!IsOwner)
            {
                return;
            }

            while (_networkTimer.ShouldTick())
            {
                HandleClientTick();
                HandleServerTick();
            }
        }

        private void HandleClientTick() {
            if (!IsClient || !IsOwner) return;

            var currentTick = _networkTimer.CurrentTick;
            var bufferIndex = currentTick % BufferSize;
            
            var inputPayload = new InputPayload() {
                tick = currentTick,
                input = _input,
            };
            
            _clientInputBuffer.Add(inputPayload, bufferIndex);
            SendToServerRpc(inputPayload);
            
            var statePayload = ProcessMovement(inputPayload);
            _clientStateBuffer.Add(statePayload, bufferIndex);
            
            HandleServerReconciliation();
        }

        private void HandleServerTick() {
            if (!IsServer) return;
             
            var bufferIndex = -1;
            while (_serverInputQueue.Count > 0) {
                var inputPayload = _serverInputQueue.Dequeue();
                
                bufferIndex = inputPayload.tick % BufferSize;
                
                var statePayload = ProcessMovement(inputPayload);
                _serverStateBuffer.Add(statePayload, bufferIndex);
            }
            
            if (bufferIndex == -1) return;
            SendToClientRpc(_serverStateBuffer.Get(bufferIndex));
        }

        [ClientRpc]
        private void SendToClientRpc(StatePayload statePayload)
        {
            if (!IsOwner) return;
            _lastServerState = statePayload;
        }

        [ServerRpc]
        private void SendToServerRpc(InputPayload input)
        {
            _serverInputQueue.Enqueue(input);
        }

        private void HandleServerReconciliation()
        {
            if (!ShouldReconcile()) return;

            float positionError;
            int bufferIndex;
            
            bufferIndex = _lastServerState.tick % BufferSize;
            if (bufferIndex - 1 < 0) return; // Not enough information to reconcile
            
            var rewindState = IsHost ? _serverStateBuffer.Get(bufferIndex - 1) : _lastServerState; // Host RPCs execute immediately, so we can use the last server state
            var clientState = IsHost ? _clientStateBuffer.Get(bufferIndex - 1) : _clientStateBuffer.Get(bufferIndex);
            positionError = Vector3.Distance(rewindState.position, clientState.position);
            
            if (positionError > reconciliationThreshold) {
                ReconcileState(rewindState);
            }

            _lastProcessedState = rewindState;
        }

        private void ReconcileState(StatePayload rewindState)
        {
            transform.position = rewindState.position;
            transform.rotation = rewindState.rotation;
            rigidbody.velocity = rewindState.velocity;
            rigidbody.angularVelocity = rewindState.angularVelocity;

            if (!rewindState.Equals(_lastServerState)) return;
            
            _clientStateBuffer.Add(rewindState, rewindState.tick);

            int tickToReplay = _lastServerState.tick;
            while (tickToReplay < _networkTimer.CurrentTick)
            {
                var bufferIndex = tickToReplay % BufferSize;
                var statePayload = ProcessMovement(_clientInputBuffer.Get(bufferIndex));
                _clientStateBuffer.Add(statePayload, bufferIndex);
                tickToReplay++;
            }
        }

        private bool ShouldReconcile()
        {
            var isNewServerState = !_lastServerState.Equals(default);
            var isLastStateUndefinedOrDifferent = _lastProcessedState.Equals(default)
                || !_lastProcessedState.Equals(_lastServerState);
            
            return isNewServerState && isLastStateUndefinedOrDifferent;
        }

        private StatePayload SimulateMovement(InputPayload inputPayload)
        {
            Physics.simulationMode = SimulationMode.Script;

            var pos = transform.position;
            var rot = transform.rotation;
            var vel = rigidbody.velocity;
            var angVel = rigidbody.angularVelocity;
            
            Move(inputPayload.input);
            Physics.Simulate(Time.fixedDeltaTime);
            Physics.simulationMode = SimulationMode.FixedUpdate;
            
            var payload = new StatePayload
            {
                tick = inputPayload.tick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rigidbody.velocity,
                angularVelocity = rigidbody.angularVelocity
            };

            transform.position = pos;
            transform.rotation = rot;
            rigidbody.velocity = vel;
            rigidbody.angularVelocity = angVel;

            return payload;
        }

        private StatePayload ProcessMovement(InputPayload input)
        {
            Move(input.input);

            return new StatePayload()
            {
                tick = _networkTimer.CurrentTick,
                position = transform.position,
                rotation = transform.rotation,
                velocity = rigidbody.velocity,
                angularVelocity = rigidbody.angularVelocity
            };
        }

        private void Move(VehicleInput input)
        {
            if (input.accelerator == 0 
                && input.brake == 0
                && input.steering == 0)
            {
                return;
            }
            
            input.Clamp();
            
            var velocity = rigidbody.velocity;
            var movingForward = velocity.magnitude > 0.1f 
                && Vector3.Dot(transform.forward, velocity) > 0;
            
            if (velocity.magnitude < 0.1f)
            {
                movingForward = input.accelerator > 0;
            }

            if (movingForward)
            {
                // apply acceleration to the front wheels
                car.wheels.frontLeft.motorTorque = input.accelerator * acceleration;
                car.wheels.frontRight.motorTorque = input.accelerator * acceleration;

                // apply braking to all wheels
                var brakeForce = input.brake * breakingForce;

                car.wheels.frontLeft.brakeTorque = brakeForce;
                car.wheels.frontRight.brakeTorque = brakeForce;
                car.wheels.rearLeft.brakeTorque = brakeForce;
                car.wheels.rearRight.brakeTorque = brakeForce;
            }
            else
            {
                // apply reverse acceleration to the front wheels
                car.wheels.frontLeft.motorTorque = input.brake * -acceleration;
                car.wheels.frontRight.motorTorque = input.brake * -acceleration;
                
                // apply braking to all wheels
                var brakeForce = input.accelerator * breakingForce;
                
                car.wheels.frontLeft.brakeTorque = brakeForce;
                car.wheels.frontRight.brakeTorque = brakeForce;
                car.wheels.rearLeft.brakeTorque = brakeForce;
                car.wheels.rearRight.brakeTorque = brakeForce;
            }

            // handle turning
            var steering = input.steering * maxTurnAngle;
            car.wheels.frontLeft.steerAngle = steering;
            car.wheels.frontRight.steerAngle = steering;
        
            Speed = rigidbody.velocity.magnitude;
            
            if (IsOwner) EngineRPM.Value = Speed;

        }

        public void SetAcceleration(float accelerator)
        {
            _input.accelerator = accelerator;
        }

        public void SetBrake(float brake)
        {
            _input.brake = brake;

            if (car && car.rearHeadlights)
            {
                car.rearHeadlights.SetOn(brake > 0);   
            }
        }

        public void SetSteering(float steering)
        {
            _input.steering = steering;
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
