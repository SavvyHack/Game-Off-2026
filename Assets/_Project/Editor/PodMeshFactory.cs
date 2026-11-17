using System.Collections.Generic;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>Procedural meshes for the greybox pod: hollow tubes (hull, bulkheads, ribs) and a star field.</summary>
    public static class PodMeshFactory
    {
        /// <summary>
        /// A hollow tube along the Z axis, centred on the origin. The inner surface faces the axis, so the
        /// same shape works as a room you stand inside (hull) and as a ring you walk through (bulkhead).
        /// </summary>
        public static Mesh Tube(string name, float innerRadius, float outerRadius, float depth, int segments)
        {
            var builder = new Builder();
            float half = depth * 0.5f;

            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.PI * 2f * i / segments;
                float a1 = Mathf.PI * 2f * (i + 1) / segments;
                var d0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f);
                var d1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f);
                Vector3 front = Vector3.forward * half;
                Vector3 back = Vector3.back * half;

                // Inner wall, normals towards the axis.
                builder.Quad(d0 * innerRadius + back, d0 * innerRadius + front,
                    d1 * innerRadius + front, d1 * innerRadius + back, -d0, -d0, -d1, -d1);

                // Outer wall, normals away from the axis.
                builder.Quad(d0 * outerRadius + back, d0 * outerRadius + front,
                    d1 * outerRadius + front, d1 * outerRadius + back, d0, d0, d1, d1);

                // End faces.
                builder.Quad(d0 * innerRadius + front, d0 * outerRadius + front,
                    d1 * outerRadius + front, d1 * innerRadius + front, Vector3.forward);
                builder.Quad(d0 * innerRadius + back, d0 * outerRadius + back,
                    d1 * outerRadius + back, d1 * innerRadius + back, Vector3.back);
            }

            return builder.ToMesh(name);
        }

        /// <summary>Small quads scattered on a sphere, facing its centre. Seeded so rebuilds are identical.</summary>
        public static Mesh StarField(string name, int count, float radius, int seed)
        {
            var builder = new Builder();
            var random = new System.Random(seed);

            for (int i = 0; i < count; i++)
            {
                // Uniform direction on a sphere.
                float z = (float)(random.NextDouble() * 2.0 - 1.0);
                float angle = (float)(random.NextDouble() * Mathf.PI * 2.0);
                float r = Mathf.Sqrt(1f - z * z);
                var direction = new Vector3(r * Mathf.Cos(angle), r * Mathf.Sin(angle), z);

                float size = radius * Mathf.Lerp(0.0012f, 0.0042f, (float)random.NextDouble());
                Vector3 right = Vector3.Cross(direction, Mathf.Abs(direction.y) > 0.9f ? Vector3.right : Vector3.up).normalized * size;
                Vector3 up = Vector3.Cross(right, direction).normalized * size;
                Vector3 centre = direction * radius;

                builder.Quad(centre - right - up, centre - right + up, centre + right + up, centre + right - up, -direction);
            }

            return builder.ToMesh(name);
        }

        private sealed class Builder
        {
            private readonly List<Vector3> vertices = new List<Vector3>();
            private readonly List<Vector3> normals = new List<Vector3>();
            private readonly List<int> triangles = new List<int>();

            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal) => Quad(a, b, c, d, normal, normal, normal, normal);

            /// <summary>Corners in loop order. Winding is chosen so the face is visible from the side its normals point to.</summary>
            public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 na, Vector3 nb, Vector3 nc, Vector3 nd)
            {
                int i = vertices.Count;
                vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
                normals.Add(na.normalized); normals.Add(nb.normalized); normals.Add(nc.normalized); normals.Add(nd.normalized);

                bool flip = Vector3.Dot(Vector3.Cross(b - a, c - a), na + nb + nc + nd) < 0f;
                if (flip)
                {
                    triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 1);
                    triangles.Add(i); triangles.Add(i + 3); triangles.Add(i + 2);
                }
                else
                {
                    triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
                    triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
                }
            }

            public Mesh ToMesh(string name)
            {
                var mesh = new Mesh { name = name };
                mesh.SetVertices(vertices);
                mesh.SetNormals(normals);
                mesh.SetTriangles(triangles, 0);
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
