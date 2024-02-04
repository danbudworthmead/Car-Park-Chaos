using _Game.Car.Player_Cars;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Nametag : MonoBehaviour
{
    [SerializeField] private TextMeshPro textObject;
    private PlayerData data;
    private void Update()
    {
        if (!data)
        {
            data = transform.parent.GetComponent<PlayerData>();
            return;
        }

        textObject.text = data.Player;
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
        Camera.main.transform.rotation * Vector3.up);
    }
}
