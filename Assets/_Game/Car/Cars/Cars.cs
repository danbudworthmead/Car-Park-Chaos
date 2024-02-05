using UnityEngine;

namespace _Game.Car.Cars
{
    
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AllCarData", order = 1)]
    public class Cars : ScriptableObject
    {
        public GameObject[] cars;
        
        public GameObject GetCar(int index)
        {
            return cars[index % cars.Length];
        }
    }
}
