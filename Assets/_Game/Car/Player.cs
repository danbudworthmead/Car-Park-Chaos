using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private Cars.Cars carDatabase;
        [SerializeField] private Transform carPivot;
        
        [ClientRpc]
        public void InstantiateCarClientRpc(int carIdx)
        {
            // log this with owner id
            Debug.Log($"Player {OwnerClientId} is spawning car {carIdx}");
            Instantiate(carDatabase.GetCar(carIdx), carPivot.position, carPivot.rotation, carPivot);
        }
    }
}
