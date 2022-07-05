using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;

namespace GeoSharpi
{
    public class SessionNode : Node
    {
        public SessionNode(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        public override RDFPlainLiteral GetClass()
        {
            return new RDFPlainLiteral("v4d:SessionNode");
        }
    }
}

