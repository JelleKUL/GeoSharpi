using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.Reflection;
using System.Linq;

namespace GeoSharpi
{
    /// <summary>
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
        [Header("RDF Variables")]
        [Tooltip("The transform of the resource")]
        [RDFUri("v4d","https://w3id.org/v4d/core#")]
        public Matrix4x4 cartesianTransform = new Matrix4x4();
        [Tooltip("The path to the resource, saved on disk as relative, in memory as absolute")]
        [RDFUri("v4d","https://w3id.org/v4d/core#", RDFModelEnums.RDFDatatypes.XSD_STRING)]
        public string path;
        [Tooltip("The moment the Asset was created")]
        [RDFUri("exif", "http://www.w3.org/2003/12/exif/ns#", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
        public string dateTime = "";


        public Node(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        protected void CreateNode(string _graphPath = "", string _subject = "")
        {
            graph = new RDFGraph();
            dateTime = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            // add the subject Type
            if (_subject != "") subject = _subject;
            if (subject == null || subject == "")
            {
                Debug.LogWarning("No subject defined! Creating a new one");
                subject = "Node-" + dateTime;
            }
            
            /*
            graph.AddTriple(new RDFTriple(
                GetSubject(), 
                new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                GetClass()
                )
            );
            */
        }

         
        RDFPlainLiteral GetClass()
        {
            return new RDFPlainLiteral("v4d:" + this.GetType().Name);
        }

        RDFResource GetSubject()
        {
            return new RDFResource(subject);
        }

         

        public RDFGraph ToGraph()
        {
            //if (graph == null) 
            CreateNode();

            graph.AddTriple(new RDFTriple(
                GetSubject(),
                new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"),
                new RDFPlainLiteral("GeoSharpi:" + this.GetType().Name)
                )
            );

            foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var att = field.GetCustomAttributes(typeof(RDFUriAttribute), true).FirstOrDefault() as RDFUriAttribute;
                if (att == null)
                {
                    Debug.Log($"The field {field.Name} will not Be Seriazed");
                }
                else
                {
                    Debug.Log($"The field {field.Name} will be serialized with namespace: {att.uri}");
                    RDFTriple newTriple;
                    if (att.type != RDFModelEnums.RDFDatatypes.RDFS_LITERAL)
                        newTriple = new RDFTriple(GetSubject(), new RDFResource(att.uri + field.Name), new RDFTypedLiteral(field.GetValue(this).ToString(), att.type));
                    else
                        newTriple = new RDFTriple(GetSubject(), new RDFResource(att.uri + field.Name), new RDFPlainLiteral(field.GetValue(this).ToString()));
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
