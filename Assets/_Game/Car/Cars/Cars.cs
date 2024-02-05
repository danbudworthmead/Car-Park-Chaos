using UnityEngine;

namespace _Game.Car.Cars
{
    
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AllCarData", order = 1)]
    public class Cars : ScriptableObject
    {
        public Car[] cars;
        
        public Car GetCar(int index)
        {
            return cars[index % cars.Length];
        }
    }
}
