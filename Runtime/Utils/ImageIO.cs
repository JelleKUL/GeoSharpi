using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeoSharpi.Utils
{
    /// <summary>
    /// Methods to save and load jpegs
    /// </summary>
    public class ImageIO
    {
        //todo add jpg checks
        /// <summary>
        /// Save a texture2D to a file
        /// </summary>
        /// <param name="imageTexture">the texture to be saved</param>
        /// <param name="imagePath">the path of the file, include the file name and extension</param>
        /// <param name="quality">the quality of the jpeg</param>
        /// /// <returns>the succes of the save</returns>
        public static bool SaveImage(Texture2D imageTexture, string imagePath, int quality = 75)
        {
            byte[] imageBytes = imageTexture.EncodeToJPG(quality);
            System.IO.File.WriteAllBytes(imagePath, imageBytes);
            return true;
        }

        /// <summary>
        /// Load an image from the specified path
        /// </summary>
        /// <param name="imagePath">the path of the file, include the file name and extension</param>
        /// <returns>the texture2D to display in the scene</returns>
        public static Texture2D LoadImage(string imagePath)
        {
            Texture2D tex = null;
            byte[] imageBytes;

            if (System.IO.File.Exists(imagePath))
            {
                imageBytes = System.IO.File.ReadAllBytes(imagePath);
                tex = new Texture2D(2, 2);
                tex.LoadImage(imageBytes); //..this will auto-resize the texture dimensions.
            }

            return tex;
        }
    }
}
