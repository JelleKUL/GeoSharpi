using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using GeoSharpi.Utils;

namespace GeoSharpi.Nodes
{
    /// <summary>
    /// The Node Class for a Session
    /// </summary>
    [System.Serializable]
    public class SessionNode : Node
    {
        [Tooltip("The subjects of the Nodes which are in the same session")]
        [RDFUri("v4d", "https://w3id.org/v4d/core#")]
        public List<string> linkedSubjects = new List<string>();

        public SessionNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        /// <summary>
        /// Add a Node to the Current Session
        /// </summary>
        /// <param name="node"></param>
        public void AddSubject(Node node)
        {
            if (linkedSubjects.Contains(node.GetSubject().ToString())) return;

            linkedSubjects.Add(node.GetSubject().ToString());
        }

        public override GameObject GetResourceObject()
        {
            return null;
        }

    }
}

