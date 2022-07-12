using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RDFSharp.Model;
using System.Linq;

namespace GeoSharpi
{
    public class RDFManager : MonoBehaviour
    {
        [SerializeField]
        private string readPath;
        [SerializeField]
        private RDFNameSpacesScriptableObject nameSpaces;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        //reads an rdf file and converts it to a node
        void WriteRDF(string path)
        {
            RDFGraph graph = new RDFGraph();
            RDFResource res = new RDFResource("http://www.jellever.be/duck");
            RDFTriple trip = new RDFTriple(res, res, new RDFTypedLiteral("170", RDFModelEnums.RDFDatatypes.XSD_INTEGER));
            graph.AddTriple(trip);
            graph.ToFile(RDFModelEnums.RDFFormats.Turtle, path);
            Debug.Log("Successful save to: " + path);
        }
        [ContextMenu("ReadRDF")]
        public void ReadCurrentRDF()
        {
            RDFGraph newGraph = ReadRDF(readPath);

            foreach (var nameSpace in nameSpaces.GetNameSpaces())
            {
                RDFNamespaceRegister.AddNamespace(nameSpace);
            }
            var triplesEnum = newGraph.TriplesEnumerator;
            
            while (triplesEnum.MoveNext())
            {
                Debug.Log("Subject: " + triplesEnum.Current.Subject);

                string pred = triplesEnum.Current.Predicate.ToString();
                foreach (var item in nameSpaces.nameSpaces)
                {
                    if (pred.StartsWith(item.uri))
                    {
                        Debug.Log("FullPredicate: " + pred);
                        pred = pred.Replace(item.uri, item.prefix + ":");
                        break;
                    }
                }
                Debug.Log("Predicate: " + pred);
               
                Debug.Log("Object: " + triplesEnum.Current.Object);
            }
            
            List<RDFGraph> graphs = GetSubjectGraphs(newGraph);
            Debug.Log("nr of subjects: " + graphs.Count);
            foreach (var graph in graphs)
            {
                var dataTable = graph.ToDataTable();
                string name = dataTable.Rows[0].ItemArray.ToList()[0].ToString().Replace("http://", "").Replace("/","");
                Debug.Log(name);
                graph.ToFile(RDFModelEnums.RDFFormats.Turtle,"Assets/RDF/" + name + ".ttl");
            }

        }
        public RDFGraph ReadRDF(string path)
        {
            return RDFGraph.FromFile(RDFModelEnums.RDFFormats.Turtle, path);
        }

        public List<RDFGraph> GetSubjectGraphs(RDFGraph mainGraph)
        {
            List<RDFGraph> splitGraphList = new List<RDFGraph>();
            List<string> subjects = new List<string>();

            var triplesEnum = mainGraph.TriplesEnumerator;
            while (triplesEnum.MoveNext())
            {
                string subject = triplesEnum.Current.Subject.ToString();
                int subjectIndex = subjects.IndexOf(subject);
                if (subjectIndex >= 0) //Subject is in the list, add to the corresponding subgraph
                {
                    splitGraphList[subjectIndex].AddTriple(triplesEnum.Current);
                }
                else // the subject is not yet in the list, creating a new entry
                {
                    subjects.Add(subject);
                    splitGraphList.Add(new RDFGraph(new List<RDFTriple>() { triplesEnum.Current }));
                }

            }
            return splitGraphList;
        }
    }
}
