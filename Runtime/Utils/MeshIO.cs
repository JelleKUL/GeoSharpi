using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

namespace GeoSharpi.Utils
{
	/// <summary>
	/// Methods for saving and loading obj Meshes
	/// </summary>
	public class MeshIO
	{
		/// <summary>
		/// Save a mesh to a file
		/// </summary>
		/// <param name="mesh">the mesh to save</param>
		/// <param name="meshPath">the path of the file, include the file name and extension</param>
		/// <returns>the succes of the save</returns>
		public static bool SaveMesh(Mesh mesh, string meshPath)
        {
			using (StreamWriter sw = new StreamWriter(meshPath))
			{
				sw.Write(MeshToString(mesh));
			}
			return true;
		}

		/// <summary>
		/// Loads a mesh as a gameobject from a file
		/// </summary>
		/// <param name="path">the location of the mesh as an absolute path</param>
		/// <returns>the Textured mesh as a GameObject</returns>
		public static GameObject LoadMesh(string path)
        {
			return MeshImporter.MeshImporter.Load(path);
        }


		/// <summary>
		/// Generates an obj string from a mesh
		/// This class is a modified version of the one found at the UnifyCommunity wiki.
		/// It provides utilities for exporting a mesh to a.obj file
		/// author KeliHlodversson(see http://unifycommunity.com/wiki/index.php?title=ObjExporter)
		/// </summary>
		/// <param name="m">the mesh</param>
		/// <returns>a obj formatted as a string</returns>
		public static string MeshToString(Mesh m)
		{

			StringBuilder sb = new StringBuilder();

			sb.Append("g ").Append(m.name).Append("\n");
			foreach (Vector3 v in m.vertices)
			{
				sb.Append(string.Format("v {0} {1} {2}\n", -v.x, v.y, v.z)); // mirror around the x-axis
			}
			sb.Append("\n");
			foreach (Vector3 v in m.normals)
			{
				sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
			}
			sb.Append("\n");
			foreach (Vector3 v in m.uv)
			{
				sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
			}
			for (int material = 0; material < m.subMeshCount; material++)
			{
				sb.Append("\n");

				int[] triangles = m.GetTriangles(material);
				for (int i = 0; i < triangles.Length; i += 3)
				{
					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
						triangles[i+2] + 1, triangles[i + 1] + 1, triangles[i ] + 1)); //reverse direction to flip the face direction
				}
			}
			return sb.ToString();
		}
	}
}