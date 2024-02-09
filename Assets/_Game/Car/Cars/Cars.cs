using System;
using System.Linq;
using UnityEngine;

namespace _Game.Car.Cars
{
    
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AllCarData", order = 1)]
    public class Cars : ScriptableObject
    {
        [SerializeField] public Car[] cars;
        
        public Car GetCar(int index)
        {
            return cars[index % cars.Length];
        }

        public void AddCar(Car prefab)
        {
            // check if the car is already in the array
            if (cars.Contains(prefab))
            {
                Debug.LogWarning("Car already exists in the database");
                return;
            }
            
            cars = cars.Append(prefab).ToArray();
            
            // remove any missing elements
            cars = cars.Where(car => car != null).ToArray();
        }
    }
}
