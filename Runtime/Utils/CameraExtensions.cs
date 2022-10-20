using UnityEngine;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// Camera Extensions to get more info from the camera
    /// </summary>
    public static class CameraExtensions
    {
        private static readonly float HalfDiagonalOf35Millimeter = Mathf.Sqrt(36 * 36 + 24 * 24) * 0.5f;

        /// <summary>
        /// Returns the exif standard 35mmequivalent focal length of the camera
        /// </summary>
        /// <param name="camera"></param>
        /// <returns>the 35mm equivalent focal length</returns>
        public static float Get35MillimeterFocalLength(this Camera camera)
        {
            if (camera == null) return 0;

            // camera.fieldOfView is vertical field of view.
            // But meanings changes depending on whether camera.usePhyiscalProperties flag is enabled.
            // if enabled, camera.fieldOfView is cropped camera.sensorSize's vertical fov.
            // if disabled, camera.fieldOfView is entire screen's vertical fov.
            var sensor = camera.usePhysicalProperties ? camera.sensorSize : new Vector2(camera.aspect, 1);
            var sensorDiag = Mathf.Sqrt(sensor.x * sensor.x + sensor.y * sensor.y);
            var verticalFov = camera.fieldOfView * Mathf.Deg2Rad;
            var camToSensor = sensor.y / Mathf.Tan(verticalFov * 0.5f);

            // tan(theta) = 35mmDiag / focalLength = sensorDiag / camToSensor
            // so, focalLength = 35mmDiag * camToSensor / sensorDiag
            return HalfDiagonalOf35Millimeter * camToSensor / sensorDiag;
        }
    }
}
