using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.Car
{
    public class Car : NetworkBehaviour
    {
        // car wheels
        [SerializeField] private Transform frontLeftWheel;
        [SerializeField] private Transform frontRightWheel;
        [SerializeField] private Transform rearLeftWheel;
        [SerializeField] private Transform rearRightWheel;
        
        // car lights
        [SerializeField] private Transform frontLights;
        [SerializeField] private Transform rearLights;
        
        // car plates
        [SerializeField] private Transform frontPlate;
        [SerializeField] private Transform rearPlate;
        
        // car body
        [SerializeField] private Transform body;
        
        // car engine location
        [SerializeField] private Transform engine;
        
        // car horn location
        [SerializeField] private Transform horn;
        
        // car headlights
        [SerializeField] public Headlights rearHeadlights;

        [Serializable]
        public class Wheels
        {
            public WheelCollider frontLeft;
            public WheelCollider frontRight;
            public WheelCollider rearLeft;
            public WheelCollider rearRight;
        }
        
        [SerializeField] public Wheels wheels;

        public void CreatePlayerCarPrefab()
        {
            AssignParts();
            AddRigidbody();
            AddComponents();
            AddColliders();
            AddLights();
        }

        private void AddLights()
        {
            rearHeadlights = rearLights.gameObject.AddComponent<Headlights>();
        }

        public void CreateNpcCarPrefab()
        {
            AssignParts();
            AddColliders(false);
        }
        
        public void AssignParts()
        {
            frontLeftWheel = transform.Find("Front Left Wheel");
            frontRightWheel = transform.Find("Front Right Wheel");
            rearLeftWheel = transform.Find("Rear Left Wheel");
            rearRightWheel = transform.Find("Rear Right Wheel");
            
            frontLights = transform.Find("Front Lights");
            rearLights = transform.Find("Rear Lights");
            
            frontPlate = transform.Find("Front Plate");
            rearPlate = transform.Find("Rear Plate");
            
            body = transform.Find("Body");
            
            engine = transform.Find("Engine");
            horn = transform.Find("Horn");
            
            // log errors
            if (frontLeftWheel == null) Debug.LogError("Front Left Wheel not found");
            if (frontRightWheel == null) Debug.LogError("Front Right Wheel not found");
            if (rearLeftWheel == null) Debug.LogError("Rear Left Wheel not found");
            if (rearRightWheel == null) Debug.LogError("Rear Right Wheel not found");
            
            if (frontLights == null) Debug.LogError("Front Lights not found");
            if (rearLights == null) Debug.LogError("Rear Lights not found");
            
            if (frontPlate == null) Debug.LogError("Front Plate not found");
            if (rearPlate == null) Debug.LogError("Rear Plate not found");
            
            if (body == null) Debug.LogError("Body not found");
            
            if (engine == null) Debug.LogError("Engine not found");
            if (horn == null) Debug.LogError("Horn not found");
        }

        public void AddComponents()
        {
            gameObject.AddComponent<NetworkObject>();
            gameObject.AddComponent<CarController>();
            
            var physics = gameObject.AddComponent<CarPhysics>();
            physics.car = this;
            physics.rigidbody = GetComponent<Rigidbody>();
            
            var carAudio = gameObject.AddComponent<CarAudio>();
            carAudio.carPhysics = physics;
            carAudio.engineAudio = engine.gameObject.AddComponent<AudioSource>();
            carAudio.hornAudio = horn.gameObject.AddComponent<AudioSource>();
        }

        public void AddColliders(bool withWheels = true)
        {
            // add a box collider to the car and calculate the size based on the body
            var collider = gameObject.AddComponent<BoxCollider>();
            var bounds =  body.GetComponent<MeshRenderer>().bounds;
            var size = bounds.size;
            collider.size = new Vector3(size.x, size.y, size.z);
            
            var center = collider.center;
            center.y = size.y - (size.y / 4);
            collider.center = center;

            if (withWheels)
            {
                // add wheel collider to each wheel
                wheels = new Wheels
                {
                    frontLeft = AddWheelCollider(frontLeftWheel),
                    frontRight = AddWheelCollider(frontRightWheel),
                    rearLeft = AddWheelCollider(rearLeftWheel),
                    rearRight = AddWheelCollider(rearRightWheel)
                };
            }
        }

        private WheelCollider AddWheelCollider(Transform wheel)
        {
            var col = wheel.gameObject.AddComponent<WheelCollider>();
            // calculate the radius and width of the wheel from the mesh, take the scale into account
            var mesh = wheel.GetComponent<MeshRenderer>().bounds;
            var radius = mesh.size.y / 2 / wheel.localScale.y;
            col.radius = radius;
            col.center = new Vector3(0, 0, radius);
            return col;
        }

        public void AddRigidbody()
        {
            // add a rigidbody to the car
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 1500;
            rb.drag = 0.5f;
            rb.angularDrag = 0.5f;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = false;
            
            // add a center of mass to the car
            var center = Vector3.zero;
            rb.centerOfMass = center;
            
            // add network transform to the car
            gameObject.AddComponent<NetworkTransform>();
            gameObject.AddComponent<NetworkRigidbody>();
        }
    }
}
