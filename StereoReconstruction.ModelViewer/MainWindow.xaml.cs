namespace StereoReconstruction.ModelViewer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using HelixToolkit;
    using Microsoft.Win32;
    
    public partial class MainWindow : Window
    {
        class Vertex
        {
            public double[] Position { get; set; }

            public Vertex(Point3D point)
            {
                Position = new double[3] { point.X, point.Y, point.Z };
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
            d.InitialDirectory = "models";
            d.FileName = null;
            d.Filter = "3D model files (*.3ds;*.obj;*.lwo;*.stl)|*.3ds;*.obj;*.objz;*.lwo;*.stl";
            d.DefaultExt = ".3ds";
            if (!d.ShowDialog().Value)
                return;
            Model3DGroup сurrentModel = ModelImporter.Load(d.FileName);
            if (viewport.Children.Count > 2) viewport.Children.RemoveAt(2);
            viewport.Add(new ModelVisual3D { Content = сurrentModel });
            viewport.ZoomToFit(100);

            var verts = new List<Point3D>();
            MeshGeometry3D mesh = null;
            foreach (var model in сurrentModel.Children)
            {
                if (typeof(GeometryModel3D).IsInstanceOfType(model))
                    if (typeof(MeshGeometry3D).IsInstanceOfType(((GeometryModel3D)model).Geometry))
                    {
                        mesh = (MeshGeometry3D)((GeometryModel3D)model).Geometry;
                        verts.AddRange(mesh.Positions);
                    }
            }
            List<Vertex> vertices = verts.Distinct().Select(p => new Vertex(p)).ToList();
        }
    }
}
