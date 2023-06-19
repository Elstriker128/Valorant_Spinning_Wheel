using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Web;

namespace Valorant_Spinning_Wheel
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private float currentAngle = 0;
        private float spinSpeed;
        private float friction = 0.1f;
        
        private Brush[] valueBrushes;

        XDocument document = XDocument.Load("C:/Users/User/OneDrive/Desktop/VSP/Valorant_Spinning_Wheel/Agent_Data/Agents.xml");
        private int count = 0;
        private string[] agents;

        public Form1()
        {
            InitializeComponent();

            //Generate random wheel spin speed between 80 and 110
            Random random = new Random();
            spinSpeed = random.Next(80,110);

            label1.Text = "Agent";
            
            pictureBox2.Image = Image.FromFile("C:/Users/User/OneDrive/Desktop/VSP/Valorant_Spinning_Wheel/Style/Arrow.png");
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            //Initiate agents array
            agents = new string[document.Descendants().Count()-1];
            ReadAgents();
            Array.Resize(ref agents, agents.Length);

            //Initiate colors for the arcs
            valueBrushes = new Brush[agents.Count()];
            valueBrushes = GenerateBrushes();

            //Initialize timer
            timer = new Timer();
            timer.Interval = 25;
            timer.Tick += new EventHandler(timer_Tick);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Start the spinning
            currentAngle = 0;
            timer.Start();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //Reset the wheel and timer
            timer.Stop();
            Random random = new Random();
            spinSpeed = random.Next(80, 110);
            currentAngle = 0;
            valueBrushes = new Brush[agents.Count()];
            valueBrushes = GenerateBrushes();
            pictureBox1.Invalidate();
        }
        /// <summary>
        /// A method that reads all the agents from a XML file
        /// </summary>
        private void ReadAgents()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("C:/Users/User/OneDrive/Desktop/VSP/Valorant_Spinning_Wheel/Agent_Data/Agents.xml");

            XmlElement root = doc.DocumentElement;
            foreach (XmlNode node in root.ChildNodes)
            {
                string agentName = node.Attributes["name"].Value;

                agents[count]= agentName;
                count++;
            }
        }
        /// <summary>
        /// A method that returns an array of different color brushes for each of the agents
        /// </summary>
        /// <returns>an array of different color brushes for each of the agents</returns>
        private Brush[] GenerateBrushes()
        {
            Brush[] brushes= new Brush[agents.Count()];
            Random random= new Random();
            for (int i = 0; i < agents.Count(); i++)
            {
                int r = random.Next(1,256);
                int g = random.Next(1,256);
                int b = random.Next(1, 256);
                brushes[i]=new SolidBrush(Color.FromArgb(r, g, b));
            }
            return brushes;
        }
        /// <summary>
        /// A method that paints a round pie like object into the pictureBox1
        /// </summary>
        /// <param name="sender">an object of the object class</param>
        /// <param name="e">an object of the PaintEventArgs event class</param>
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            float angle = 0;
            float sweep = 360f / agents.Length;
            PointF center = new PointF(pictureBox1.Width/2,pictureBox1.Height/2);
            e.Graphics.TranslateTransform(pictureBox1.Width / 2, pictureBox1.Height / 2);
            e.Graphics.RotateTransform(currentAngle);
            e.Graphics.TranslateTransform(-pictureBox1.Width / 2, -pictureBox1.Height / 2);
            for (int i = 0; i < agents.Count(); i++)
            {
                e.Graphics.FillPie(valueBrushes[i % valueBrushes.Length], pictureBox1.ClientRectangle, angle, sweep);
                e.Graphics.DrawPie(Pens.Black, pictureBox1.ClientRectangle, angle, sweep);
                DrawStringOnArc(e.Graphics, agents[i], pictureBox1.Font, valueBrushes[i % valueBrushes.Length], pictureBox1.ClientRectangle, angle, sweep, center, i);
                angle += sweep;
                
            }
        }
        /// <summary>
        /// A method that repeats everytime the timer ticks
        /// </summary>
        /// <param name="sender">an object of the object class</param>
        /// <param name="e">an object of the PaintEventArgs event class</param>
        private void timer_Tick(object sender, EventArgs e)
        {
            // Spin the wheel
            spinSpeed *= (1 - friction);
            if (spinSpeed < 0.1f)
            {
                spinSpeed = 0f;
                timer.Stop();
                string landedValue = CalculateLandedValue(270-currentAngle);
                MessageBox.Show($"You landed on {landedValue}!");
                return;
            }
            currentAngle += spinSpeed;
            pictureBox1.Invalidate();
            label1.Text = CalculateLandedValue(270-currentAngle).ToString();
        }
        /// <summary>
        /// A method that calculates the value that the wheel stopped on
        /// </summary>
        /// <param name="angle">the angle that the wheel stopped on</param>
        /// <returns>the value that the wheel stopped on</returns>
        private string CalculateLandedValue(float angle)
        {
            //Calculate the value that the wheel landed on
            float anglePerValue = 360f / agents.Count();
            float landedAngle = angle % 360f;
            if (landedAngle < 0)
            {
                landedAngle += 360;
            }
                int landedIndex = (int)(landedAngle / anglePerValue);
                return agents[landedIndex];
        }
        /// <summary>
        /// A method that draws a string on the painted pie and inserts the agent's name into the neccesarry spot
        /// </summary>
        /// <param name="g">The painted pie object</param>
        /// <param name="s">The name of the agent</param>
        /// <param name="font">The used font for the agents name</param>
        /// <param name="brush">The used brush for painting the arc</param>
        /// <param name="rect">An object of the painted rectangle</param>
        /// <param name="startAngle">The start from where the arcs will be painted</param>
        /// <param name="sweepAngle">The size for each of the arcs</param>
        /// <param name="center">The center of the rectangle</param>
        /// <param name="count">The amount of agents</param>
        private void DrawStringOnArc(Graphics g, string s, Font font, Brush brush, RectangleF rect, float startAngle, float sweepAngle, PointF center, int count)
        {
            // Draw a string along an arc
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect, startAngle, sweepAngle);
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            g.DrawPath(Pens.Black, path);
            g.FillPath(brush, path);

            

            //Get the bounds of the path
            RectangleF pathBounds = path.GetBounds();

            float dx = center.X - (pathBounds.X + pathBounds.Width / 2);
            float dy = center.Y - (pathBounds.Y + pathBounds.Height / 2);
            float distance = (float)Math.Sqrt(dx * dx + dy * dy) - 45;

            double x = center.X + distance * Math.Cos(((sweepAngle * count) + (480 * Math.PI / 180)) * Math.PI / 180);
            double y = center.Y + distance * Math.Sin(((sweepAngle * count) + (480 * Math.PI / 180)) * Math.PI / 180);

            PointF textPosition = new PointF((float)x, (float)y);

            g.DrawString(s, font, Brushes.Black, textPosition, format);

        }
        
    }
}
