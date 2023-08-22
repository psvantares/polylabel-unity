using UnityEngine;
using UnityEngine.Rendering;

namespace Sandbox.PolyLabel
{
    public class ShapeView : MonoBehaviour
    {
        private Color circleColor;
        private Vector3 circlePos;
        private float circleSize;
        private Renderer shapeRenderer;
        private MeshFilter meshFilter;
        private Collider shapeCollider;
        private MaterialPropertyBlock prop;

        public void Initialize(Material shapeMaterial)
        {
            if (shapeRenderer == null)
            {
                shapeRenderer = GetComponent<Renderer>();
                shapeRenderer.shadowCastingMode = ShadowCastingMode.Off;
                shapeRenderer.receiveShadows = false;
                shapeRenderer.lightProbeUsage = LightProbeUsage.Off;
                shapeRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }

            shapeRenderer.sharedMaterial = shapeMaterial;

            if (shapeCollider == null)
            {
                shapeCollider = gameObject.AddComponent<MeshCollider>();
            }

            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            SetActiveCollider(true);
            SetActiveRenderer(true);
            SetColorShape(Color.white);

            prop ??= new MaterialPropertyBlock();
        }

        public void FillColor(Color color)
        {
            if (!shapeCollider.enabled)
            {
                return;
            }

            SetColorShape(color);
            SetActiveCollider(false);
        }

        public int GetVerticesCount()
        {
            return meshFilter == null ? 0 : meshFilter.mesh.vertexCount;
        }

        public int GetTrianglesCount()
        {
            return meshFilter == null ? 0 : meshFilter.mesh.triangles.Length / 3;
        }

        private void SetColorShape(Color color)
        {
            var vertices = meshFilter.mesh.vertices;
            var colors = new Color[vertices.Length];

            for (var i = 0; i < vertices.Length; i++)
            {
                colors[i] = color;
            }

            meshFilter.mesh.colors = colors;
        }

        private void SetActiveCollider(bool active)
        {
            shapeCollider.enabled = active;
        }

        private void SetActiveRenderer(bool active)
        {
            shapeRenderer.enabled = active;
        }
    }
}