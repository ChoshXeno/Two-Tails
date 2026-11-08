using UnityEngine;

namespace TwoTails.Prototype
{
    public sealed class PrototypeDebugPlayers : MonoBehaviour
    {
        [SerializeField, Tooltip("Enable DebugPlayer2 in the Editor and Development Builds. Release players always keep it disabled.")] private bool enablePlayer2 = true;
        [SerializeField, Tooltip("Scene instance kept inactive in the saved scene.")] private GameObject player2;

        private void Awake()
        {
            if (player2 != null)
                player2.SetActive(enablePlayer2 && (Application.isEditor || Debug.isDebugBuild));
        }
    }
}
