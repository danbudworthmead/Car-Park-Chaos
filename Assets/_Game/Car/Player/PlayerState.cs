using System;
using Unity.Netcode;
using UnityEngine;

namespace _Game.Car.Player_Cars
{
    public class PlayerState : NetworkBehaviour
    {
        [SerializeField] private Rigidbody carRigidbody;
        [SerializeField] private CarPhysics physics;

        public enum PlayerStates
        {
            Alive,
            Dead,
        }
        
        private PlayerStates _state = PlayerStates.Alive;
        public bool _isParked = false;
        public bool IsAlive => _state == PlayerStates.Alive;
        public bool IsDead => _state == PlayerStates.Dead;

        [ClientRpc]
        public void UnfreezeClientRpc()
        {
            carRigidbody.constraints = RigidbodyConstraints.None;
        }

        public void SetParked()
        {
            if (_isParked)
            {
                return;
            }
            
            _isParked = true;
            // Freeze car? (probably not the best method but)
            carRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            Debug.Log($"{OwnerClientId} has parked");
        }

        [ClientRpc]
        public void SetDeadClientRpc()
        {
            _state = PlayerStates.Dead;
            Debug.Log($"{OwnerClientId} has been knocked out!");
        }
    }
}
