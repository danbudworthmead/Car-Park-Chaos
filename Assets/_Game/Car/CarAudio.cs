using UnityEngine;
using Unity.Netcode;

public class CarAudio : NetworkBehaviour
{
    private Rigidbody carPhysics;
    private AudioSource carAudio;

    private NetworkVariable<float> pitch = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    // Start is called before the first frame update
    void Start()
    {
        carAudio = GetComponent<AudioSource>();
        carPhysics = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            pitch.Value = 1+carPhysics.velocity.magnitude/12f;
        }
        carAudio.pitch = pitch.Value;
    }
}