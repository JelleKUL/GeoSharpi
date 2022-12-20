# Extending the nodes

If you want to work with more Node types or fields

## Node Extension
Custom Nodes can be created by inheriting from `Node`.


## Serializing Fields/properties
Extra fields that should be serialised should be decorated with `[RDFUri(prefix,uri,dataType)]`
The datatypes need to be of type: `RDFModelEnums.RDFDatatypes` from the `RDFSharp.Model` namespace

## Example

```cs
using GeoSharpi.Nodes;
using RDFSharp.Model;

[System.Serializable]
public class CustomNode : Node
{
    // regular fields can be set with the RDFUri attribute
    [RDFUri(prefix,uri,dataType)]
    public var newVar;

    // Prpoerties need to specify the field before they can be set with the RDFUri attribute
    [field: RDFUri(prefix,uri,dataType)]
    [field: SerializeField] //this also counts for making it show up in the inspector
    public var newVar {get; private set;};

    // The constructor can contain extra custom logic, but should use the base constructor.
    public CustomNode(string _graphPath = "", string _subject = "")
    {
        CreateNode(_graphPath, _subject);

        // custom logic here
    }

    // These functions should be oerriden to provide custom saving and loading functionality
    public override GameObject GetResourceObject(){}
    public override void LoadResource(string path){}
    public override void SaveResource(string path){}
}
``` 