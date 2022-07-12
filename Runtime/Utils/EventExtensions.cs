using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GeoSharpi
{
    [System.Serializable]
    public class IntEvent : UnityEvent<int>
    {
    }

    [System.Serializable]
    public class StringEvent : UnityEvent<string>
    {
    }

    [System.Serializable]
    public class NodeEvent : UnityEvent<Node>
    {
    }

    [System.Serializable]
    public class ImageNodeEvent : UnityEvent<ImageNode>
    {
    }

    [System.Serializable]
    public class GeometryNodeNodeEvent : UnityEvent<GeometryNode>
    {
    }
}
