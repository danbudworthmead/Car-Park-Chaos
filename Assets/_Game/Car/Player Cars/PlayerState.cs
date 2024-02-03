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
            Parked,
            Dead,
        }
        
        private PlayerStates _state = PlayerStates.Alive;
        
        public bool IsAlive => _state == PlayerStates.Alive;
        public bool IsDead => _state == PlayerStates.Dead;

        [ClientRpc]
        public void SetAliveClientRpc()
        {
            _state = PlayerStates.Alive;
            carRigidbody.isKinematic = false;
        }

        [ClientRpc]
        public void SetParkedClientRpc()
        {
            _state = PlayerStates.Parked;
            // Freeze car? (probably not the best method but)
            carRigidbody.isKinematic = true;
            Debug.Log($"{OwnerClientId} has parked");
        }

        [ClientRpc]
        public void SetDeadClientRpc()
        {
            _state = PlayerStates.Dead;
            Debug.Log($"{OwnerClientId} has been knocked out!");
        }

        private void Update()
        {
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
            {
                meshRenderer.material.color = IsAlive ? Color.green : Color.red;
            }
        }
    }
}
