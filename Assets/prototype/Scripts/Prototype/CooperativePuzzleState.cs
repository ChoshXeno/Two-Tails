using System;
using Unity.Netcode;
using UnityEngine;
namespace TwoTails.Prototype
{
 public enum CooperativePuzzleKind { Switchback, Bridge, Counterweight }
 public struct CooperativePuzzleState : INetworkSerializable, IEquatable<CooperativePuzzleState>
 {
  public bool Latched; public float A,B; public int WeightA,WeightB,Revision; public byte Plates;
  public Vector3 Box0,Box1,Box2;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter
  {s.SerializeValue(ref Latched);s.SerializeValue(ref A);s.SerializeValue(ref B);s.SerializeValue(ref WeightA);s.SerializeValue(ref WeightB);s.SerializeValue(ref Revision);s.SerializeValue(ref Plates);s.SerializeValue(ref Box0);s.SerializeValue(ref Box1);s.SerializeValue(ref Box2);}
  public bool Equals(CooperativePuzzleState o)=>Latched==o.Latched&&A==o.A&&B==o.B&&WeightA==o.WeightA&&WeightB==o.WeightB&&Revision==o.Revision&&Plates==o.Plates&&Box0==o.Box0&&Box1==o.Box1&&Box2==o.Box2;
 }
}
