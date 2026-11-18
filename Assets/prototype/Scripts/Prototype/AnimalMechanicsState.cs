using System;
using Unity.Netcode;
using UnityEngine;
namespace TwoTails.Prototype
{
 public struct AnimalMechanicsState:INetworkSerializable,IEquatable<AnimalMechanicsState>
 {
  public byte SpeciesA,SpeciesB;public Vector3 PositionA,PositionB;public int Picked,Deposited,Revision;public float WaterLevel;public double BusyA,BusyB;
  public void NetworkSerialize<T>(BufferSerializer<T> s) where T:IReaderWriter{s.SerializeValue(ref SpeciesA);s.SerializeValue(ref SpeciesB);s.SerializeValue(ref PositionA);s.SerializeValue(ref PositionB);s.SerializeValue(ref Picked);s.SerializeValue(ref Deposited);s.SerializeValue(ref Revision);s.SerializeValue(ref WaterLevel);s.SerializeValue(ref BusyA);s.SerializeValue(ref BusyB);}
  public bool Equals(AnimalMechanicsState o)=>SpeciesA==o.SpeciesA&&SpeciesB==o.SpeciesB&&PositionA==o.PositionA&&PositionB==o.PositionB&&Picked==o.Picked&&Deposited==o.Deposited&&Revision==o.Revision&&WaterLevel==o.WaterLevel&&BusyA==o.BusyA&&BusyB==o.BusyB;
 }
}
