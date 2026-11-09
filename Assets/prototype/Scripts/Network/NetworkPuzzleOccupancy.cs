using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TwoTails.Prototype
{
    internal sealed class NetworkPuzzleOccupancy
    {
        private readonly Collider[] buffer = new Collider[12];

        internal static bool IsCurrentPlayer(NetworkManager manager, NetworkPlayerPrototype player)
        {
            return player != null && player.isActiveAndEnabled && player.IsSpawned &&
                player.NetworkObject.IsPlayerObject &&
                manager.ConnectedClients.TryGetValue(player.OwnerClientId, out var connection) &&
                connection.PlayerObject == player.NetworkObject;
        }

        internal void Sample(NetworkManager manager, BoxCollider volume, LayerMask layers, HashSet<ulong> players)
        {
            players.Clear();
            if (!volume.enabled || !volume.gameObject.activeInHierarchy)
                return;

            Transform pose = volume.transform;
            Vector3 center = pose.TransformPoint(volume.center);
            Vector3 scale = pose.lossyScale;
            Vector3 half = Vector3.Scale(volume.size * 0.5f,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            int count = Physics.OverlapBoxNonAlloc(center, half, buffer, pose.rotation, layers, QueryTriggerInteraction.Collide);
            var colliders = count == buffer.Length
                ? Physics.OverlapBox(center, half, pose.rotation, layers, QueryTriggerInteraction.Collide)
                : buffer;
            if (colliders != buffer)
                count = colliders.Length;

            for (int i = 0; i < count; i++)
            {
                var player = colliders[i].GetComponentInParent<NetworkPlayerPrototype>();
                if (IsCurrentPlayer(manager, player))
                    players.Add(player.NetworkObjectId);
            }
        }
    }
}
