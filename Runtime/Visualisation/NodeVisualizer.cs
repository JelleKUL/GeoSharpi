using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeoSharpi.Nodes;
using GeoSharpi.Utils;

namespace GeoSharpi.Visualisation
{
    /// <summary>
    /// A class to visualise any type of node
    /// </summary>
    public class NodeVisualizer : MonoBehaviour
    {
        [SerializeReference]
        public Node node;

        /// <summary>
        /// The constructor to instantiate the node in the scene
        /// </summary>
        /// <param name="_node">the node to instansiate</param>
        /// <param name="parentTransform">The parent of the transform, defaults to this gameobject</param>
        public void SetUpNode(Node _node, Transform parentTransform = null)
        {
            node = _node;
            name = node.GetName();
            GameObject nodeResource = node.GetResourceObject(); //add the resource as a child of this gameobject
            if (nodeResource)
                nodeResource.transform.SetParent(transform); // set the parent of the resource to match this relative transform
            else Debug.Log("No object found for " + name + "...");
            transform.SetParent(parentTransform); // Parent this transform to the parenttransform

            // Set the local transform to match the Node
            Matrix4x4 transformMatrix = node.cartesianTransform;
            if (!transformMatrix.ValidTRS())
            {
                if (transformMatrix.transpose.ValidTRS()) transformMatrix = transformMatrix.transpose;
            }

            if (transformMatrix.ValidTRS())
            {
                transform.localPosition = transformMatrix.ExtractPosition();
                transform.localRotation = transformMatrix.ExtractRotation();
                transform.localScale = transformMatrix.ExtractScale();
            }
            else Debug.Log("No valid TRS" + transformMatrix);
        }

        /// <summary>
        /// Resets the current Node
        /// </summary>
        [ContextMenu("Reset Node")]
        public void ResetNode()
        {
            node = new Node();
        }

        
    }
}


