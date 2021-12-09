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

        //Generation Params
        private const int MinBufferDistance = 10;
        private const double PlatformAreaScale = 0.2; //Platforms will take up 20% of space
        private const int GenerationFailsafeCutoffLimit = 100000;
        private const int MinPlatformCount = 10;

        //Customizable Generation Params (Set from textboxes)
        private int NumSides;
        private int PlatformMinRadius;
        private int PlatformMaxRadius;

        //Generation Data
        private int OuterPerimeterRadius;
        private List<Point> OuterPerimeterPoints = new List<Point>();
        private List<Platform> Platforms = new List<Platform>();
        private bool GenerationComplete = false;

        //Simulation Params
        private Timer DisplayTimer;
        private Timer SimulationTimer;
        private DateTime SimStartTime;
        private bool SimulationRunning = false;
        private List<Platform> InitialPlatforms = new List<Platform>();

        /*--- Next Steps ---
         - Figure out different amounts of platforms (low/med/high == 10/20/30 % of total area ???)
         - Some type of control for switching between amounts of platforms (Dropdown?)
         - Different Platform shapes (Hexagons, maybe others)
         - Set up simulation based on timer that removes random platforms ever few seconds
         - Scale how many platforms dissappear per second based on how many platforms there are
         - Figure out cleaner way of writing debug info to console (Make toggleable somehow)
         - Improve single platform generation/simulation logic/timing
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

        private double GetBufferedPlatformArea()
        {
            double platformAreaWithBuffer = 0;
            
            foreach (var platform in Platforms)
            {
                platformAreaWithBuffer += platform.GetBufferedArea(MinBufferDistance);
            }

            return platformAreaWithBuffer;
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
            
            double totalArea = GetAreaOfPolygon(outerPerimeterPoints);
            double maxPlatformArea = totalArea * PlatformAreaScale;
            double combinedPlatformArea = 0;
            double bufferedPlatformArea = 0;

            int generateCount = 0;

            while (combinedPlatformArea < maxPlatformArea)
            {
                var platCenterPoint = new Point(random.Next(boundingBox.MinX, boundingBox.MaxX),
                        random.Next(boundingBox.MinY, boundingBox.MaxY));
                var platRadius = random.Next(PlatformMinRadius, PlatformMaxRadius);

                generateCount++;

                if (IsValidSpawn(outerPerimeterPoints, platCenterPoint, platRadius))
                {
                    var newPlatform = new Platform
                    {
                        Center = platCenterPoint,
                        Radius = platRadius,
                        Diameter = platRadius * 2
                    };

                    Platforms.Add(newPlatform);
                    combinedPlatformArea += newPlatform.GetArea();
                    bufferedPlatformArea += newPlatform.GetBufferedArea(MinBufferDistance);
                }

                /*Added Failsafe for two cases that were causing deadlocks:
                 * Total area of all platforms + their buffer is greater than the total area of the outer polygon
                 * (Meaning no more space is available, even though maxPlatformArea hasn't been reached)
                 * Any remaining space in the polygon isn't large enough for even a min radius platform (didn't
                 * really know how to fix this one so just added a cutoff limit)
                */
                if (bufferedPlatformArea >= totalArea || generateCount == GenerationFailsafeCutoffLimit)
                {
                    Debug.WriteLine("Failsafe Cutoff reached.");
                    Debug.WriteLine("Iteration Reached: {0}/{1}(LIMIT)", generateCount, GenerationFailsafeCutoffLimit);
                    Debug.WriteLine("Total Area: {0} Buffered Plat Area: {1}",
                                totalArea, bufferedPlatformArea);
                    break;
                }
            }

            Debug.WriteLine("Generation Complete. Total Iterations: {0}", generateCount);
        }

        private Platform GeneratePlatform()
        {
            var outerPerimeterPoints = OuterPerimeterPoints;
            //var boundingBox = GetBoundingBox(outerPerimeterPoints);
            var platformCenters = Platforms.Select(p => p.Center).ToList();
            var boundingBox = GetBoundingBox(platformCenters);

            Random random = new Random();

            int generateCount = 0;

            Platform platform = null;

            while (true)
            {
                var platCenterPoint = new Point(random.Next(boundingBox.MinX, boundingBox.MaxX),
                            random.Next(boundingBox.MinY, boundingBox.MaxY));
                var platRadius = random.Next(PlatformMinRadius, PlatformMaxRadius);

                generateCount++;

                if (IsValidSpawn(outerPerimeterPoints, platCenterPoint, platRadius))
                {
                    platform = new Platform
                    {
                        Center = platCenterPoint,
                        Radius = platRadius,
                        Diameter = platRadius * 2
                    };
                    Debug.WriteLine("Platform Add - Iterations: " + generateCount);
                    //platformSpawned = true;
                    break;
                }

                //Add some debug logging here
                if (generateCount == GenerationFailsafeCutoffLimit)
                    break;
            }

            return platform;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //Set default values for simulation params
            NumSides = 10;
            PlatformMinRadius = 20;
            PlatformMaxRadius = 30;

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

        private void BeginSimulation()
        {
            SimStartTime = DateTime.Now;
            InitialPlatforms = new List<Platform>(Platforms);

            DisplayTimer = new Timer { Interval = 100 };
            DisplayTimer.Tick += new EventHandler(DisplayTimer_Tick);

            SimulationTimer = new Timer { Interval = 1000 };
            SimulationTimer.Tick += new EventHandler(SimulationTimer_Tick);

            DisplayTimer.Start();
            SimulationTimer.Start();
            SimulationRunning = true;
        }

        private void EndSimulation()
        {
            DisplayTimer.Stop();
            SimulationTimer.Stop();
            SimulationRunning = false;

            Platforms = new List<Platform>(InitialPlatforms);
            TimeDisplay.Text = "Time: 00:00.00";
        }

        private void SimulationTick(TimeSpan runtime)
        {
            if (GenerationComplete && Platforms.Count() > MinPlatformCount)
            {
                //Clean this up
                if (runtime.Seconds % 3 == 0)
                {
                    //Maybe figure out way of not including new plats in remove random
                    //Could have seperate list for new plats that gets unioned/concat during Draw?
                    Platform newPlat = GeneratePlatform();
                    if (newPlat != null)
                    {
                        Platforms.Add(newPlat);
                        Debug.WriteLine("Platform added");
                    }
                }
                else
                {
                    Random random = new Random();
                    int randomPlatIndex = random.Next(0, Platforms.Count());
                    Platforms.RemoveAt(randomPlatIndex);
                    Debug.WriteLine("Platform removed");
                }

                Refresh();
            }
        }

        private void DisplayTimer_Tick(object sender, EventArgs e)
        {
            TimeDisplay.Text = string.Format(@"Time: {0:mm\:ss\.ff}", (DateTime.Now - SimStartTime));
        }

        private void SimulationTimer_Tick(object sender, EventArgs e)
        {
            var simulationRuntime = DateTime.Now - SimStartTime;
            Debug.WriteLine("Simulation Tick: {0} seconds passed", simulationRuntime.Seconds);

            SimulationTick(simulationRuntime);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (GenerationComplete)
            {
                if (!SimulationRunning)
                {
                    BeginSimulation();
                    StartButton.Text = "Stop";
                }
                else
                {
                    EndSimulation();
                    StartButton.Text = "Start";
                }

                Refresh();
            }
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

        public double GetArea()
        {
            return Math.PI * (Radius * Radius);
        }

        public double GetBufferedArea(int buffer)
        {
            int bufferedRadius = Radius + buffer;
            return Math.PI * (bufferedRadius * bufferedRadius);
        }
    }

    public class BoundingBox
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }
    }
}
