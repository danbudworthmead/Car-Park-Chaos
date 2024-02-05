using Unity.Netcode;
using UnityEngine;

namespace _Game.Car.Player_Cars
{
    public class PlayerData : NetworkBehaviour
    {
        public string Player { get; private set; } = "NONAME";
        
        [SerializeField] private Cars.Cars carDatabase;

        public NetworkVariable<int> carChoice = new(-1, 
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private void Start()
        {
            if (IsOwner)
            {
                carChoice.Value = Random.Range(0, carDatabase.cars.Length);
            }
        }

        private void Update()
        {
            name = Player;
        }

        [ClientRpc]
        public void SetClientRpc(string player)
        {
            Player = player;
        }
    }
}
