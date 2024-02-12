using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public struct VehicleInput : INetworkSerializable
    {
        public float accelerator;
        public float brake;
        public float steering;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref accelerator);
            serializer.SerializeValue(ref brake);
            serializer.SerializeValue(ref steering);
        }

        public void Clamp()
        {
            accelerator = Mathf.Clamp01(accelerator);
            brake = Mathf.Clamp01(brake);
            steering = Mathf.Clamp(steering, -1f, 1f);
        }
    }
}