using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class CarAudio : NetworkBehaviour
    {
        [SerializeField]
        public CarPhysics carPhysics;
    
        [SerializeField]
        public AudioSource engineAudio;
        
        [SerializeField]
        public AudioSource hornAudio;
    
        // Start is called before the first frame update
        private void Start()
        {
            Debug.Assert(carPhysics);
            Debug.Assert(engineAudio);
            Debug.Assert(hornAudio);
        }

        // Update is called once per frame
        private void Update()
        {
            engineAudio.pitch = 1+carPhysics.engineRpm.Value/12f;
        }

        [ServerRpc]
        public void HonkHornServerRpc()
        {
            HonkHornClientRpc();
        }

        [ClientRpc]
        public void HonkHornClientRpc()
        {
            hornAudio.Play();
        }
    }
}