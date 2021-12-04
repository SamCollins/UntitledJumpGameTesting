using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        private readonly int WindowWidth, WindowHeight = 0;
        private Point WindowCenter;

        //Simulation Params
        private int MinBufferDistance = 10;
        private double PlatformAreaScale = 0.2; //Platforms will take up 20% of space

        //Customizable Params (Set from textboxes)
        private int NumSides;
        private int PlatformMinRadius;
        private int PlatformMaxRadius;

        //Generation Data
        private int OuterPerimeterRadius;
        private List<Point> OuterPerimeterPoints = new List<Point>();
        private List<Platform> Platforms = new List<Platform>();
        private bool GenerationComplete = false;

        private Timer Timer;
        private DateTime StartTime;
        private bool IsTimerRunning = false;

        /*--- Next Steps ---
         - Figure out different amounts of platforms (low/med/high == 40/50/60 % of total area ???)
         - Some type of control for switching between amounts of platforms (Dropdown?)
         - Different Platform shapes (Hexagons, maybe others)
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

            for (int i = 0; i < numSides; i++)
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

        private double GetAreaOfPolygon(List<Point> points)
        {
            double area = 0;
            int j = points.Count - 1;

            for (int i = 0; i < points.Count; i++)
            {
                area += (points[j].X + points[i].X) * (points[j].Y - points[i].Y);
                j = i;
            }

            return Math.Abs(area / 2);
        }

        private bool IsValidSpawn(List<Point> outerPerimeterPoints, Point platCenterPoint, int platRadius)
        {
            //Initially had BoundingBox check here, but removed since points are only generated using bounding box as min/max

            //Is Point in Polygon check, uses raycasting/edge crossing algorithm (Count how many edges are between a ray from
            //outside the polygon to the point, if odd the point is inside, if even the point is outside)
            bool inbounds = false;
            for (int i = 0, j = outerPerimeterPoints.Count - 1; i < outerPerimeterPoints.Count; j = i++)
            {
                if ((outerPerimeterPoints[i].Y > platCenterPoint.Y) != (outerPerimeterPoints[j].Y > platCenterPoint.Y) &&
                        platCenterPoint.X < (outerPerimeterPoints[j].X - outerPerimeterPoints[i].X) * (platCenterPoint.Y - outerPerimeterPoints[i].Y) 
                            / (outerPerimeterPoints[j].Y - outerPerimeterPoints[i].Y) + outerPerimeterPoints[i].X)
                {
                    inbounds = !inbounds;
                }
            }

            //If center point not in bounds the spawn is not valid
            if (!inbounds)
                return false;

            //First need to get "Standard" form equation for a line between two points (Ax + By + C = 0)
            for (int i = 0, j = 1; i < outerPerimeterPoints.Count; i++, j++)
            {
                //Loops through checking the lines made up by adjacent points, on final iteration j is rest to 0
                //so that the line between the final/first point is checked also
                if (j == outerPerimeterPoints.Count) j = 0;

                int A = outerPerimeterPoints[i].Y - outerPerimeterPoints[j].Y;
                int B = outerPerimeterPoints[j].X - outerPerimeterPoints[i].X;
                int C = (outerPerimeterPoints[i].X * outerPerimeterPoints[j].Y) - (outerPerimeterPoints[j].X * outerPerimeterPoints[i].Y);

                double distance = Math.Abs(
                    (A * platCenterPoint.X + B * platCenterPoint.Y + C) / Math.Sqrt(A * A + B * B));

                //If distance to any wall is shorter than new platform radius + buffer distance
                //platform is touching a wall and spawn isn't valid
                if (distance < platRadius + MinBufferDistance)
                    return false;
            }

            foreach (var platform in Platforms)
            {
                //Pythagorean theorum to find distance between two points
                int xDiff = platCenterPoint.X - platform.Center.X;
                int yDiff = platCenterPoint.Y - platform.Center.Y;
                double distance = Math.Sqrt((xDiff * xDiff) + (yDiff * yDiff));

                //If distance between any platform center and new platform center is less than both platform
                //radiuses combined then platforms are overlapping and spawn isn't valid
                if (distance < (platform.Radius + platRadius + MinBufferDistance))
                    return false;
            }

            return true;
        }

        private void GeneratePlatforms()
        {
            Debug.WriteLine("Generating Platforms...");

            var outerPerimeterPoints = OuterPerimeterPoints;
            var boundingBox = GetBoundingBox(outerPerimeterPoints);

            Random random = new Random();

            //Potential bug, if area scaling value too high, could end up with no valid placement
            //for new platform, so even though limit isn't reached program will deadlock in
            //infinite loop, can also get locked if plat min radius is too small (2)
            //probably cause the added buffer makes no valid spots since so many small plats
            //Fix will be something like counter/failsafe that checks how many times it tries and fails
            //to find a valid spot, then breaking loop once that limit is hit. Maybe also include a
            //min/max of the platform area that needs to be hit, like if it cant find any valid spots
            //but its at like 80-90% of platform area you can call it good and move on
            double totalArea = GetAreaOfPolygon(outerPerimeterPoints);
            double maxPlatformArea = totalArea * PlatformAreaScale;
            double combinedPlatformArea = 0;

            while (combinedPlatformArea < maxPlatformArea)
            {
                var platCenterPoint = new Point(random.Next(boundingBox.MinX, boundingBox.MaxX),
                        random.Next(boundingBox.MinY, boundingBox.MaxY));
                var platRadius = random.Next(PlatformMinRadius, PlatformMaxRadius);

                if (IsValidSpawn(outerPerimeterPoints, platCenterPoint, platRadius))
                {
                    Platforms.Add(new Platform
                    {
                        Center = platCenterPoint,
                        Radius = platRadius,
                        Diameter = platRadius * 2
                    });
                    //Adjust this when changing platforms to hexagons ??
                    combinedPlatformArea += Math.PI * (platRadius * platRadius);
                }
            }

            Debug.WriteLine("Generation Complete");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Set default values for simulation params
            NumSides = 10;
            PlatformMinRadius = 10;
            PlatformMaxRadius = 20;

            //By making radius 40% of height full shape will take up ~80% of window
            OuterPerimeterRadius = (WindowHeight / 5) * 2;

            NumSidesTextbox.Text = NumSides.ToString();
            PlatMinRadTextbox.Text = PlatformMinRadius.ToString();
            PlatMaxRadTextbox.Text = PlatformMaxRadius.ToString();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Pen pen = new Pen(Color.Black, 5);

            if (GenerationComplete && OuterPerimeterPoints.Count > 1)
            {
                graphics.DrawPolygon(pen, OuterPerimeterPoints.ToArray());
                DrawPlatforms(graphics);
            }

            graphics.Dispose();
        }

        private void DrawPlatforms(Graphics graphics)
        {
            Pen pen = new Pen(Color.Black, 5);
            SolidBrush brush = new SolidBrush(Color.Red);

            foreach (var platform in Platforms)
            {
                graphics.FillEllipse(brush, platform.Center.X - platform.Radius, platform.Center.Y - platform.Radius,
                    platform.Diameter, platform.Diameter);
                graphics.DrawEllipse(pen, platform.Center.X - platform.Radius, platform.Center.Y - platform.Radius,
                    platform.Diameter, platform.Diameter);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeDisplay.Text = string.Format(@"Time : {0:mm\:ss\.ff}", (DateTime.Now - StartTime));
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Timer = new Timer
            {
                Interval = 100
            };
            Timer.Tick += new EventHandler(Timer_Tick);
            StartTime = DateTime.Now;
            Timer.Start();
            IsTimerRunning = true;
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            if (NumSides > 1 && PlatformMinRadius > 0 && PlatformMaxRadius > 0 && PlatformMinRadius <= PlatformMaxRadius)
            {
                Platforms = new List<Platform>();
                OuterPerimeterPoints = CalculatePoints(WindowCenter, OuterPerimeterRadius, NumSides);
                GeneratePlatforms();
                GenerationComplete = true;
            }
            
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

        private void PlatMinRadTextbox_TextChanged(object sender, EventArgs e)
        {
            TextBox platMinRadTextbox = (TextBox)sender;
            if (int.TryParse(platMinRadTextbox.Text, out int platMinRadius))
            {
                PlatformMinRadius = platMinRadius;
            }
        }

        private void PlatMaxRadTextbox_TextChanged(object sender, EventArgs e)
        {
            TextBox platMaxRadTextbox = (TextBox)sender;
            if (int.TryParse(platMaxRadTextbox.Text, out int platMaxRadius))
            {
                PlatformMaxRadius = platMaxRadius;
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
