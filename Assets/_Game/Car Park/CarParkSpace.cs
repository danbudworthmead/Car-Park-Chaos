using UnityEngine;

namespace _Game.Car_Park
{
    public class CarParkSpace : MonoBehaviour
    {
        [SerializeField] private GameObject[] cars;
        
        private void Start()
        {
            var car = Instantiate(cars[Random.Range(0, cars.Length)], transform);
            car.transform.localPosition = Vector3.zero;
            car.transform.localRotation = Quaternion.identity;
        }
    }
}