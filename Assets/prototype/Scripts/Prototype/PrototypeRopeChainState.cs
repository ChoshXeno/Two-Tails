using System;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    // Eight links; the fixed anchor is supplied by the scene transform.
    public struct PrototypeRopeChainState : INetworkSerializable, IEquatable<PrototypeRopeChainState>
    {
        public Vector3 P1, P2, P3, P4, P5, P6, P7, P8;

        public Vector3 Get(int i) => i switch
        {
            1 => P1, 2 => P2, 3 => P3, 4 => P4,
            5 => P5, 6 => P6, 7 => P7, _ => P8
        };

        public void Set(int i, Vector3 value)
        {
            switch (i)
            {
                case 1: P1 = value; break; case 2: P2 = value; break;
                case 3: P3 = value; break; case 4: P4 = value; break;
                case 5: P5 = value; break; case 6: P6 = value; break;
                case 7: P7 = value; break; default: P8 = value; break;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref P1); serializer.SerializeValue(ref P2);
            serializer.SerializeValue(ref P3); serializer.SerializeValue(ref P4);
            serializer.SerializeValue(ref P5); serializer.SerializeValue(ref P6);
            serializer.SerializeValue(ref P7); serializer.SerializeValue(ref P8);
        }

        public bool Equals(PrototypeRopeChainState other) =>
            P1 == other.P1 && P2 == other.P2 && P3 == other.P3 && P4 == other.P4 &&
            P5 == other.P5 && P6 == other.P6 && P7 == other.P7 && P8 == other.P8;
    }
}
