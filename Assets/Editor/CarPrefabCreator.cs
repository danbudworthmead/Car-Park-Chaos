using _Game.Car;
using UnityEngine;

namespace Editor
{
    public static class CarPrefabCreator
    {
        [UnityEditor.MenuItem("Utils/Create car prefab")]
        public static void CreateCarPrefab()
        {
            CreateCar(true);
            CreateCar(false);
            
            // save the prefab
            UnityEditor.AssetDatabase.SaveAssets();
            
            // log success
            Debug.Log("Prefab created");
        }

        private static void CreateCar(bool isDrivable)
        {
            // this method will take the selected model and create a prefab from it
            var selected = UnityEditor.Selection.activeGameObject;
            if (selected == null)
            {
                // log an error
                Debug.LogError("No object selected");
                return;
            }

            var name = selected.name;
            var folder = $"Assets/_Game/Car/Cars/{(isDrivable ? "Player" : "Npc")}";
            
            // create the folder if it doesn't exist
            if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
            {
                UnityEditor.AssetDatabase.CreateFolder("Assets/_Game/Car/Cars", isDrivable ? "Player" : "Npc");
            }
            
            var path = $"{folder}/{name}.prefab";
            
            var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(selected, path, out var success);
            if (!success)
            {
                // log an error
                Debug.LogError("Prefab creation failed");
                return;
            }
            
            // add car components
            var drivable = prefab.AddComponent<Car>();
            drivable.transform.position = Vector3.zero;
            if (isDrivable)
            {
                drivable.CreatePlayerCarPrefab();
            }
            else
            {
                drivable.CreateNpcCarPrefab();
            }
            
            // add to the scriptable object database
            var databasePath = $"Assets/_Game/Car/Cars/{(isDrivable ? "PlayerCars" : "NpcCars")}.asset";
            var carDatabase = UnityEditor.AssetDatabase.LoadAssetAtPath<_Game.Car.Cars.Cars>(databasePath);
            if (carDatabase == null)
            {
                // create the database if it doesn't exist
                carDatabase = ScriptableObject.CreateInstance<_Game.Car.Cars.Cars>();
                UnityEditor.AssetDatabase.CreateAsset(carDatabase, databasePath);
            }
            
            // add the car to the database
            carDatabase.AddCar(prefab.GetComponent<Car>());
        }
    }
}