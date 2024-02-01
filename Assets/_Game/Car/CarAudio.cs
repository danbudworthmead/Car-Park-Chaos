using UnityEngine;
using Unity.Netcode;

public class CarAudio : NetworkBehaviour
{
    private Rigidbody carPhysics;
    private AudioSource carAudio;
    
    // Start is called before the first frame update
    void Start()
    {
        carAudio = GetComponent<AudioSource>();
        carPhysics = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        carAudio.pitch= 1+carPhysics.velocity.magnitude/10f;
    }
}