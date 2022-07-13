using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.Linq;
using System;
using System.Reflection;

namespace GeoSharpi
{

    public class CaptureSessionManager : MonoBehaviour
    {
        public CaptureSession assetSession;
        [SerializeField]
        private string savePath = "";
        [SerializeField]
        [Tooltip("The url of the post request destination")]
        private string dataPostUrl = "";

        public string graphLoadPath = "";

        public List<Node> differentNodes = new List<Node>();

        [ContextMenu("Save Graph")]
        public RDFGraph SaveGraph()
        {
            return assetSession.UpdateGraph();
        }

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

                    differentNodes.Add(item);
                }
            }
        }

        [ContextMenu("Find types in graph")]
        public void LogAllGraphTypes()
        {
            RDFGraph graph = RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, graphLoadPath);

            RDFGraph predicateGraph = graph.SelectTriplesByPredicate(new RDFResource("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));

            List<RDFTriple> predicateTriples = new List<RDFTriple>();

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
                            differentNodes.Add(newNode);
                            newNode.FromGraph(graph, new RDFResource(triplesEnum.Current.Subject.ToString()));
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    if (type.Contains("Node"))
                    {
                        Debug.Log(triplesEnum.Current.Subject + " is a custom type or generic Node, will be parsed as a Node");
                        Node newNode = new Node();
                        differentNodes.Add(newNode);
                        newNode.FromGraph(graph, new RDFResource(triplesEnum.Current.Subject.ToString()));
                        found = true;
                    }
                    
                }

            }

            
        }
        
        /// <summary>
        /// Adds a node to the current Session
        /// </summary>
        /// <param name="node"></param>
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

        //Load a session from an rdf path
        public void LoadSession(string path)
        {

        }

        /// <summary>
        /// Sends the current session to the specified server
        /// </summary>
        public void SendSessionToServer()
        {
            if (assetSession == null)
            {
                Debug.Log("there is no active session, nothing to send to the server");
                return;
            }

            // create a datastream to send the data to the server
            //StartCoroutine(UploadSession());
        }
        /*
        IEnumerator UploadSession()
        {
            // todo compress the folder to send all the data at once
            // for now we will send the json file as a test

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection(assetSession.GetJsonString()));

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
        */

    }

}