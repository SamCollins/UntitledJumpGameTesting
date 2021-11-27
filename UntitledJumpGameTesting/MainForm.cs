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
        private int PlatformMinRadius = 20;
        private int PlatformMaxRadius = 30;
        private int MinBufferDistance = 10;

        private bool DrawLayout = false;

        /*--- Next Steps ---
         - Figure out way to get total area (Probably formula exists given list of vertices, maybe
           just split into a bunch of triangles using center point/radius for height?)
         - Figure out different amounts of platforms (low/med/high == 40/50/60 % of total area ???)
         - Different Platform shapes (Hexagons, maybe others)
         - Add More Textboxes for customizing values (Min/Max plat radius, Maybe buffer distance)
         - Maybe figure out way to set default values for textboxes
         */

        public MainForm()
        {
            InitializeComponent();

            WindowWidth = ClientRectangle.Width;
            WindowHeight = ClientRectangle.Height;
            WindowCenter = new Point(WindowWidth / 2, WindowHeight / 2);
        }

        private List<Point> CalculatePoints(Point centerPoint, int radius, int numSides)
        {
            List<Point> points = new List<Point>();

            for (int i = 0; i < NumSides; i++)
            {
                var x = centerPoint.X + (radius * Math.Cos(2 * Math.PI * i / numSides));
                var y = centerPoint.Y + (radius * Math.Sin(2 * Math.PI * i / numSides));
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

        private bool IsTouchingWalls(List<Point> outerPoints, Point centerPoint, int radius)
        {
            //First need to get "Standard" form equation for a line between two points (Ax + By + C = 0)
            for (int i = 0, j = 1; i < outerPoints.Count; i++, j++)
            {
                //Loops through checking the lines made up by adjacent points, on final iteration j is rest to 0
                //so that the line between the final/first point is checked also
                if (j == outerPoints.Count) j = 0;

                int A = outerPoints[i].Y - outerPoints[j].Y;
                int B = outerPoints[j].X - outerPoints[i].X;
                int C = (outerPoints[i].X * outerPoints[j].Y) - (outerPoints[j].X * outerPoints[i].Y);

                double distance = Math.Abs(
                    (A * centerPoint.X + B * centerPoint.Y + C) / Math.Sqrt(A * A + B * B));

                //If distance to wall is shorter than radius, platform is touching a wall
                //Maybe add min buffer distance here (Add to radius to enforce min distance from walls)
                if (distance < radius + MinBufferDistance)
                    return true;
            }

            return false;
        }

        private void TestWalls(List<Point> outerPoints, Graphics graphics)
        {
            Pen pen = new Pen(Color.Green, 5);

            //graphics.DrawLine(pen, outerPoints[0], outerPoints[1]);

            for (int i = 0, j = 1; i < outerPoints.Count; i++, j++)
            {
                //On final iteration set j back to first point so that line between the end/start is checked also
                if (j == outerPoints.Count) j = 0;

                graphics.DrawLine(pen, outerPoints[i], outerPoints[j]);
            }
        }

        private bool IsTouchingPlatforms(List<Platform> platforms, Point centerPoint, int radius)
        {
            foreach (var platform in platforms)
            {
                //Pythagorean theorum
                int xDiff = centerPoint.X - platform.Center.X;
                int yDiff = centerPoint.Y - platform.Center.Y;
                double distance = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

                if (distance < (platform.Radius + radius + MinBufferDistance))
                    return true;
            }

            return false;
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
                var centerPoint = new Point(random.Next(boundingBox.MinX, boundingBox.MaxX),
                        random.Next(boundingBox.MinY, boundingBox.MaxY));

                //Maybe find way to merge InBounds check and TouchingWalls check into one method
                //to reduce outerPoints copy's/method calls during generation
                if (IsPointInBounds(outerPoints, centerPoint))
                {
                    var radius = random.Next(PlatformMinRadius, PlatformMaxRadius);

                    if (!IsTouchingWalls(outerPoints, centerPoint, radius))
                    {
                        //For sure clean this up to be one method (IsValidPlatformSpawn or something)
                        if (!IsTouchingPlatforms(platforms, centerPoint, radius))
                        {
                            platforms.Add(new Platform
                            {
                                Center = centerPoint,
                                Radius = radius,
                                Diameter = radius * 2
                            });
                            currentPlatforms++;
                        }
                    }
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

            //By making radius 40% of height full shape will take up ~80% of window
            var outerRadius = (WindowHeight / 5) * 2;

            var outerPerimeterPoints = CalculatePoints(WindowCenter, outerRadius, NumSides);

            if (outerPerimeterPoints.Count > 1 && DrawLayout)
            {
                var platforms = GeneratePlatforms(outerPerimeterPoints);
                DrawPlatforms(graphics, platforms);

                graphics.DrawPolygon(pen, outerPerimeterPoints.ToArray());
            }

            graphics.Dispose();
        }

        private void DrawPlatforms(Graphics graphics, List<Platform> platforms)
        {
            Pen pen = new Pen(Color.Black, 5);
            SolidBrush brush = new SolidBrush(Color.Red);

            foreach (var platform in platforms)
            {
                graphics.FillEllipse(brush, platform.Center.X - platform.Radius, platform.Center.Y - platform.Radius,
                    platform.Diameter, platform.Diameter);
                graphics.DrawEllipse(pen, platform.Center.X - platform.Radius, platform.Center.Y - platform.Radius,
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
        public int Radius { get; set; }
        public int Diameter { get; set; }
    }

    public class BoundingBox
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
    }
}
