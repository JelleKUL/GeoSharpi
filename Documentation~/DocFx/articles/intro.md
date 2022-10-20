# Getting Started

## Functionality

All resources are stored as an abstract dataclass `Node()`, containing its metadata and resource (image, mesh,...).


### Data Capture

This package contains functions to capture and save Images and meshes from either in game, or real life using XR devices.

#### Capture Session

Datacapture is managed by the `CaptureSessionManager`. This script contains the nescecary functions to save and store different resources.

### Data Reconstruction

An big part of this package is the ability to display RDF based Sessions and Nodes.

All nodes can be visualised using the `NodeVisualizer()` 


### Server Connection

The whole `CaptureSession` can be send to a webserver using a post request. It is up to the server to interpret the data.
If the server responds with a calculated location, the App will store it as a reference.

## Extension

### Node Extension
Custom Nodes can be created by inheriting from `Node`.

### Serialising Variables
Extra variables that should be serialised should be decorated with `[RDFUri(prefix,uri,dataType)]`

### Example

```cs
using GeoSharpi;

[System.Serializable]
public class CustomNode : Node
{
    [RDFUri(prefix,uri,dataType)]
    public var newVar;

    // The constructor can contain extra custom logic, but should use the base constructor.
    public CustomNode(string _graphPath = "", string _subject = "")
    {
        CreateNode(_graphPath, _subject);
    }
}
``` 