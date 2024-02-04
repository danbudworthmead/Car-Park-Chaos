using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public class CarAudio : NetworkBehaviour
    {
        [SerializeField]
        private CarPhysics carPhysics;
    
        [SerializeField]
        private AudioSource carAudio;
    
        // Start is called before the first frame update
        private void Start()
        {
            Debug.Assert(carAudio);
            Debug.Assert(carPhysics);
        }

        // Update is called once per frame
        private void Update()
        {
            carAudio.pitch = 1+carPhysics.EngineRPM.Value/12f;
        }
    }
}