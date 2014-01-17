namespace SEToolbox.Support
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using HelixToolkit.Wpf;

    public static class MeshHelper
    {
        public static Model3DGroup Load(string path, Dispatcher dispatcher = null, bool freeze = false, bool IgnoreErrors = false)
        {
            Material DefaultMaterial = Materials.Blue;

            if (path == null)
            {
                return null;
            }

            if (dispatcher == null)
            {
                dispatcher = Dispatcher.CurrentDispatcher;
            }

            Model3DGroup model;
            var ext = Path.GetExtension(path);
            if (ext != null)
            {
                ext = ext.ToLower();
            }

            switch (ext)
            {
                case ".3ds":
                    {
                        var r = new StudioReader(dispatcher) { DefaultMaterial = DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".lwo":
                    {
                        var r = new LwoReader(dispatcher) { DefaultMaterial = DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);

                        break;
                    }

                case ".obj":
                    {
                        var r = new ObjReader(dispatcher) { DefaultMaterial = DefaultMaterial, Freeze = freeze, IgnoreErrors = IgnoreErrors };
                        model = r.Read(path);
                        break;
                    }

                case ".objz":
                    {
                        var r = new ObjReader(dispatcher) { DefaultMaterial = DefaultMaterial, Freeze = freeze, IgnoreErrors = IgnoreErrors };
                        model = r.ReadZ(path);
                        break;
                    }

                case ".stl":
                    {
                        var r = new StLReader(dispatcher) { DefaultMaterial = DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".off":
                    {
                        var r = new OffReader(dispatcher) { DefaultMaterial = DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                default:
                    throw new InvalidOperationException("File format not supported.");
            }

            //if (!freeze)
            //{
            //    dispatcher.Invoke(new Action(() => model.SetName(Path.GetFileName(path))));
            //}

            return model;
        }

        public static void TransformScale(this Model3DGroup model, double scale)
        {
            TransformScale(model, scale, scale, scale);
        }

        public static void TransformScale(this Model3DGroup model, double scaleX, double scaleY, double scaleZ)
        {
            foreach (GeometryModel3D gm in model.Children)
            {
                var g = gm.Geometry as MeshGeometry3D;

                for (int i = 0; i < g.Positions.Count; i++)
                {
                    g.Positions[i] = new Point3D(g.Positions[i].X * scaleX, g.Positions[i].Y * scaleY, g.Positions[i].Z * scaleZ);
                }

                if (g.Normals != null)
                {
                    for (int i = 0; i < g.Normals.Count; i++)
                    {
                        g.Normals[i] = new Vector3D(g.Normals[i].X * scaleX, g.Normals[i].Y * scaleY, g.Normals[i].Z * scaleZ);
                        g.Normals[i].Normalize();
                    }
                }
            }
        }

        public static bool RayIntersetTriangle(Point3D p1, Point3D p2, Point3D p3, Point3D r1, Point3D r2, out Point3D intersection)
        {
            // http://gamedev.stackexchange.com/questions/5585/line-triangle-intersection-last-bits

            Vector3D Normal;
            Point3D IntersectPos;

            intersection = default(Point3D);

            // Find Triangle Normal

            var poly = new Polygon3D(new List<Point3D>() { p1, p2, p3 });

            Normal = poly.GetNormal();
            //Normal.cross( P2 - P1, P3 - P1 );
            //Normal.normalize(); // not really needed?  Vector3f does this with cross.

            // Find distance from LP1 and LP2 to the plane defined by the triangle

            double Dist1 = Vector3D.DotProduct(r1 - p1, Normal);
            double Dist2 = Vector3D.DotProduct(r2 - p1, Normal);

            if ((Dist1 * Dist2) >= 0.0f)
            {
                //SFLog(@"no cross"); 
                return false;
            } // line doesn't cross the triangle.

            if (Dist1 == Dist2)
            {
                //SFLog(@"parallel"); 
                return false;
            } // line and plane are parallel

            // Find point on the line that intersects with the plane
            IntersectPos = r1 + (r2 - r1) * (-Dist1 / (Dist2 - Dist1));

            // Find if the interesection point lies inside the triangle by testing it against all edges
            Vector3D vTest;


            vTest = Vector3D.CrossProduct(Normal, p2 - p1);
            if (Vector3D.DotProduct(vTest, IntersectPos - p1) < 0.0f)
            {
                //SFLog(@"no intersect P2-P1"); 
                return false;
            }

            vTest = Vector3D.CrossProduct(Normal, p3 - p2);
            if (Vector3D.DotProduct(vTest, IntersectPos - p2) < 0.0f)
            {
                //SFLog(@"no intersect P3-P2"); 
                return false;
            }

            vTest = Vector3D.CrossProduct(Normal, p1 - p3);
            if (Vector3D.DotProduct(vTest, IntersectPos - p1) < 0.0f)
            {
                //SFLog(@"no intersect P1-P3"); 
                return false;
            }

            intersection = IntersectPos;

            return true;
        }

        public static Point3D Min(Point3D point1, Point3D point2, Point3D point3)
        {
            return new Point3D(Math.Min(Math.Min(point1.X, point2.X), point3.X), Math.Min(Math.Min(point1.Y, point2.Y), point3.Y), Math.Min(Math.Min(point1.Z, point2.Z), point3.Z));
        }

        public static Point3D Max(Point3D point1, Point3D point2, Point3D point3)
        {
            return new Point3D(Math.Max(Math.Max(point1.X, point2.X), point3.X), Math.Max(Math.Max(point1.Y, point2.Y), point3.Y), Math.Max(Math.Max(point1.Z, point2.Z), point3.Z));
        }

        public static Point3D Floor(this Point3D point)
        {
            return new Point3D(Math.Floor(point.X), Math.Floor(point.Y), Math.Floor(point.Z));
        }

        public static Point3D Ceiling(this Point3D point)
        {
            return new Point3D(Math.Ceiling(point.X), Math.Ceiling(point.Y), Math.Ceiling(point.Z));
        }

        public static Point3D Round(this Point3D point)
        {
            return new Point3D(Math.Round(point.X), Math.Round(point.Y), Math.Round(point.Z));
        }
    }
}
