using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    [SerializeField] private CarPhysics carPhysics;
    [SerializeField] private TMP_Text speedText;
    
    private void Update()
    {
        speedText.text = carPhysics.Speed.ToString("0");
    }
}
