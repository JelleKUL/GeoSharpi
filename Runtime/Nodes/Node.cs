using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.Reflection;
using System.Linq;

namespace GeoSharpi
{
    /// <summary>
    /// 
    /// The Base node object that provides data to a resource
    /// </summary>
    [System.Serializable]
    public class Node
    {
        [Tooltip("The Graph Containing the resource")]
        public RDFGraph graph;
        [Tooltip("The path path of the desired Graph of the resource")]
        public string graphPath;
        [Tooltip("The Identifier of the resource")]
        public string subject;
        [Tooltip("The transform of the resource")]
        [RDFUri("v4d","https://w3id.org/v4d/core#")]
        public Matrix4x4 cartesianTransform = new Matrix4x4();
        [Tooltip("The path to the resource, saved on disk as relative, in memory as absolute")]
        [RDFUri("v4d","https://w3id.org/v4d/core#")]
        public string path;

        private RDFResource subjectResource;



        public Node(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        protected void CreateNode(string _graphPath = "", string _subject = "")
        {
            graph = new RDFGraph();
            // add the subject Type
            subject = _subject;
            /*
            graph.AddTriple(new RDFTriple(
                GetSubject(), 
                new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                GetClass()
                )
            );
            */
        }

         
        public virtual RDFPlainLiteral GetClass()
        {
            return new RDFPlainLiteral("v4d:Node");
        }

        void FromGraph(RDFGraph graph)
        {

        }

        RDFResource GetSubject()
        {
            return new RDFResource(subject);
        }

         

        public RDFGraph ToGraph()
        {
            if (graph == null) CreateNode();

            graph.AddTriple(new RDFTriple(
                GetSubject(),
                new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                new RDFPlainLiteral("v4d:node")
                )
            );

            foreach (var field in typeof(Node).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var att = field.GetCustomAttributes(typeof(RDFUriAttribute), true).FirstOrDefault() as RDFUriAttribute;
                if (att == null)
                {
                    Debug.Log($"The field {field.Name} will not Be Seriazed");
                }
                else
                {
                    Debug.Log($"The field {field.Name} will be serialized with namespace: {att.uri}");
                    RDFTriple newTriple = new RDFTriple(GetSubject(), new RDFResource(att.uri + field.Name), new RDFTypedLiteral(field.GetValue(this).ToString(), att.type));
                    RDFNamespaceRegister.AddNamespace(new RDFNamespace(att.prefix, att.uri));
                    graph.AddTriple(newTriple);
                }
            }

            Debug.Log(graph);

            return graph;

        }

        [System.Serializable]
        public struct RDFVar
        {
            public RDFResource predicate;
            public string value;
            public RDFModelEnums.RDFDatatypes dataType;
        }
    }


}
