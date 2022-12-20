# Data Capture

This package contains functions to capture and save Images and meshes from either in game, or real life using XR devices.

## Capture Session

Datacapture is managed by the `CaptureSessionManager`. This script contains the nescecary functions to save and store different resources.

## Server Connection

The whole `CaptureSession` can be send to a webserver using a post request. It is up to the server to interpret the data.
If the server responds with a calculated location, the App will store it as a reference.
