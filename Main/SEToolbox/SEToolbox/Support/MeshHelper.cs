namespace SEToolbox.Support
{
    using HelixToolkit.Wpf;
    using System;
    using System.IO;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using VRageMath;

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

        // WIP.
        public static bool RayIntersetTriangle(Point3D p1, Point3D p2, Point3D p3, Point3D r1, Point3D r2, out Point3D intersection)
        {
            // http://gamedev.stackexchange.com/questions/5585/line-triangle-intersection-last-bits

            intersection = default(Point3D);

            // Find Triangle Normal
            var normal = Vector3D.CrossProduct(p2 - p1, p3 - p1);

            // Find distance from LP1 and LP2 to the plane defined by the triangle
            var dist1 = Vector3D.DotProduct(r1 - p1, normal);
            var dist2 = Vector3D.DotProduct(r2 - p1, normal);

            if ((dist1 * dist2) >= 0.0f)
            {
                //SFLog(@"no cross"); 
                return false;
            } // line doesn't cross the triangle.

            if (dist1 == dist2)
            {
                //SFLog(@"parallel"); 
                return false;
            } // line and plane are parallel

            // Find point on the line that intersects with the plane
            // Rouding to correct for anonymous rounding issues! Damn doubles!
            var intersectPos = r1 + (r2 - r1) * (-dist1 / (dist2 - dist1));

            // Find if the interesection point lies inside the triangle by testing it against all edges
            var vTest = Vector3D.CrossProduct(normal, p2 - p1);
            if (Vector3D.DotProduct(vTest, intersectPos - p1) < 0.0f)
            {
                //SFLog(@"no intersect P2-P1"); 
                return false;
            }

            vTest = Vector3D.CrossProduct(normal, p3 - p2);
            if (Vector3D.DotProduct(vTest, intersectPos - p2) < 0.0f)
            {
                //SFLog(@"no intersect P3-P2"); 
                return false;
            }

            vTest = Vector3D.CrossProduct(normal, p1 - p3);
            if (Vector3D.DotProduct(vTest, intersectPos - p1) < 0.0f)
            {
                //SFLog(@"no intersect P1-P3"); 
                return false;
            }

            intersection = intersectPos;

            return true;
        }

        public static bool RayIntersetTriangleRound(Point3D p1, Point3D p2, Point3D p3, Point3D r1, Point3D r2, out Point3D intersection)
        {
            // http://gamedev.stackexchange.com/questions/5585/line-triangle-intersection-last-bits

            intersection = default(Point3D);
            const int rounding = 14;

            // Find Triangle Normal
            var normal = CrossProductRound(Round(p2 - p1, rounding), Round(p3 - p1, rounding));

            // Find distance from LP1 and LP2 to the plane defined by the triangle
            var dist1 = DotProductRound(Round(r1 - p1, rounding), normal);
            var dist2 = DotProductRound(Round(r2 - p1, rounding), normal);

            if ((dist1 * dist2) >= 0.0f)
            {
                //SFLog(@"no cross"); 
                return false;
            } // line doesn't cross the triangle.

            if (dist1 == dist2)
            {
                //SFLog(@"parallel"); 
                return false;
            } // line and plane are parallel

            // Find point on the line that intersects with the plane
            // Rouding to correct for anonymous rounding issues! Damn doubles!
            var intersectPos = r1 + (r2 - r1) * Math.Round(-dist1 / Math.Round(dist2 - dist1, rounding), rounding);

            // Find if the interesection point lies inside the triangle by testing it against all edges
            var vTest = CrossProductRound(normal, Round(p2 - p1, rounding));
            if (DotProductRound(vTest, Round(intersectPos - p1, rounding)) < 0.0f)
            {
                //SFLog(@"no intersect P2-P1"); 
                return false;
            }

            vTest = CrossProductRound(normal, Round(p3 - p2, rounding));
            if (DotProductRound(vTest, Round(intersectPos - p2, rounding)) < 0.0f)
            {
                //SFLog(@"no intersect P3-P2"); 
                return false;
            }

            vTest = CrossProductRound(normal, Round(p1 - p3, rounding));
            if (DotProductRound(vTest, Round(intersectPos - p1, rounding)) < 0.0f)
            {
                //SFLog(@"no intersect P1-P3"); 
                return false;
            }

            intersection = intersectPos;

            return true;
        }

        internal static Point3D Min(Point3D point1, Point3D point2, Point3D point3)
        {
            return new Point3D(Math.Min(Math.Min(point1.X, point2.X), point3.X), Math.Min(Math.Min(point1.Y, point2.Y), point3.Y), Math.Min(Math.Min(point1.Z, point2.Z), point3.Z));
        }

        internal static Point3D Max(Point3D point1, Point3D point2, Point3D point3)
        {
            return new Point3D(Math.Max(Math.Max(point1.X, point2.X), point3.X), Math.Max(Math.Max(point1.Y, point2.Y), point3.Y), Math.Max(Math.Max(point1.Z, point2.Z), point3.Z));
        }

        internal static Point3D Floor(this Point3D point)
        {
            return new Point3D(Math.Floor(point.X), Math.Floor(point.Y), Math.Floor(point.Z));
        }

        internal static Point3D Ceiling(this Point3D point)
        {
            return new Point3D(Math.Ceiling(point.X), Math.Ceiling(point.Y), Math.Ceiling(point.Z));
        }

        internal static Vector3D Round(this Vector3D vector)
        {
            return new Vector3D(Math.Round(vector.X), Math.Round(vector.Y), Math.Round(vector.Z));
        }

        internal static Vector3D Round(this Vector3D vector, int places)
        {
            return new Vector3D(Math.Round(vector.X, places), Math.Round(vector.Y, places), Math.Round(vector.Z, places));
        }

        internal static Point3D Round(this Point3D point)
        {
            return new Point3D(Math.Round(point.X), Math.Round(point.Y), Math.Round(point.Z));
        }

        internal static Point3D Round(this Point3D point, int places)
        {
            return new Point3D(Math.Round(point.X, places), Math.Round(point.Y, places), Math.Round(point.Z, places));
        }

        internal static Vector3D CrossProductRound(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D
            {
                X = Math.Round(Math.Round(vector1.Y * vector2.Z, 14) - Math.Round(vector1.Z * vector2.Y, 14), 14),
                Y = Math.Round(Math.Round(vector1.Z * vector2.X, 14) - Math.Round(vector1.X * vector2.Z, 14), 14),
                Z = Math.Round(Math.Round(vector1.X * vector2.Y, 14) - Math.Round(vector1.Y * vector2.X, 14), 14)
            };
        }

        internal static double DotProductRound(Vector3D vector1, Vector3D vector2)
        {
            return Math.Round(Math.Round(Math.Round(vector1.X * vector2.X, 14) + Math.Round(vector1.Y * vector2.Y, 14), 14) + Math.Round(vector1.Z * vector2.Z, 14), 14);
        }

        internal static Vector3I Mirror(this Vector3I vector, Mirror xMirror, int xAxis, Mirror yMirror, int yAxis, Mirror zMirror, int zAxis)
        {
            var newVector = new Vector3I(vector.X, vector.Y, vector.Z);
            switch (xMirror)
            {
                case Support.Mirror.Odd: newVector.X = xAxis - (vector.X - xAxis); break;
                case Support.Mirror.EvenUp: newVector.X = xAxis - (vector.X - xAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.X = xAxis - (vector.X - xAxis) - 1; break;
            }
            switch (yMirror)
            {
                case Support.Mirror.Odd: newVector.Y = yAxis - (vector.Y - yAxis); break;
                case Support.Mirror.EvenUp: newVector.Y = yAxis - (vector.Y - yAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.Y = yAxis - (vector.Y - yAxis) - 1; break;
            }
            switch (zMirror)
            {
                case Support.Mirror.Odd: newVector.Z = zAxis - (vector.Z - zAxis); break;
                case Support.Mirror.EvenUp: newVector.Z = zAxis - (vector.Z - zAxis) + 1; break;
                case Support.Mirror.EvenDown: newVector.Z = zAxis - (vector.Z - zAxis) - 1; break;
            }
            return newVector;
        }

    }
}
