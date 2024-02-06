using _Game.Car.Player;
using _Game.Car.Player_Cars;
using TMPro;
using UnityEngine;

namespace _Game.Car
{
    public class Nametag : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textObject;
        private PlayerData _data;
        private PlayerState _state;
    
        private void Update()
        {
            if (!_data)
            {
                _data = transform.parent.GetComponent<PlayerData>();
                _state = _data.GetComponent<PlayerState>();
                return;
            }

            textObject.text = $"{_data.Player}: {(_state.IsAlive ? "Alive" : "Dead")}";
            var rotation = UnityEngine.Camera.main.transform.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward, 
                rotation * Vector3.up);
        }
    }
}
