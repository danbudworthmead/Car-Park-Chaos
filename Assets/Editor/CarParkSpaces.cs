using System.Collections.Generic;
using _Game.Car_Park;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Editor
{
    public static class CarParkSpaces
    {
        [MenuItem("Utils/Spaces/Duplicate space finder")]
        public static void FindDuplicateSpaces()
        {
            var spaces = Object.FindObjectsOfType<CarParkSpace>();
            var duplicates = new List<CarParkSpace>();
            for (var i = 0; i < spaces.Length; i++)
            {
                for (var j = i + 1; j < spaces.Length; j++)
                {
                    // distance check
                    if (Vector3.Distance(spaces[i].transform.position, spaces[j].transform.position) < 0.1f)
                    {
                        duplicates.Add(spaces[i]);
                        duplicates.Add(spaces[j]);
                    }
                }
            }
            
            // log the duplicates
            if (duplicates.Count > 0)
            {
                Debug.Log($"Found duplicate spaces: {duplicates.Count}");
                foreach (var duplicate in duplicates)
                {
                    Debug.Log(duplicate.name);
                }
            }
            else
            {
                Debug.Log("No duplicate spaces found");
            }
        }
        
        [MenuItem("Utils/Spaces/Name spaces")]
        public static void NameSpaces()
        {
            var spaces = Object.FindObjectsOfType<CarParkSpace>();
            for (var i = 0; i < spaces.Length; i++)
            {
                spaces[i].name = $"Space ({i})";
            }
        }
        
        [MenuItem("Utils/Spaces/Fix spaces array")]
        public static void FixSpacesArray()
        {
            var spaces = Object.FindObjectsOfType<CarParkSpace>();
            Object.FindObjectOfType<_Game.Car_Park.CarParkSpaces>().spaces = spaces;
        }
    }
}
#endif