using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace SphereViewWpf.Utils
{
	class GeomTools
	{
		/// <summary>
		/// Calculate (x,y,z) coordinates from given angles and radius
		/// </summary>
		/// <param name="theta">Theta angle</param>
		/// <param name="phi">Phi angle</param>
		/// <param name="radius">Radius</param>
		/// <returns>Coordinates</returns>
		public static Point3D CalcPosition(double theta, double phi, double radius)
		{
			double x = radius * Math.Sin(theta) * Math.Sin(phi);
			double z = radius * Math.Cos(phi);
			double y = radius * Math.Cos(theta) * Math.Sin(phi);

			return new Point3D(x, y, z);
		}

		/// <summary>
		/// Calculate normal from given angles
		/// </summary>
		/// <param name="theta">Theta angle</param>
		/// <param name="phi">Phi angle</param>
		/// <returns></returns>
		public static Vector3D CalcNormal(double theta, double phi)
		{
			return (Vector3D)CalcPosition(theta, phi, 1.0);
		}

		/// <summary>
		/// Convert degrees to radians
		/// </summary>
		/// <param name="degrees">Value in degrees</param>
		/// <returns>Value in radians</returns>
		public static double Deg2Rad(double degrees)
		{
			return (degrees / 180.0) * Math.PI;
		}

		/// <summary>
		/// Calculate texture coordinate from given angles
		/// </summary>
		/// <param name="theta">Theta angle</param>
		/// <param name="phi">Phi angle</param>
		/// <returns></returns>
		public static Point CalcTextureCoord(double theta, double phi)
		{
			Point p = new Point(theta / (2 * Math.PI), phi / (Math.PI));
			return p;
		}

		/// <summary>
		/// Create a tessellated sphere mesh
		/// </summary>
		/// <param name="divTheta">Theta divisions</param>
		/// <param name="divPhi">Phi divisions</param>
		/// <param name="radius">Radius</param>
		/// <returns>Sphere mesh</returns>
		public static MeshGeometry3D CreateSphereMesh(int divTheta, int divPhi, double radius)
		{
			double dt = Deg2Rad(360.0) / divTheta;
			double dp = Deg2Rad(180.0) / divPhi;

			MeshGeometry3D mesh = new MeshGeometry3D();

			// Calculate points with normals and texture coordinates
			for (int pi = 0; pi <= divPhi; pi++)
			{
				double phi = pi * dp;

				for (int ti = 0; ti <= divTheta; ti++)
				{
					double theta = ti * dt;

					mesh.Positions.Add(CalcPosition(theta, phi, radius));
					mesh.Normals.Add(CalcNormal(theta, phi));
					mesh.TextureCoordinates.Add(CalcTextureCoord(theta, phi));
				}
			}

			// Calculate triangles
			for (int pi = 0; pi < divPhi; pi++)
			{
				for (int ti = 0; ti < divTheta; ti++)
				{
					int x0 = ti;
					int x1 = (ti + 1);
					int y0 = pi * (divTheta + 1);
					int y1 = (pi + 1) * (divTheta + 1);

					mesh.TriangleIndices.Add(x0 + y0);
					mesh.TriangleIndices.Add(x0 + y1);
					mesh.TriangleIndices.Add(x1 + y0);

					mesh.TriangleIndices.Add(x1 + y0);
					mesh.TriangleIndices.Add(x0 + y1);
					mesh.TriangleIndices.Add(x1 + y1);
				}
			}

			mesh.Freeze();
			return mesh;
		}
	}

}
