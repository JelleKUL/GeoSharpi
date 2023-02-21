using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.Linq;
using System;
using System.Reflection;
using System.IO;
using GeoSharpi.Nodes;
using GeoSharpi.Visualisation;
using UnityEngine.Networking;

namespace GeoSharpi.Capture
{
    /// <summary>
    /// Manages the creation of CaptureSessions
    /// </summary>
    public class CaptureSessionManager : MonoBehaviour
    {
        public CaptureSession assetSession = null;

        [Header("Session Creation")]
        [SerializeField]
        [Tooltip("The absolute folder path to save the sessin to, leave emty to use persistant datapath")]
        private string savePath = "";
        [SerializeField]
        [Tooltip("The url of the post request destination")]
        private string dataPostUrl = "";

        [Header("Session Reconstruction")]
        [Tooltip("The location of the .ttl RDF grapg file")]
        [TextArea]
        public string graphLoadPath = "";
        [Tooltip("Use the linked subjects to only add the referenced nodes in the sesison to the list")]
        public bool useLinkedSubjects = true;

        private void Start()
        {
            //CreateNewSession();
            assetSession = null;
        }

        /// <summary>
        /// Saves the Graph
        /// </summary>
        /// <returns>The Updated RDF Graph</returns>
        [ContextMenu("Save Graph")]
        public RDFGraph SaveGraph()
        {
            return assetSession.UpdateGraph();
        }

        /// <summary>
        /// Logs all the Node types, including the Extended ones
        /// </summary>
        [ContextMenu("Log All Node Types")]
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
                    Debug.Log(item.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Load a graph from the graphloadPath
        /// </summary>
        [ContextMenu("Parse RDF Graph from LoadPath")]
        public void LoadGraph()
        {
            LoadSession(graphLoadPath);
        }
        

        /// <summary>
        /// Adds a node to the current Session
        /// </summary>
        /// <param name="node">The Node to add</param>
        public void AddNode(Node node)
        {
            CheckSession();
            assetSession.AddNode(node);
        }

        /// <summary>
        /// Creates a new session to store the data
        /// </summary>
        public void CreateNewSession()
        {
            assetSession = new CaptureSession(savePath, Matrix4x4.identity);
        }

        /// <summary>
        /// Checks if there is an existing session. If not, creates a new session
        /// </summary>
        void CheckSession()
        {
            Debug.Log("Checking session");
            if (assetSession == null) CreateNewSession();
        }

        /// <summary>
        /// Load a session from an rdf path
        /// </summary>
        /// <param name="path">The Path to load from</param>
        public void LoadSession(string path)
        {
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, path);
            RDFGraph predicateGraph = graph.SelectTriplesByPredicate(new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
            List<RDFTriple> predicateTriples = new List<RDFTriple>();
            List<Node> newNodes = new List<Node>();
            assetSession.sessionPath = Path.GetDirectoryName(path);

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
                            Debug.Log(triplesEnum.Current.Subject + " is of type: " + item.GetType());
                            Node newNode = (Node)Activator.CreateInstance(item.GetType());
                            if (newNode.GetType() == typeof(SessionNode)) assetSession.sessionNode = newNode as SessionNode;
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
                        Debug.Log(triplesEnum.Current.Subject + " is a custom type or generic Node with type: "+ type + ", it will be parsed as a Node");
                        Node newNode = new Node();
                        newNodes.Add(newNode);
                        newNode.FromGraph(graph, new RDFResource(triplesEnum.Current.Subject.ToString()));
                        found = true;
                    }
                    else Debug.Log(triplesEnum.Current.Subject + " is a not a Node type and will be skipped");

                }

            }
            assetSession.nodes = new List<Node>();
            if (useLinkedSubjects)
            {
                foreach (var item in newNodes)
                {
                    if (assetSession.sessionNode.linkedSubjects.Contains(item.GetSubject().ToString())) assetSession.nodes.Add(item);
                }
            }
            else assetSession.nodes = newNodes;
        }

        /// <summary>
        /// spawn the current Capturesession in the scene
        /// </summary>
        [ContextMenu("Visualise session")]
        public void VisualiseSession()
        {
            GameObject nodeVisualiser = new GameObject();
            NodeVisualizer nodeVis = nodeVisualiser.AddComponent<NodeVisualizer>();
            nodeVis.SetUpNode(assetSession.sessionNode);

            foreach (Node node in assetSession.nodes)
            {
                GameObject newNodeVisualiser = new GameObject();
                NodeVisualizer newNodeVis = newNodeVisualiser.AddComponent<NodeVisualizer>();
                node.LoadResource(assetSession.sessionPath);
                newNodeVis.SetUpNode(node, nodeVisualiser.transform);
            }
        }
        /// <summary>
        /// Sends the current session to the specified server
        /// </summary>
        [ContextMenu("Send Session to server")]
        public void SendSessionToServer()
        {
            if (assetSession == null)
            {
                Debug.Log("there is no active session, nothing to send to the server");
                return;
            }

            // create a datastream to send the data to the server
            StartCoroutine(UploadSession());
        }
        
        IEnumerator UploadSession()
        {
            WWWForm formData = new WWWForm();
            formData.AddField("session", assetSession.sessionNode.GetName());
            var info = new DirectoryInfo(assetSession.sessionPath);
            FileInfo[] fileInfo = info.GetFiles();
            foreach (FileInfo file in fileInfo)
            {
                formData.AddBinaryData("file", File.ReadAllBytes(file.FullName), file.Name);
            }

            UnityWebRequest www = UnityWebRequest.Post(dataPostUrl, formData);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("file upload succes");
            }

        }
        

    }

}