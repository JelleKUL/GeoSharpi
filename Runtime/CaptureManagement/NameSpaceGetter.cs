using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using RDFSharp.Model;
using RDFSharp.Query;
using System.Net;
using System;


public class NameSpaceGetter : MonoBehaviour
{
    [SerializeField]
    private string getUrl = "http://www.w3.org/2003/12/exif/ns#";
    static readonly HttpClient client = new HttpClient();
    [SerializeField]
    [TextArea]
    private string returnVal = "";
    [SerializeField]

    private RDFNameSpacesScriptableObject extraNameSpaces;

    // Start is called before the first frame update
    void Start()
    {
        FromUri();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    async void LogRequest()
    {
        returnVal = await GetRequest(getUrl);
        Debug.Log(returnVal.ToString());


    }

    async void FromUri()
    {
        System.Uri uri = new System.Uri(getUrl);
        Debug.Log(uri);
        await FromUri(uri);
        Debug.Log("Succes");
    }

    public async Task<string> GetRequest(string uri)
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            Debug.Log("Getting data");
            HttpResponseMessage response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);

            return responseBody;
        }
        catch (HttpRequestException e)
        {
            
            Debug.Log("\nException Caught!");
            Debug.Log("Message : " + e.Message.ToString());
            return null;
        }
    }

    /// <summary>
    /// Reads a graph by trying to dereference the given Uri
    /// </summary>
    public async Task <RDFGraph> FromUri(Uri uri, int timeoutMilliseconds = 20000)
    {
        if (uri == null)
            throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter is null.");
        if (!uri.IsAbsoluteUri)
            throw new RDFModelException("Cannot read RDF graph from Uri because given \"uri\" parameter does not represent an absolute Uri.");

        RDFGraph result = new RDFGraph();
        //try
        //{
            Debug.Log("Trying get");
            HttpWebRequest webRequest = WebRequest.CreateHttp(uri);
            Debug.Log("Converted");
            webRequest.MaximumAutomaticRedirections = 4;
            webRequest.AllowAutoRedirect = true;
            webRequest.Timeout = timeoutMilliseconds;
            //RDF/XML
            //webRequest.Headers.Add(HttpRequestHeader.Accept, "application/rdf+xml");
            
            //TURTLE
            //webRequest.Headers.Add(HttpRequestHeader.Accept, "text/turtle");
            //webRequest.Headers.Add(HttpRequestHeader.Accept, "application/turtle");
            //webRequest.Headers.Add(HttpRequestHeader.Accept, "application/x-turtle");
            //N-TRIPLES
            //webRequest.Headers.Add(HttpRequestHeader.Accept, "application/n-triples");
            //TRIX
            //webRequest.Headers.Add(HttpRequestHeader.Accept, "application/trix");

            Debug.Log("starting webRequest");
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            if (webRequest.HaveResponse)
            {
                Debug.Log("Got response");
                System.IO.Stream stream = webResponse.GetResponseStream();
                Debug.Log(stream);

                //RDF/XML
                if (string.IsNullOrEmpty(webResponse.ContentType) ||
                        webResponse.ContentType.Contains("application/rdf+xml"))
                {
                    result = await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.RdfXml, webResponse.GetResponseStream());
                }
                //TURTLE
                else if (webResponse.ContentType.Contains("text/turtle") ||
                            webResponse.ContentType.Contains("application/turtle") ||
                                webResponse.ContentType.Contains("application/x-turtle"))
                    result = await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.Turtle, webResponse.GetResponseStream());

                //N-TRIPLES
                else if (webResponse.ContentType.Contains("application/n-triples"))
                    result = await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.NTriples, webResponse.GetResponseStream());

                //TRIX
                else if (webResponse.ContentType.Contains("application/trix"))
                    result = await RDFGraph.FromStreamAsync(RDFModelEnums.RDFFormats.TriX, webResponse.GetResponseStream());
                Debug.Log("Got It");
            }
        //}
        //catch (Exception ex)
        //{
        //    throw new RDFModelException($"Cannot read RDF graph from Uri {uri} because: " + ex.Message);
        //}

        return result;
    }

    /// <summary>
    /// Async


}
