using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    private CarPhysics _carPhysics;

    private void Awake()
    {
        _carPhysics = GetComponent<CarPhysics>();
    }

    public void OnAccelerate(InputValue value)
    {
        _carPhysics.SetAcceleration(value.Get<float>());
    }
    
    public void OnBrake(InputValue value)
    {
        _carPhysics.SetBrake(value.Get<float>());
    }

    public void OnSteering(InputValue value)
    {
        _carPhysics.SetSteering(value.Get<float>());
    }
}
