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
        public SessionNode sessionNode;
        public List<Node> nodes = new List<Node>();

        public int imageQuality = 75;

        private string sessionPath = "";

        /// <summary>
        /// The instantiator for a new session
        /// automatically creates a new folder with the current timestamp.
        /// </summary>
        public CaptureSession(string path, Matrix4x4 origin)
        {
            sessionNode = new SessionNode();
            if (path == "") path = Application.persistentDataPath;
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

        public void AddNode(Node node)
        {
            nodes.Add(node);
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
