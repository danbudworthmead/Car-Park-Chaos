using _Game.Car;
using TMPro;
using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float torque;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float suspension;
    [SerializeField] private float tireHeight;
    [SerializeField] private Transform wheelsParent;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private Headlights headlights;
    
    private float _acceleration;
    private float _steering;

    public void SetAcceleration(float acceleration)
    {
        headlights.SetOn(acceleration < 0);
        
        if (acceleration < 0)
        {
            acceleration *= 0.5f;
        }

        _acceleration = acceleration * torque;
    }

    public void SetSteering(float steering)
    {
        _steering = steering * turnSpeed;
    }

    private void FixedUpdate()
    {
        for (var i = 0; i < wheelsParent.childCount; ++i)
        {
            var wheel = wheelsParent.GetChild(i);
            if (Physics.Raycast(wheel.transform.position, -wheel.transform.up *tireHeight, out var hit, 1f))
            {
                // apply suspension to the car
                hit.distance = tireHeight - hit.distance;
                rigidbody.AddForceAtPosition(transform.up * (suspension * hit.distance), hit.point);
            
                // apply acceleration to each wheel
                rigidbody.AddForceAtPosition(wheel.transform.forward * _acceleration, hit.point);
            }
            
            // if we are the front two wheels
            if (i < 2)
            {
                // apply steering to the wheels
                wheel.localRotation = Quaternion.Lerp(wheel.localRotation, 
                    Quaternion.Euler(0, _steering, 0), 0.1f);
            }
        }
        
        // clamp the speed of the car
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, maxSpeed);
        speedText.text = rigidbody.velocity.magnitude.ToString("0.00");
    }
    
    private void OnDrawGizmos()
    {
        for (int i = 0; i < wheelsParent.childCount; ++i)
        {
            var wheel = wheelsParent.GetChild(i);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(wheel.transform.position, -wheel.transform.up * tireHeight);
            Gizmos.DrawRay(wheel.transform.position, wheel.transform.forward * 1f);
        }
    }
}
