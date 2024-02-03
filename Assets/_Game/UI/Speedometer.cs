using System.Collections;
using System.Collections.Generic;
using _Game.Car;
using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [SerializeField] private TMP_Text speedText;
    private CarPhysics _carPhysics;
    
    private void Update()
    {
        if (!_carPhysics)
        {
            _carPhysics = FindObjectOfType<CarController>()?.GetComponent<CarPhysics>();
            return;
        }
        
        speedText.text = _carPhysics ? _carPhysics.Speed.ToString("0") : "CarPhysics is null";
    }
}
