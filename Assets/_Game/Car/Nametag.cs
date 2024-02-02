using _Game.Car.Player_Cars;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Nametag : MonoBehaviour
{
    [SerializeField] private TextMeshPro textObject;
    [SerializeField] private PlayerData data;
    public void Setup()
    {
        textObject.text = data.Player;
    }
}
