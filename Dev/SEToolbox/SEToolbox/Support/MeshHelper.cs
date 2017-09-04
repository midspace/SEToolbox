namespace SEToolbox.Support
{
    using System;
    using System.IO;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;

    using HelixToolkit.Wpf;

    public static class MeshHelper
    {
        public static Model3DGroup Load(string path, Dispatcher dispatcher = null, bool freeze = false, bool ignoreErrors = false)
        {
            if (path == null)
            {
                return null;
            }

            var defaultMaterial = Materials.Blue;

            if (dispatcher == null)
            {
                dispatcher = Dispatcher.CurrentDispatcher;
            }

            Model3DGroup model;
            var ext = Path.GetExtension(path).ToLower();

            switch (ext)
            {
                case ".3ds":
                    {
                        var r = new StudioReader(dispatcher) { DefaultMaterial = defaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".lwo":
                    {
                        var r = new LwoReader(dispatcher) { DefaultMaterial = defaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".obj":
                    {
                        var r = new ObjReader(dispatcher) { DefaultMaterial = defaultMaterial, Freeze = freeze, IgnoreErrors = ignoreErrors };
                        model = r.Read(path);
                        break;
                    }

                case ".objx":
                    {
                        var r = new ObjReader(dispatcher) { DefaultMaterial = defaultMaterial, Freeze = freeze, IgnoreErrors = ignoreErrors };
                        model = r.ReadZ(path);
                        break;
                    }

                case ".stl":
                    {
                        var r = new StLReader(dispatcher) { DefaultMaterial = defaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".off":
                    {
                        var r = new OffReader(dispatcher) { DefaultMaterial = defaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                default:
                    throw new InvalidOperationException("File format not supported.");
            }

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
                var g = (MeshGeometry3D)gm.Geometry;

                for (var i = 0; i < g.Positions.Count; i++)
                {
                    g.Positions[i] = new Point3D(g.Positions[i].X * scaleX, g.Positions[i].Y * scaleY, g.Positions[i].Z * scaleZ);
                }

                if (g.Normals != null)
                {
                    for (var i = 0; i < g.Normals.Count; i++)
                    {
                        g.Normals[i] = new Vector3D(g.Normals[i].X * scaleX, g.Normals[i].Y * scaleY, g.Normals[i].Z * scaleZ);
                        g.Normals[i].Normalize();
                    }
                }
            }
        }

        public static bool RayIntersetTriangle(Point3D p1, Point3D p2, Point3D p3, Point3D r1, Point3D r2, out Point3D intersection, out int norm)
        {
            // http://gamedev.stackexchange.com/questions/5585/line-triangle-intersection-last-bits

            intersection = default(Point3D);
            norm = 0;

            // Find Triangle Normal
            var normal = Vector3D.CrossProduct(p2 - p1, p3 - p1);
            normal.Normalize();

            // not a triangle. Two or more points may occupy the same place.
            if (normal.IsUndefined())
            {
                return false;
            }

            // Find distance from LP1 and LP2 to the plane defined by the triangle
            var dist1 = Vector3D.DotProduct(r1 - p1, normal);
            var dist2 = Vector3D.DotProduct(r2 - p1, normal);

            if ((dist1 * dist2) >= 0.0f)
            {
                // line doesn't cross the triangle.
                return false;
            }

            if (dist1 == dist2)
            {
                // line and plane are parallel
                return false;
            }

            // Find point on the line that intersects with the plane.
            var intersectPos = r1 + (r2 - r1) * (-dist1 / (dist2 - dist1));

            // Alternate calculation, but slower.
            //var intersectPos = r1 + (Vector3D.DotProduct(normal, p1 - r1) / Vector3D.DotProduct(normal, r2 - r1)) * (r2 - r1);

            // Find if the interesection point lies inside the triangle by testing it against all edges
            var vTest = Vector3D.CrossProduct(normal, p2 - p1);
            var dTest = Vector3D.DotProduct(vTest, intersectPos - p1);
            if (dTest < 0.0f)
            {
                // no intersect P2-P1
                return false;
            }

            vTest = Vector3D.CrossProduct(normal, p3 - p2);
            dTest = Vector3D.DotProduct(vTest, intersectPos - p2);
            if (dTest < 0.0f)
            {
                // no intersect P3-P2
                return false;
            }

            vTest = Vector3D.CrossProduct(normal, p1 - p3);
            dTest = Vector3D.DotProduct(vTest, intersectPos - p3);
            if (dTest < 0.0f)
            {
                // no intersect P1-P3
                return false;
            }

            // Determine if Normal is facing towards or away from Ray.
            norm = Math.Sign(Vector3D.DotProduct(r2 - r1, normal));

            intersection = intersectPos;

            return true;
        }

        public static bool RayIntersetTriangleRound(Point3D p1, Point3D p2, Point3D p3, Point3D[] rays, out Point3D intersection, out int norm)
        {
            for (var i = 0; i < rays.Length; i += 2)
            {
                if (RayIntersetTriangleRound(p1, p2, p3, rays[i], rays[i + 1], out intersection, out norm)) // Ray
                    return true;
                if (RayIntersetTriangleRound(p1, p2, p3, rays[i + 1], rays[i], out intersection, out norm)) // Reverse Ray
                    return true;
            }
            intersection = default(Point3D);
            norm = 0;
            return false;
        }

        public static bool RayIntersetTriangleRound(Point3D p1, Point3D p2, Point3D p3, Point3D r1, Point3D r2, out Point3D intersection, out int norm)
        {
            // http://gamedev.stackexchange.com/questions/5585/line-triangle-intersection-last-bits

            intersection = default(Point3D);
            norm = 0;
            const int rounding = 14;

            // Find Triangle Normal
            var normal = CrossProductRound(Round(p2 - p1, rounding), Round(p3 - p1, rounding));
            normal.Normalize();

            // not a triangle. Two or more points may occupy the same place.
            if (normal.IsUndefined())
            {
                return false;
            }

            // Find distance from LP1 and LP2 to the plane defined by the triangle
            var dist1 = DotProductRound(Round(r1 - p1, rounding), normal);
            var dist2 = DotProductRound(Round(r2 - p1, rounding), normal);

            if ((dist1 * dist2) >= 0.0f)
            {
                // line doesn't cross the triangle.
                return false;
            }

            if (dist1 == dist2)
            {
                // ray line and plane are parallel
                return false;
            }

            // Find point on the line that intersects with the plane
            // Rouding to correct for anonymous rounding issues! Damn doubles!
            var intersectPos = r1 + (r2 - r1) * Math.Round(-dist1 / Math.Round(dist2 - dist1, rounding), rounding);

            // Find if the interesection point lies inside the triangle by testing it against all edges

            var vTest = Vector3D.CrossProduct(normal, p2 - p1);
            //var vTest = CrossProductRound(normal, Round(p2 - p1, rounding));
            //if (DotProductRound(vTest, Round(intersectPos - p1, rounding)) < 0.0f)
            if (Math.Round(Vector3D.DotProduct(vTest, intersectPos - p1), 12) < 0.0f)
            {
                // No intersection on edge P2-P1.
                return false;
            }

            vTest = Vector3D.CrossProduct(normal, p3 - p2);
            //vTest = CrossProductRound(normal, Round(p3 - p2, rounding));
            //if (DotProductRound(vTest, Round(intersectPos - p2, rounding)) < 0.0f)
            if (Math.Round(Vector3D.DotProduct(vTest, intersectPos - p2), 12) < 0.0f)
            {
                // No intersection on edge P3-P2.
                return false;
            }

            vTest = Vector3D.CrossProduct(normal, p1 - p3);
            //vTest =  CrossProductRound(normal, Round(p1 - p3, rounding));
            //if (DotProductRound(vTest, Round(intersectPos - p1, rounding)) < 0.0f)
            if (Math.Round(Vector3D.DotProduct(vTest, intersectPos - p1), 12) < 0.0f)
            {
                // No intersection on edge P1-P3.
                return false;
            }

            // Determine if Normal is facing towards or away from Ray.
            norm = Math.Sign(DotProductRound(r2 - r1, normal));

            intersection = intersectPos;

            return true;
        }

        public static Point3D Floor(this Point3D point)
        {
            return new Point3D(Math.Floor(point.X), Math.Floor(point.Y), Math.Floor(point.Z));
        }

        public static Point3D Ceiling(this Point3D point)
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
            return Math.Round(Math.Round(Math.Round(vector1.X * vector2.X, 14) + Math.Round(vector1.Y * vector2.Y, 14), 14) + Math.Round(vector1.Z * vector2.Z, 14), 13);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="roll"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static Transform3D TransformVector(Vector3D origin, double roll, double yaw, double pitch)
        {
            var transform = new Transform3DGroup();
            transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), yaw))); // y angle
            transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, -1, 0), roll))); // z angle
            transform.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(-1, 0, 0), pitch))); // x angle
            transform.Children.Add(new TranslateTransform3D(origin));
            return transform;
        }

        public static Point3D Min(Point3D point1, Point3D point2, Point3D point3)
        {
            return new Point3D(Math.Min(Math.Min(point1.X, point2.X), point3.X), Math.Min(Math.Min(point1.Y, point2.Y), point3.Y), Math.Min(Math.Min(point1.Z, point2.Z), point3.Z));
        }

        public static Point3D Max(Point3D point1, Point3D point2, Point3D point3)
        {
            return new Point3D(Math.Max(Math.Max(point1.X, point2.X), point3.X), Math.Max(Math.Max(point1.Y, point2.Y), point3.Y), Math.Max(Math.Max(point1.Z, point2.Z), point3.Z));
        }
    }
}
