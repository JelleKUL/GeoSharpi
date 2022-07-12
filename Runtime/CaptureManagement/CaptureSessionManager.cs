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
        private CaptureSession assetSession;
        [SerializeField]
        private string savePath = "";
        [SerializeField]
        [Tooltip("The url of the post request destination")]
        private string dataPostUrl = "";

        [ContextMenu("Save Graph")]
        public RDFGraph SaveGraph()
        {
            return assetSession.UpdateGraph();
        }

        [ContextMenu("Log All Node Types")]
        public void GetAllNodeTypes()
        {
            
            IEnumerable<Node> exporters = 
                Assembly.GetAssembly(typeof(Node)).GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Node)) && !t.IsAbstract)
                .Select(t => (Node)Activator.CreateInstance(t));

            foreach (var item in exporters)
            {
                Debug.Log(item);
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