using System.Collections.Generic;
using _Game.Car;
using UnityEngine;

public class CarEffects : MonoBehaviour
{
    [SerializeField] private Transform skidMarkParent;
    
    [SerializeField] private CarPhysics carPhysics;
    
    [Range(0.0f, 1.0f)] public float skidGate = .17f;

    private List<TrailRenderer> skidMarks = new ();
    
    private void Start()
    {
        foreach (Transform child in skidMarkParent)
        {
            var t = child.gameObject.GetComponent<TrailRenderer>();
            skidMarks.Add(t);
        }
    }

    private void StartEmitter()
    {
        foreach (var t in skidMarks)
        {
            if (!t.emitting)
            {
                t.emitting = true;
            }
        }
    }

    private void StopEmitter()
    {
        foreach (var t in skidMarks)
        {
            if (t.emitting)
            {
                t.emitting = false;
            }
        }
    }

    public void Update()
    {
        if (carPhysics.SpeedDotProduct > skidGate)
        {
            StartEmitter();
        }
        else
        {
            StopEmitter();
        }
    }

    private void LateUpdate()
    {
        var rotation = skidMarkParent.rotation.eulerAngles;
        skidMarkParent.rotation = Quaternion.Euler(0,rotation.y,rotation.z);
    }
}
