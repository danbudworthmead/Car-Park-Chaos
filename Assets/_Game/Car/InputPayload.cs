using Unity.Netcode;
using UnityEngine;

namespace _Game.Car
{
    public struct InputPayload : INetworkSerializable
    {
        public int tick;
        public VehicleInput input;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref input);
        }
    }
}