using System;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    public struct AnimalMechanicsState : INetworkSerializable, IEquatable<AnimalMechanicsState>
    {
        public const int SlotLimit = 16;
        public const int LogLimit = 15;
        public byte SpeciesA, SpeciesB, SpeciesC, SpeciesD;
        public Vector3 PositionC, PositionD;
        public double BusyC, BusyD;
        public float HeavyOffset;
        public float HeavyDrop;
        public bool BoardBroken;
        public Vector3 PositionA, PositionB;
        public int Picked, Revision;
        // Each nibble stores zero for empty, or the original log index plus one.
        public ulong Slots;
        public float WaterLevel;
        public double BusyA, BusyB;

        public int GetSlot(int index) => (int)((Slots >> (index * 4)) & 15UL);

        public void SetSlot(int index, int log)
        {
            int shift = index * 4;
            Slots = (Slots & ~(15UL << shift)) | ((ulong)log << shift);
        }

        public bool TryBuild(int slotCount, ref int inventory)
        {
            if (inventory == 0) return false;
            for (int slot = 0; slot < slotCount; slot++)
            {
                if (GetSlot(slot) != 0) continue;
                int log = 0;
                while ((inventory & (1 << log)) == 0) log++;
                SetSlot(slot, log + 1);
                inventory &= ~(1 << log);
                return true;
            }
            return false;
        }

        public bool TryReclaim(int slot, ref int inventory)
        {
            int log = GetSlot(slot);
            if (log == 0) return false;
            SetSlot(slot, 0);
            inventory |= 1 << (log - 1);
            return true;
        }

        public int Progress
        {
            get
            {
                int count = 0;
                for (ulong slots = Slots; slots != 0; slots >>= 4)
                    if ((slots & 15UL) != 0) count++;
                return count;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref SpeciesC);
            serializer.SerializeValue(ref SpeciesD);
            serializer.SerializeValue(ref PositionC);
            serializer.SerializeValue(ref PositionD);
            serializer.SerializeValue(ref BusyC);
            serializer.SerializeValue(ref BusyD);
            serializer.SerializeValue(ref HeavyOffset);
            serializer.SerializeValue(ref HeavyDrop);
            serializer.SerializeValue(ref BoardBroken);
            serializer.SerializeValue(ref SpeciesA);
            serializer.SerializeValue(ref SpeciesB);
            serializer.SerializeValue(ref PositionA);
            serializer.SerializeValue(ref PositionB);
            serializer.SerializeValue(ref Picked);
            serializer.SerializeValue(ref Slots);
            serializer.SerializeValue(ref Revision);
            serializer.SerializeValue(ref WaterLevel);
            serializer.SerializeValue(ref BusyA);
            serializer.SerializeValue(ref BusyB);
        }

        public bool Equals(AnimalMechanicsState other) =>
            SpeciesC == other.SpeciesC && SpeciesD == other.SpeciesD &&
            PositionC == other.PositionC && PositionD == other.PositionD && BusyC == other.BusyC && BusyD == other.BusyD &&
            HeavyOffset == other.HeavyOffset && HeavyDrop == other.HeavyDrop && BoardBroken == other.BoardBroken && SpeciesA == other.SpeciesA && SpeciesB == other.SpeciesB &&
            PositionA == other.PositionA && PositionB == other.PositionB &&
            Picked == other.Picked && Slots == other.Slots && Revision == other.Revision &&
            WaterLevel == other.WaterLevel && BusyA == other.BusyA && BusyB == other.BusyB;
    }
}
