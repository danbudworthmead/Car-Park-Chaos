using _Game.Car;
using UnityEngine;
using Unity.Netcode;

public class CarAudio : NetworkBehaviour
{
    [SerializeField]
    private CarPhysics carPhysics;
    
    [SerializeField]
    private AudioSource carAudio;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!carAudio) carAudio = GetComponent<AudioSource>();
        if (!carPhysics) carPhysics = GetComponent<CarPhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        carAudio.pitch = 1+carPhysics.EngineRPM.Value/12f;
    }
}