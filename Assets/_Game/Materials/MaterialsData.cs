using System;
using UnityEngine;

namespace _Game.Materials
{
    [Serializable]
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Materials", order = 2)]
    public class MaterialsData : ScriptableObject
    {
        private static MaterialsData _instance;
        
        [SerializeField] private Material[] materials;

        private void Awake()
        {
            _instance = this;
        }

        public static Material Get(int index)
        {
            if (_instance == null)
            {
                // load self from resources
                _instance = Resources.Load<MaterialsData>("MaterialsData");
            }
            
            return _instance.materials[index % _instance.materials.Length];
        }
    }
}
