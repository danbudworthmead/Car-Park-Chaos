using Unity.Netcode;

namespace _Game.Car.Player_Cars
{
    public class PlayerData : NetworkBehaviour
    {
        public string Player { get; private set; } = "NONAME";

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
