using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UntitledJumpGameTesting
{
    public partial class MainForm : Form
    {
        //Set up values
        private int NumSides, WindowWidth, WindowHeight = 0;
        private Point WindowCenter;

        //Simulation Params (Maybe customizable in future?)
        //Platform Radiuses should probably scale with window height like full radius
        private int PlatformMinDiameter = 20;
        private int PlatformMaxDiameter = 50;

        private bool DrawLayout = false;

        /*--- Next Steps ---
         - Figure out way of telling if point is in/out of bounds
         - Create 'Platform' class or struct of something (Just to store center point + radius)
         - Figure out way to get total area (Probably formula exists given list of vertices, maybe
           just split into a bunch of triangles using center point/radius for height?)
         - Figure out different amounts of platforms (low/med/high == 40/50/60 % of total area ???)
         */

        public MainForm()
        {
            InitializeComponent();

            WindowWidth = ClientRectangle.Width;
            WindowHeight = ClientRectangle.Height;
            WindowCenter = new Point(WindowWidth / 2, WindowHeight / 2);
        }

        private List<Point> GetOuterPoints()
        {
            //By making radius 40% of height full shape will take up ~80% of window
            var radius = (WindowHeight / 5) * 2;

            List<Point> points = new List<Point>();

            for (int i = 0; i < NumSides; i++)
            {
                var x = WindowCenter.X + (radius * Math.Cos(2 * Math.PI * i / NumSides));
                var y = WindowCenter.Y + (radius * Math.Sin(2 * Math.PI * i / NumSides));
                points.Add(new Point((int)x, (int)y));
            }

            return points;
        }

        private BoundingBox GetBoundingBox(List<Point> points)
        {
            var boundingBox = new BoundingBox
            {
                MinX = points.OrderBy(p => p.X).Select(p => p.X).First(),
                MaxX = points.OrderByDescending(p => p.X).Select(p => p.X).First(),
                MinY = points.OrderBy(p => p.Y).Select(p => p.Y).First(),
                MaxY = points.OrderByDescending(p => p.Y).Select(p => p.Y).First()
            };

            return boundingBox;
        }

        private bool IsPointInBounds(List<Point> outerPoints, Point point)
        {
            //Maybe Disable BoundingBox logic here since already taken care of during generate
            //var boundingBox = GetBoundingBox(outerPoints);

            //if (point.X < boundingBox.MinX || point.X > boundingBox.MaxX
            //    || point.Y < boundingBox.MinY || point.Y > boundingBox.MaxY)
            //{
            //    return false;
            //}

            bool inbounds = false;

            for (int i = 0, j = outerPoints.Count - 1; i < outerPoints.Count; j = i++)
            {
                if ((outerPoints[i].Y > point.Y) != (outerPoints[j].Y > point.Y) &&
                    point.X < (outerPoints[j].X - outerPoints[i].X) * (point.Y - outerPoints[i].Y) / (outerPoints[j].Y - outerPoints[i].Y) + outerPoints[i].X)
                {
                    inbounds = !inbounds;
                }
            }

            return inbounds;
        }

        private List<Platform> GeneratePlatforms(List<Point> outerPoints)
        {
            var platforms = new List<Platform>();

            var boundingBox = GetBoundingBox(outerPoints);

            Random random = new Random();

            int maxPlatforms = 20;
            int currentPlatforms = 0;

            while (currentPlatforms < maxPlatforms)
            {
                var diameter = random.Next(PlatformMinDiameter, PlatformMaxDiameter);
                var radius = diameter / 2;
                var centerPoint = new Point(random.Next(boundingBox.MinX, boundingBox.MaxX),
                        random.Next(boundingBox.MinY, boundingBox.MaxY));

                if (IsPointInBounds(outerPoints, centerPoint))
                {
                    platforms.Add(new Platform
                    {
                        Center = centerPoint,
                        Diameter = diameter,
                        Radius = radius
                    });

                    currentPlatforms++;
                }
            }

            return platforms;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Pen pen = new Pen(Color.Black, 5);

            var points = GetOuterPoints();

            if (points.Count > 1 && DrawLayout)
            {
                var platforms = GeneratePlatforms(points);
                DrawPlatforms(graphics, platforms);

                graphics.DrawPolygon(pen, points.ToArray());
            }

            graphics.Dispose();
        }

        private void DrawPlatforms(Graphics graphics, List<Platform> platforms)
        {
            //Pen pen = new Pen(Color.Red, 5);
            SolidBrush brush = new SolidBrush(Color.Red);

            foreach (var platform in platforms)
            {
                graphics.FillEllipse(brush, platform.Center.X - platform.Radius, platform.Center.Y - platform.Radius,
                    platform.Diameter, platform.Diameter);
            }
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            DrawLayout = true;
            Refresh();
        }

        private void NumSidesTextbox_TextChanged(object sender, EventArgs e)
        {
            TextBox numSidesTextBox = (TextBox)sender;
            if (int.TryParse(numSidesTextBox.Text, out int numSides))
            {
                NumSides = numSides;
            }
        }
    }

    public class Platform
    {
        public Point Center { get; set; }
        public int Diameter { get; set; }
        public int Radius { get; set; }
    }

    public class BoundingBox
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
    }
}
