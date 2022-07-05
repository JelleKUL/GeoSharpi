# GeoSharpi
Tools to collect sensory data from XR devices in the Unity engine
> This is still under active development and future versions might introduce breaking changes

```cs
 namespace GeoSharpi
```

## ToDo

- [ ] Solve wrong placement of image planes
- [ ] Add server connectivity to send data
- [ ] think of more todo's

## Installation

This can be imported as a UnityPackage in any existing Unity project through the [Package manager](https://docs.unity3d.com/Manual/Packages.html) with the Git url.

## Structure

All resources are stored as a `Node()`, containing its metadata.

### Data Capture

This package contains functions to capture and save Images and meshes from either in game, or real life using XR devices.

#### Capture Session

Datacapture is managed by the `CaptureSessionManager`. This script contains the nescecary functions to save and store different resources.

### Data Reconstruction

An big part of this package is the ability to display RDF based Sessions and Nodes.

## Server Connection

The whole `CaptureSession` can be send to a webserver using a post request. It is up to the server to interpret the data.
If the server responds with a calculated location, the App will store it as a reference.

## Licensing

The code in this project is licensed under MIT license.
