using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Game.Car
{
    public class CarAudio : NetworkBehaviour
    {
        [SerializeField] private CarPhysics carPhysics;
    
        [SerializeField] private AudioSource engineAudio;
        
        [SerializeField] private AudioSource hornAudio;

        [SerializeField] private AudioSource slipAudio;

        [Range(0.0f, 1.0f)] public float noiseGate = .17f;

        [SerializeField] public float lerpSpeed = 10f;
        
        // Start is called before the first frame update
        private void Start()
        {
            Debug.Assert(carPhysics);
            Debug.Assert(engineAudio);
            Debug.Assert(hornAudio);
            Debug.Assert(slipAudio);
        }

        // Update is called once per frame
        private void Update()
        {
            engineAudio.pitch = 1+carPhysics.EngineRPM.Value/12f;

            if (carPhysics.SpeedDotProduct > noiseGate)
            {
                slipAudio.volume = Mathf.Lerp(slipAudio.volume, carPhysics.SpeedDotProduct, Time.deltaTime*lerpSpeed);
            }
            else
            {
                slipAudio.volume = Mathf.Lerp(slipAudio.volume, 0, Time.deltaTime*lerpSpeed);
            }
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