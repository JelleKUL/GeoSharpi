using System;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.IO;
using System.Linq;
using GeoSharpi.Nodes;

namespace GeoSharpi.Capture
{
    /// <summary>
    /// A class to store all the nodes from a single session
    /// </summary>
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
        /// <param name="path">the desired path to save the session to, defaults to persistant datapath</param>
        /// <param name="origin">The origin transform as a Matrix4x4</param>
        public CaptureSession(string path, Matrix4x4 origin)
        {
            sessionNode = new SessionNode();
            sessionNode.cartesianTransform = origin;
            if (path == "" || path == null) path = Application.persistentDataPath;
            sessionPath = Path.Combine(path, sessionNode.GetName() + Path.DirectorySeparatorChar);
            if (!Directory.Exists(sessionPath))
            {
                Directory.CreateDirectory(sessionPath);
                Debug.Log("Created a new Session @ " + sessionPath);
            }
            else Debug.Log("Using Existing Session @ " + sessionPath);

        }

        /// <summary>
        /// Creates a new Session from an RDF file
        /// </summary>
        /// <param name="RDFPath">the path to the RDF File</param>
        /// <remarks>Needs proper implementation</remarks>
        public CaptureSession(string RDFPath, bool useLinkedSubjects = true)
        {

            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, RDFPath);
            RDFGraph predicateGraph = graph.SelectTriplesByPredicate(new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
            List<RDFTriple> predicateTriples = new List<RDFTriple>();
            List<Node> newNodes = new List<Node>();
            sessionPath = Path.GetDirectoryName(RDFPath);

            var triplesEnum = predicateGraph.TriplesEnumerator;
            while (triplesEnum.MoveNext())
            {
                string type = triplesEnum.Current.Object.ToString();
                bool found = false;

                foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    IEnumerable<Node> exporters =
                    ass.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(Node)) && !t.IsAbstract)
                    .Select(t => (Node)Activator.CreateInstance(t));

                    foreach (var item in exporters)
                    {
                        if (type.Contains(item.GetType().Name))
                        {
                            //Debug.Log(triplesEnum.Current.Subject + " is of type: " + item.GetType());
                            Node newNode = (Node)Activator.CreateInstance(item.GetType());
                            if (newNode.GetType() == typeof(SessionNode)) sessionNode = newNode as SessionNode;
                            else newNodes.Add(newNode);
                            newNode.FromGraph(graph, new RDFResource(triplesEnum.Current.Subject.ToString()));
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    if (type.Contains("Node"))
                    {
                        //Debug.Log(triplesEnum.Current.Subject + " is a custom type or generic Node with type: " + type + ", it will be parsed as a Node");
                        Node newNode = new Node();
                        newNodes.Add(newNode);
                        newNode.FromGraph(graph, new RDFResource(triplesEnum.Current.Subject.ToString()));
                        found = true;
                    }
                    else Debug.Log(triplesEnum.Current.Subject + " is a not a Node type and will be skipped");

                }

            }
            nodes = new List<Node>();
            if (useLinkedSubjects)
            {
                foreach (var item in newNodes)
                {
                    if (sessionNode.linkedSubjects.Contains(item.GetSubject().ToString())) nodes.Add(item);
                }
            }
            else nodes = newNodes;
        }


        private Node ParseRDFNode(string subject, RDFGraph graph)
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

        private void GetAllNodeTypes()
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

        /// <summary>
        /// Adds a Node to the Session
        /// </summary>
        /// <param name="node">The Node to add</param>
        public void AddNode(Node node)
        {
            if(nodes == null) nodes = new List<Node>();
            Debug.Log("Adding a new node to the session:" + node.subject);
            nodes.Add(node);
            sessionNode.AddSubject(node);
            node.SaveResource(sessionPath);
            UpdateGraph();
        }


        /// <summary>
        /// Updates and saves the Session to a Graph
        /// </summary>
        /// <returns>The RDF Graph</returns>
        public RDFGraph UpdateGraph()
        {
            if (nodes.Count == 0) return null;

            RDFGraph newGraph = new RDFGraph();
            string graphName = sessionNode.GetName();
            newGraph = newGraph.UnionWith(sessionNode.ToGraph());

            foreach (var node in nodes)
            {
                newGraph = newGraph.UnionWith(node.ToGraph());
                //Debug.Log("Node Name: " + node.GetName());
            }
            newGraph.ToFile(RDFModelEnums.RDFFormats.Turtle, Path.Combine(sessionPath,graphName + ".ttl"));
            Debug.Log("Saved the TTL File to: " + Path.Combine(sessionPath, graphName + ".ttl"));

            return newGraph;
        }

        /// <summary>
        /// Log's and saves the Current Graph
        /// </summary>
        [ContextMenu("Log Graph")]
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
