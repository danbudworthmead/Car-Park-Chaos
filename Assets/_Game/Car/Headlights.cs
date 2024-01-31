using UnityEngine;

namespace _Game.Car
{
    public class Headlights : MonoBehaviour
    {
        [SerializeField] private Material off;
        [SerializeField] private Material on;
    
        public void SetOn(bool on)
        {
            GetComponent<Renderer>().material = on ? this.on : off;
        }
    }
}
