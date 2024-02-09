using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Game.Car
{
    public class CarController : MonoBehaviour
    {
        private CarPhysics _carPhysics;
        private CarAudio _carAudio;

        private void Update()
        {
            var car = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (!car)
            {
                _carPhysics = null;
                _carAudio = null;
                return;
            }

            if (car.GetComponent<Car>())
            {
                _carPhysics = car.GetComponent<CarPhysics>();
                _carAudio = car.GetComponent<CarAudio>();
            }
        }

        public void OnAccelerate(InputValue value)
        {
            if (!_carPhysics) return;
            _carPhysics.SetAccelerationServerRpc(value.Get<float>());
        }
    
        public void OnBrake(InputValue value)
        {
            if (!_carPhysics) return;
            _carPhysics.SetBrakeServerRpc(value.Get<float>());
        }

        public void OnSteering(InputValue value)
        {
            if (!_carPhysics) return;
            _carPhysics.SetSteeringServerRpc(value.Get<float>());
        }

        public void OnHorn(InputValue value)
        {
            if (!_carAudio) return;
            _carAudio.HonkHornServerRpc();
        }
    }
}
