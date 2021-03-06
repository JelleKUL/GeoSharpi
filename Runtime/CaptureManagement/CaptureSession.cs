using System;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.IO;
using System.Linq;

namespace GeoSharpi
{
    [System.Serializable]
    public class CaptureSession
    {
        public SessionNode sessionNode = null;
        public List<Node> nodes = new List<Node>();

        public int imageQuality = 75;

        public string sessionPath = "";

        /// <summary>
        /// The instantiator for a new session
        /// automatically creates a new folder with the current timestamp.
        /// </summary>
        public CaptureSession(string path, Matrix4x4 origin)
        {
            sessionNode = new SessionNode();
            sessionNode.cartesianTransform = origin;
            if (path == "" || path == null) path = Application.persistentDataPath;
            sessionPath = Path.Combine(path, sessionNode.GetName() + Path.DirectorySeparatorChar);
            Directory.CreateDirectory(sessionPath);
            Debug.Log("Created a new Session @ " + sessionPath);

        }

        public CaptureSession(string RDFPath)
        {
            RDFGraph graph = new RDFGraph();

            IEnumerable <Node> exporters = typeof(Node)
                .Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Node)) && !t.IsAbstract)
                .Select(t => (Node)Activator.CreateInstance(t));
        }

        public Node ParseRDFNode(string subject, RDFGraph graph)
        {
            //step 1: get the type of the subject

            RDFGraph predicateGraph = graph.SelectTriplesByPredicate(new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));

            List<RDFTriple> predicateTriples = new List<RDFTriple>();

            var triplesEnum = predicateGraph.TriplesEnumerator;
            while (triplesEnum.MoveNext())
            {
                string type = triplesEnum.Current.Object.ToString();

            }

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                IEnumerable<Node> exporters =
                ass.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Node)) && !t.IsAbstract)
                .Select(t => (Node)Activator.CreateInstance(t));

                foreach (var item in exporters)
                {
                    Debug.Log(item);
                    //item.GetType().Name;
                }
            }



            return null;
        }

        public void GetAllNodeTypes()
        {

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {

                IEnumerable<Node> exporters =
                ass.GetTypes()
                //ass.GetAssembly(typeof(Node)).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Node)) && !t.IsAbstract)
                .Select(t => (Node)Activator.CreateInstance(t));

                foreach (var item in exporters)
                {
                    Debug.Log(item);
                }
            }
        }

        public void AddNode(Node node)
        {
            nodes.Add(node);
            sessionNode.AddSubject(node);
            node.SaveResource(sessionPath);
            UpdateGraph();
        }



        public RDFGraph UpdateGraph()
        {
            if (nodes.Count == 0) return null;

            RDFGraph newGraph = new RDFGraph();
            string graphName = sessionNode.GetName();
            newGraph = newGraph.UnionWith(sessionNode.ToGraph());

            foreach (var node in nodes)
            {
                newGraph = newGraph.UnionWith(node.ToGraph());
                Debug.Log("Node Name: " + node.GetName());
            }
            newGraph.ToFile(RDFModelEnums.RDFFormats.Turtle, Path.Combine(sessionPath,graphName + ".ttl"));
            Debug.Log("Saved the TTL File to: " + Path.Combine(sessionPath, graphName + ".ttl"));

            return newGraph;
        }

        [ContextMenu("Log & Save Graph")]
        public void LogGraph()
        {
            var triplesEnum = UpdateGraph().TriplesEnumerator;
            while (triplesEnum.MoveNext())
            {
                Debug.Log("Subject: " + triplesEnum.Current.Subject);
                string pred = triplesEnum.Current.Predicate.ToString();
                Debug.Log("Predicate: " + pred);
                Debug.Log("Object: " + triplesEnum.Current.Object);
            }
        }



    }



}
