using UnityEngine;
using UnityEngine.Rendering;

namespace TwoTails.Prototype
{
    internal sealed class DamSlotPresentation
    {
        private readonly Transform anchor;
        private readonly GameObject piece;
        private readonly GameObject outline;
        public Transform Anchor => anchor;
        public Transform Piece => piece.transform;

        public DamSlotPresentation(GameObject authoredPiece, GameObject replacement, Material outlineMaterial, Mesh outlineMesh)
        {
            anchor = authoredPiece.transform;
            if (replacement == null)
            {
                piece = authoredPiece;
            }
            else
            {
                authoredPiece.SetActive(false);
                piece = Object.Instantiate(replacement, anchor.parent);
                piece.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            }

            outline = new GameObject(authoredPiece.name + " Outline", typeof(MeshFilter), typeof(MeshRenderer));
            outline.transform.SetParent(anchor.parent, false);
            outline.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            outline.transform.localScale = anchor.localScale * 1.015f;
            outline.GetComponent<MeshFilter>().sharedMesh = outlineMesh;
            var renderer = outline.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = outlineMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            // These reservoir pieces are presentation. The authored basin and ramps provide collision.
            foreach (var collider in piece.GetComponentsInChildren<Collider>(true))
                collider.enabled = false;
        }

        public void Apply(bool filled)
        {
            piece.SetActive(filled);
            outline.SetActive(!filled);
            if (filled)
            {
                var night = piece.GetComponentInParent<AnimalMechanicsWorld>()?.Night;
                if (night != null)
                    foreach (var renderer in piece.GetComponentsInChildren<MeshRenderer>())
                        night.Prepare(renderer);
            }
        }

        public static Mesh CreateBoxOutline()
        {
            var mesh = new Mesh { name = "Dam slot edges" };
            mesh.vertices = new[]
            {
                new Vector3(-.5f,-.5f,-.5f), new Vector3(.5f,-.5f,-.5f),
                new Vector3(.5f,.5f,-.5f), new Vector3(-.5f,.5f,-.5f),
                new Vector3(-.5f,-.5f,.5f), new Vector3(.5f,-.5f,.5f),
                new Vector3(.5f,.5f,.5f), new Vector3(-.5f,.5f,.5f)
            };
            mesh.SetIndices(new[] { 0,1,1,2,2,3,3,0,4,5,5,6,6,7,7,4,0,4,1,5,2,6,3,7 },
                MeshTopology.Lines, 0);
            return mesh;
        }
    }
}
