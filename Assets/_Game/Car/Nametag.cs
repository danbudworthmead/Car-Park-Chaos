using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Nametag : MonoBehaviour
{
    [SerializeField] private TextMeshPro textObject;
    public void Setup()
    {
        textObject.text = NetworkManager.Singleton.LocalClient.ClientId.ToString(); 
    }
}
