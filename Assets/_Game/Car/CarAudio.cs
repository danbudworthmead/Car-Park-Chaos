using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class CarAudio : NetworkBehaviour
    {
        [SerializeField]
        private CarPhysics carPhysics;
    
        [SerializeField]
        private AudioSource engineAudio;
        
        [SerializeField]
        private AudioSource hornAudio;
    
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
            engineAudio.pitch = 1+carPhysics.EngineRPM.Value/12f;
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