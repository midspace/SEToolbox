namespace SEToolbox.Interop.Models
{
    using System.Linq;
    using SEToolbox.ImageLibrary.Effects;
    using SEToolbox.Interop;
    using SEToolbox.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using VRage.Import;
    using VRageRender.Import;

    //public static class MyModelConverter
    //{
    //    private const string C_POSTFIX_DIFFUSE = "_d";
    //    internal const string C_POSTFIX_DIFFUSE_EMISSIVE = "_de";
    //    private const string C_POSTFIX_DONT_HAVE_NORMAL = "_dn";
    //    internal const string C_POSTFIX_NORMAL_SPECULAR = "_ns";
    //    internal const string C_POSTFIX_MASK_EMISSIVE = "_me";

    //    #region LoadModel3d

    //    public static Model3DGroup LoadModel3d(string filename)
    //    {
    //        //SpaceEngineersCore.LoadStockDefinitions();
    //        Model3DGroup myModel = new Model3DGroup();

    //        // SE .fbx models appear to have a X rotation of -90, and yet I cannot find anything in the .mwm file to indicate this.
    //        var transform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), -90, -90, 0);
    //        myModel.Transform = transform;

    //        LoadModel3d(myModel, filename);

    //        return myModel;
    //    }

    //    private static void LoadModel3d(Model3DGroup myModel, string filename)
    //    {
    //        var contentPath = ToolboxUpdater.GetApplicationContentPath();

    //        var tagData = MyModel.LoadCustomModelData(filename);
    //        //var model = new MyModelImporter();
    //        //model.ImportData(filename);

    //        Dictionary<string, MyModelDummy> dummies = null;
    //        VRageMath.PackedVector.Byte4[] normals = null;
    //        VRageMath.PackedVector.HalfVector2[] texCoords0 = null;
    //        //VRageMath.PackedVector.Byte4[] binormals = null;
    //        //VRageMath.PackedVector.Byte4[] tangents = null;
    //        //VRageMath.PackedVector.HalfVector2[] texCoords1 = null;
    //        VRageMath.PackedVector.HalfVector4[] vertices = null;
    //        List<MyMeshPartInfo> meshParts = null;
    //        //float specularShininess = 0;
    //        float specularPower = 0;
    //        //MyModelInfo modelInfo = null;

    //        foreach (var kvp in tagData)
    //        {
    //            if (kvp.Key == MyImporterConstants.TAG_DUMMIES) dummies = (Dictionary<string, MyModelDummy>)kvp.Value;
    //            if (kvp.Key == MyImporterConstants.TAG_VERTICES) vertices = (VRageMath.PackedVector.HalfVector4[])kvp.Value;
    //            if (kvp.Key == MyImporterConstants.TAG_MESH_PARTS) meshParts = (List<MyMeshPartInfo>)kvp.Value;
    //            //if (kvp.Key == MyImporterConstants.TAG_MODEL_INFO) modelInfo = (MyModelInfo)kvp.Value;
    //            //if (kvp.Key == MyImporterConstants.TAG_SPECULAR_SHININESS) specularShininess = (float)kvp.Value;
    //            if (kvp.Key == MyImporterConstants.TAG_SPECULAR_POWER) specularPower = (float)kvp.Value;
    //            if (kvp.Key == MyImporterConstants.TAG_NORMALS) normals = (VRageMath.PackedVector.Byte4[])kvp.Value;
    //            if (kvp.Key == MyImporterConstants.TAG_TEXCOORDS0) texCoords0 = (VRageMath.PackedVector.HalfVector2[])kvp.Value;
    //            //if (kvp.Key == MyImporterConstants.TAG_BINORMALS) binormals = (VRageMath.PackedVector.Byte4[])kvp.Value;
    //            //if (kvp.Key == MyImporterConstants.TAG_TANGENTS) tangents = (VRageMath.PackedVector.Byte4[])kvp.Value;
    //            //if (kvp.Key == MyImporterConstants.TAG_TEXCOORDS1) texCoords1 = (VRageMath.PackedVector.HalfVector2[])kvp.Value;
    //        }

    //        foreach (var meshPart in meshParts)
    //        {
    //            GeometryModel3D geomentryModel = new GeometryModel3D();
    //            myModel.Children.Add(geomentryModel);

    //            MeshGeometry3D meshGeometery = new MeshGeometry3D();
    //            geomentryModel.Geometry = meshGeometery;

    //            #region mesh parts

    //            meshGeometery.Normals = new Vector3DCollection();
    //            meshGeometery.Positions = new Point3DCollection();
    //            meshGeometery.TextureCoordinates = new PointCollection();
    //            meshGeometery.TriangleIndices = new Int32Collection();

    //            for (var i = 0; i < normals.Length; i++)
    //            {
    //                meshGeometery.Normals.Add(VF_Packer.UnpackNormal(ref normals[i]).ToVector3D());
    //            }

    //            foreach (var vertex in vertices)
    //            {
    //                var vector = vertex.ToVector4();
    //                meshGeometery.Positions.Add(new Point3D(vector.W * vector.X, vector.W * vector.Y, vector.W * vector.Z));
    //            }

    //            // http://blogs.msdn.com/b/danlehen/archive/2005/11/06/489627.aspx
    //            //var texTransform = MeshHelper.TransformVector(new Vector3D(0, 0, 0), -90, 90, 0);
    //            //Transform="scale(1,-1) translate(0,1)"
    //            foreach (var vertex in texCoords0)
    //            {
    //                meshGeometery.TextureCoordinates.Add(vertex.ToVector2().ToPoint());
    //                //var point = vertex.ToVector2().ToPoint();
    //                //meshGeometery.TextureCoordinates.Add(new Point(point.X, ((point.Y + 0) * +1) + 0));
    //            }

    //            //foreach (var index in meshPart.m_indices)
    //            //{
    //            //    meshGeometery.TriangleIndices.Add(index);
    //            //}

    //            // Flip the indicides so the textures are on the Outside, not the inside.
    //            for (var i = 0; i < meshPart.m_indices.Count; i += 3)
    //            {
    //                try
    //                {
    //                    meshGeometery.TriangleIndices.Add(meshPart.m_indices[i + 2]);
    //                    meshGeometery.TriangleIndices.Add(meshPart.m_indices[i + 1]);
    //                    meshGeometery.TriangleIndices.Add(meshPart.m_indices[i + 0]);
    //                }
    //                catch
    //                {
    //                    break;
    //                }
    //            }

    //            #endregion


    //            if (meshPart.m_MaterialDesc != null)
    //            {
    //                #region filenames

    //                var diffuseTextureName = meshPart.m_MaterialDesc.Textures.FirstOrDefault().Value;

    //                var directoryName = Path.GetDirectoryName(diffuseTextureName);
    //                var textureName = Path.GetFileNameWithoutExtension(diffuseTextureName);
    //                var textureExt = Path.GetExtension(diffuseTextureName);

    //                if (textureName.LastIndexOf(C_POSTFIX_DIFFUSE) == (textureName.Length - 2))
    //                {
    //                    textureName = textureName.Substring(0, textureName.Length - 2);
    //                }

    //                if (textureName.LastIndexOf(C_POSTFIX_DIFFUSE_EMISSIVE) == (textureName.Length - 3))
    //                {
    //                    textureName = textureName.Substring(0, textureName.Length - 3);
    //                }

    //                if (textureName.LastIndexOf(C_POSTFIX_MASK_EMISSIVE) == (textureName.Length - 3))
    //                {
    //                    textureName = textureName.Substring(0, textureName.Length - 3);
    //                }
    //                var maskTextureFile = Path.Combine(Path.Combine(contentPath, directoryName), textureName + C_POSTFIX_MASK_EMISSIVE + textureExt);
    //                var diffuseTextureFile = Path.Combine(Path.Combine(contentPath, directoryName), textureName + C_POSTFIX_DIFFUSE_EMISSIVE + textureExt);
    //                var specularTextureFile = Path.Combine(Path.Combine(contentPath, directoryName), textureName + C_POSTFIX_NORMAL_SPECULAR + textureExt);

    //                #endregion

    //                // Jan.Nekvapil:
    //                // HSV in color mask isnt absolute value, as you can see ingame in Color Picker, Hue is absolute and Saturation with Value are just offsets to texture values.

    //                var ambient = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
    //                var diffuseColor = meshPart.m_MaterialDesc.DiffuseColor.ToSandboxMediaColor();
    //                //var specular = meshPart.m_MaterialDesc.SpecularColor.ToSandboxMediaColor();
    //                var diffuseBrush = new SolidColorBrush(diffuseColor);
    //                var colorBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));

    //                // method in memory source
    //                //var defaultImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(Path.Combine(contentPath, meshPart.m_MaterialDesc.DiffuseTextureName));
    //                //var defaultImageBrush = defaultImage == null ? null : new ImageBrush(defaultImage) { ViewportUnits = BrushMappingMode.Absolute };

    //                // method file
    //                //var imageBitmap = SEToolbox.ImageLibrary.ImageTextureUtil.CreateBitmap(textureFile);
    //                //var tempFile = Path.Combine(@"C:\temp", Path.GetFileNameWithoutExtension(textureFile) + ".png");
    //                //ImageTextureUtil.WriteImage(imageBitmap, tempFile);
    //                //var imageBrush = new ImageBrush(new BitmapImage(new Uri(tempFile, UriKind.Absolute)));

    //                //MaterialGroup myBackMatGroup = new MaterialGroup();
    //                //myBackMatGroup.Children.Add(new DiffuseMaterial() { AmbientColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Brush = diffuseImageBrush, Color = diffuseColor });
    //                //myBackMatGroup.Children.Add(new SpecularMaterial() { Brush = maskImageBrush, Color = specular, SpecularPower = specularPower });
    //                //geomentryModel.BackMaterial = myBackMatGroup;

    //                MaterialGroup myMatGroup = new MaterialGroup();
    //                if (File.Exists(maskTextureFile))
    //                {
    //                    var cubeColor = Color.FromArgb(255, 159, 68, 68);

    //                    //var diffuseTextureImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(maskTextureFile, false);
    //                    //var diffuseImageBrush = diffuseTextureImage == null ? null : new ImageBrush(diffuseTextureImage) { ViewportUnits = BrushMappingMode.Absolute };
    //                    //myMatGroup.Children.Add(new DiffuseMaterial() { Brush = diffuseImageBrush, Color = diffuseColor, AmbientColor = ambient });

    //                    myMatGroup.Children.Add(new DiffuseMaterial() { Brush = new SolidColorBrush(cubeColor), Color = cubeColor, AmbientColor = ambient });

    //                    var maskTextureImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(maskTextureFile, false, new MaskPixelEffect());
    //                    var maskImageBrush = maskTextureImage == null ? null : new ImageBrush(maskTextureImage) { ViewportUnits = BrushMappingMode.Absolute };

    //                    myMatGroup.Children.Add(new DiffuseMaterial() { Brush = maskImageBrush, Color = diffuseColor, AmbientColor = ambient });
    //                    //myMatGroup.Children.Add(new EmissiveMaterial() { Brush = maskImageBrush, Color = diffuseColor });
    //                    //myMatGroup.Children.Add(new SpecularMaterial() { Brush = specularImageBrush, Color = specular, SpecularPower = specularPower });


    //                    var emissiveTextureImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(maskTextureFile, false, new EmissivePixelEffect(0));
    //                    var emissiveImageBrush = emissiveTextureImage == null ? null : new ImageBrush(emissiveTextureImage) { ViewportUnits = BrushMappingMode.Absolute };
    //                    myMatGroup.Children.Add(new EmissiveMaterial() { Brush = emissiveImageBrush, Color = diffuseColor });
    //                }
    //                else
    //                {
    //                    var diffuseTextureImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(diffuseTextureFile, true);
    //                    var diffuseImageBrush = diffuseTextureImage == null ? null : new ImageBrush(diffuseTextureImage) { ViewportUnits = BrushMappingMode.Absolute };
    //                    myMatGroup.Children.Add(new DiffuseMaterial() { Brush = diffuseImageBrush, Color = diffuseColor, AmbientColor = ambient });

    //                    var emissiveTextureImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(diffuseTextureFile, false, new EmissivePixelEffect(0));
    //                    var emissiveImageBrush = emissiveTextureImage == null ? null : new ImageBrush(emissiveTextureImage) { ViewportUnits = BrushMappingMode.Absolute };
    //                    //myMatGroup.Children.Add(new EmissiveMaterial() { Brush = emissiveImageBrush, Color = diffuseColor });
    //                    myMatGroup.Children.Add(new EmissiveMaterial() { Brush = emissiveImageBrush, Color = Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF) });
    //                }

    //                if (File.Exists(specularTextureFile))
    //                {

    //                    //new NormalMaterial() Normal maps?

    //                    var specularTextureImage = SEToolbox.ImageLibrary.ImageTextureUtil.CreateImage(specularTextureFile);
    //                    var specularImageBrush = specularTextureImage == null ? null : new ImageBrush(specularTextureImage) { ViewportUnits = BrushMappingMode.Absolute };
    //                    myMatGroup.Children.Add(new SpecularMaterial() { Brush = specularImageBrush, SpecularPower = specularPower });
    //                }

    //                geomentryModel.Material = myMatGroup;
    //            }
    //            else
    //            {
    //                // No material assigned.
    //                MaterialGroup myMatGroup = new MaterialGroup();
    //                var cubeColor = Color.FromArgb(255, 159, 68, 68);
    //                var ambient = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
    //                var diffuseColor = Color.FromArgb(255, 159, 68, 68);
    //                var colorBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xFF, 0x00));
                 
    //                myMatGroup.Children.Add(new DiffuseMaterial() { Brush = new SolidColorBrush(cubeColor), Color = cubeColor, AmbientColor = ambient });
    //                myMatGroup.Children.Add(new DiffuseMaterial() { Brush = colorBrush, Color = diffuseColor, AmbientColor = ambient });
    //                geomentryModel.Material = myMatGroup;
    //            }


    //            foreach (var dummy in dummies)
    //            {
    //                foreach (var data in dummy.Value.CustomData)
    //                {
    //                    if (data.Key == "file")
    //                    {
    //                        var newfilename = Path.Combine(Path.GetDirectoryName(filename), data.Value + ".mwm");
    //                        LoadModel3d(myModel, newfilename);
    //                    }
    //                }
    //            }

    //        }
    //    }

    //    #endregion

    //    private static Brush GetImageBrush(string filename)
    //    {
    //        var bmpImg = new BitmapImage(new Uri(filename, UriKind.Absolute));
    //        return new ImageBrush(bmpImg);
    //    }
    //}
}
