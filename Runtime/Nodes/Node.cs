using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.Reflection;
using System.Linq;
using System;
using System.IO;

namespace GeoSharpi
{
    /// <summary>
    /// The Base node object that provides data to a resource
    /// </summary>
    [System.Serializable]
    public class Node
    {
        [Tooltip("The Identifier of the resource")]
        public string subject = "";
        [Tooltip("The path path of the desired Graph of the resource")]
        public string graphPath = "";


        [Header("RDF Variables")]

        [Tooltip("The transform of the resource")]
        [RDFUri("v4d","https://w3id.org/v4d/core#")]
        public Matrix4x4 cartesianTransform = new Matrix4x4();

        [Tooltip("The path to the resource, saved on disk as relative, in memory as absolute")]
        [RDFUri("v4d","https://w3id.org/v4d/core#", RDFModelEnums.RDFDatatypes.XSD_STRING)]
        public string path = "";

        [Tooltip("The moment the Asset was created")]
        [RDFUri("exif", "http://www.w3.org/2003/12/exif/ns#", RDFModelEnums.RDFDatatypes.XSD_DATETIME)]
        public string dateTime = "";


        private RDFGraph graph;

        /// Node can be created in 3 different ways:
        /// 1: A new Instance 
        /// 2: Parsed from a Graph
        /// 3: Parsed from a GraphPath

        public Node(string _graphPath = "", string _subject = "")
        {
            CreateNode(_graphPath, _subject);
        }

        //todo add more file formats
        protected void CreateNode(string _graphPath = "", string _subject = "")
        {
            if(graphPath != "")
            {
                graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle,graphPath);
            }

            if (_subject != "") subject = _subject;

            CreateEmptyNode();


        }

        private void CreateEmptyNode()
        {
            graph = new RDFGraph();
            dateTime = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            // add the subject Type
            
            if (subject == null || subject == "")
            {
                Debug.LogWarning("No subject defined! Creating a new one");
                subject = this.GetType().Name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ff");
            }
        }

         
        RDFPlainLiteral GetClass()
        {
            return new RDFPlainLiteral("v4d:" + this.GetType().Name);
        }

        public RDFResource GetSubject()
        {
            Uri uriResult;
            bool result = Uri.TryCreate(subject, UriKind.Absolute, out uriResult);
            if (!result)
            {
                Uri.TryCreate("file://" + subject, UriKind.Absolute, out uriResult);
                Debug.LogWarning("Created valid URI: " + uriResult + "\n This is a Hack to add a uri format to the subject");
                subject = uriResult.ToString();
            }
            
            RDFResource res = new RDFResource(subject);
            return res;
        }

        public string GetName()
        {
            string sub = GetSubject().ToString();
            if (sub.EndsWith("/")) sub = sub.Remove(sub.Length - 1);
            
            return Path.GetFileNameWithoutExtension(sub);
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
                    if (att.type != RDFModelEnums.RDFDatatypes.RDFS_LITERAL) {
                        newTriple = new RDFTriple(GetSubject(), new RDFResource(att.uri + field.Name), new RDFTypedLiteral(field.GetValue(this).ToString(), att.type));
                    }
                    else
                        newTriple = new RDFTriple(GetSubject(), new RDFResource(att.uri + field.Name), new RDFPlainLiteral(field.GetValue(this).ToString()));
                    RDFNamespaceRegister.AddNamespace(new RDFNamespace(att.prefix, att.uri));
                    graph.AddTriple(newTriple);
                    Debug.Log("Added Tripple: " + field.Name);
                }
            }

            Debug.Log(graph);

            return graph;

        }

        public virtual GameObject GetResourceObject()
        {
            return null;
        }

        

    }


}
