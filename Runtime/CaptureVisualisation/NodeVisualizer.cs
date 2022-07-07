using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoSharpi
{
    public class NodeVisualizer : MonoBehaviour
    {
        public Node node;

        public void SetUpNode(Node _node, Transform parentTransform)
        {
            node = _node;
            GameObject nodeResource = node.GetResourceObject(); //add the resource as a child of this gameobject
            nodeResource.transform.SetParent(transform); // set the parent of the resource to match this relative transform
            transform.SetParent(parentTransform); // Parent this transform to the parenttransform

            // Set the local transform to match the Node
            Matrix4x4 transformMatrix = node.cartesianTransform;
            if (transformMatrix.ValidTRS())
            {
                transform.localPosition = transformMatrix.ExtractPosition();
                transform.localRotation = transformMatrix.ExtractRotation();
                transform.localScale    = transformMatrix.ExtractScale();
            }
        }

        
    }
}


