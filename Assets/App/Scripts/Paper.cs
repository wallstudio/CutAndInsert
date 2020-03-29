using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using mattatz.Triangulation2DSystem;
using UnityEngine;


namespace CAI
{
    public class Paper : MonoBehaviour
    {
        
        [SerializeField] Transform refarenceTransform = null;
        [SerializeField] Renderer refarenceRenderer = null;
        [SerializeField] MeshFilter refarenceMeshFilter = null;
        [SerializeField] MeshFilter meshFilter = null;

        Mesh mesh;

        void Start()
        {
            refarenceRenderer.enabled = false;
            Mesh refarenceMesh = refarenceMeshFilter.mesh;
            mesh = new Mesh();
            mesh.SetVertices(refarenceMesh.vertices.Select(v => Vector3.Scale(v, refarenceTransform.localScale)).ToList());
            mesh.SetIndices(refarenceMesh.GetIndices(0), MeshTopology.Triangles, 0);
            mesh.SetNormals(refarenceMesh.normals);
            mesh.SetUVs(0, refarenceMesh.uv);
            meshFilter.mesh = mesh;
        }

        public void Cut(IList<Vector2> vertices, IList<Vector2> uvs)
        {
            var verticesArray = vertices.ToArray();
            var piece = new Triangulator().CreateInfluencePolygon(verticesArray);
            piece.tag = "PaperPiece";
            piece.layer = LayerMask.NameToLayer("Piece");
            piece.GetComponent<Renderer>().material = refarenceRenderer.material;
            var meshFilter = piece.GetComponent<MeshFilter>();
            var collider = piece.AddComponent<MeshCollider>();
            var rigidbody = piece.AddComponent<Rigidbody>();
            rigidbody.transform.Translate(Vector3.up * 0.3f);

            Mesh newMesh = meshFilter.mesh;
            Vector3[] newMeshVertices = newMesh.vertices;
            Vector2[] rangedUvs = new Vector2[newMeshVertices.Length];
            for (int i = 0, il = newMeshVertices.Length; i < il; i++)
            {
                int index = Array.IndexOf(verticesArray, new Vector2(newMeshVertices[i].x, newMeshVertices[i].z));
                rangedUvs[i] = index >= 0 ? uvs[index] : Vector2.zero;
            }
            newMesh.SetUVs(0, rangedUvs);

            var sum = Vector3.zero;
            foreach (var v in newMeshVertices)
            {
                sum += v;
            }
            var mean = sum / newMeshVertices.Length;
            var shifted = newMeshVertices.Select(v => v - mean).ToArray();
            newMesh.vertices = shifted;
            rigidbody.transform.Translate(mean);
            collider.sharedMesh = meshFilter.mesh;
            collider.convex = true;

        }
    }
}