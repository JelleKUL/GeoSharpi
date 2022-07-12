using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GeoSharpi
{
    public class BaseMeshingController : MonoBehaviour
    {
        [SerializeField]
        private bool spawnInScene = false;
        
        public NodeEvent OnMeshCaptured = new NodeEvent();
        
        [ContextMenu("Save Mesh")]
        public void SaveMesh()
        {
            CreateNode();
        }

        public void CreateNode(Transform pos = null)
        {
            Mesh mesh = GetCurrentMesh();
            
            GeometryNode newGeometry = new GeometryNode(
                mesh,
                pos ? pos.localToWorldMatrix : transform.localToWorldMatrix
                ) ;
            OnMeshCaptured.Invoke(newGeometry);

            if (spawnInScene)
            {
                GameObject nodeVisualiser = new GameObject();
                NodeVisualizer nodeVis = nodeVisualiser.AddComponent<NodeVisualizer>();
                nodeVis.SetUpNode(newGeometry);
            }
        }

        /// <summary>
        /// Get the current mesh belonging to this object
        /// </summary>
        /// <returns></returns>
        public Mesh GetCurrentMesh()
        {
            if (TryGetComponent(out MeshFilter mf))
            {
                return mf.mesh;
            }
            Debug.LogWarning("No MeshFilter Attatched");
            return null;
        }
    }
}

