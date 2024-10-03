using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GeoSharpi.Nodes;

namespace GeoSharpi.Utils.Events
{
    /// <summary>
    /// An event which passes a Geosharpi Node
    /// </summary>
    [System.Serializable]
    public class IntEvent : UnityEvent<int>
    {
    }
    
    /// <summary>
    /// An event which passes a Geosharpi Node
    /// </summary>
    [System.Serializable]
    public class NodeEvent : UnityEvent<Node>
    {
    }

    /// <summary>
    /// An event which passes a Geosharpi ImageNode
    /// </summary>
    [System.Serializable]
    public class ImageNodeEvent : UnityEvent<ImageNode>
    {
    }

    /// <summary>
    /// An event which passes a Geosharpi GeometryNode
    /// </summary>
    [System.Serializable]
    public class GeometryNodeEvent : UnityEvent<GeometryNode>
    {
    }

    /// <summary>
    /// An event which passes a Geosharpi SessionNode
    /// </summary>
    [System.Serializable]
    public class SessionNodeEvent : UnityEvent<SessionNode>
    {
    }
}
