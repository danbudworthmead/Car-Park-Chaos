using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [SerializeField] private float maxSpeed;
    [SerializeField] private float torque;
    [SerializeField] private float turnSpeed;
    
    private Rigidbody _rigidbody;
    
    private float _acceleration;
    private float _steering;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void SetAcceleration(float acceleration)
    {
        _acceleration = acceleration * torque;
    }

    public void SetSteering(float steering)
    {
        _steering = steering * turnSpeed;
    }

    private void FixedUpdate()
    {
        // apply acceleration to the car
        _rigidbody.AddForce(transform.forward * _acceleration);
        
        // clamp the speed of the car (half if we are reversing)
        _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, 
            maxSpeed * (_acceleration < 0 ? 0.5f : 1f));
        
        // apply steering to the car but use the speed of the car
        // to make the steering more realistic
        // inverse the steering if we're reversing
        var turn = _steering * _rigidbody.velocity.magnitude;
        if (_acceleration < 0)
            turn *= -1;
        
        _rigidbody.MoveRotation(_rigidbody.rotation * Quaternion.Euler(0, turn, 0));
    }
}
