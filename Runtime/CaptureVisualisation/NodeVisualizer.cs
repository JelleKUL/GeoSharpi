using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoSharpi
{
    public class NodeVisualizer : MonoBehaviour
    {
        [SerializeReference]
        public Node node;

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

        [ContextMenu("Reset Node")]
        public void ResetNode()
        {
            node = new Node();
        }

        
    }
}


