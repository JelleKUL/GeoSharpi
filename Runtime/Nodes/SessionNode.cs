using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

namespace GeoSharpi
{
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

    }
}

