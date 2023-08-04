using System.Globalization;
using YLScsDrawing.Drawing3d;
using System.IO.Ports;

namespace WinFormsApp2
{
    public class Vector3i
    {
        public int x, y, z;
        public Vector3i(int a, int b, int c) { x = a; y = b; z = c; }
    }
    public class Tri
    {
        public Vector3d A;
        public Vector3d B;
        public Vector3d C;
        public Tri() { }
        public Tri(Vector3d A_, Vector3d B_, Vector3d C_)
        {
            A = A_;
            B = B_;
            C = C_;
        }

    }
    public class Obj
    {
        public int height = 400; public int width = 800;
        public Point3d[] vertices;
        public List<Point3d> Point = new List<Point3d> { };
        public List<Point3d> ProjectedPoint = new List<Point3d> { };
        public List<Vector3i> SurfInd = new List<Vector3i> { };
        public List<Tri> Surface = new List<Tri> { };

        public string fileName;
        public Obj(string f) { fileName = f; }
        public void getdata()
        {

            int i = 0;
            int j = 0;
            using (StreamReader reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    //Console.WriteLine($"Read line: {line}");
                    if (line.StartsWith("v"))
                    {
                        i++;
                        //Console.WriteLine(i.ToString());
                        // Parse vertex data
                        string[] fields = line.Split(' ');
                        double x = double.Parse(fields[1], CultureInfo.InvariantCulture);
                        double y = double.Parse(fields[2], CultureInfo.InvariantCulture);
                        double z = double.Parse(fields[3], CultureInfo.InvariantCulture);
                        Point3d temp = new Point3d(x, y, z);
                        Point.Add(temp);

                    }
                    else if (line.StartsWith("f"))
                    {
                        j++;
                        //Console.WriteLine(j.ToString());
                        // Parse surface data
                        string[] fields = line.Split(' ');
                        int i1 = int.Parse(fields[1]) - 1;
                        int i2 = int.Parse(fields[2]) - 1;
                        int i3 = int.Parse(fields[3]) - 1;
                        SurfInd.Add(new Vector3i(i1, i2, i3));
                    }
                }
            }
            List<Point3d> vertexList = new List<Point3d>();
            //Console.WriteLine(SurfInd.Count);
            foreach (Vector3i it in SurfInd)
            {
                vertexList.Add(Point[it.x]);
                vertexList.Add(Point[it.y]);
                vertexList.Add(Point[it.z]);
            }
            vertices = vertexList.ToArray();
            //Console.WriteLine(vertices.Length);
        }

    }
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        List<System.Drawing.Point> points = new List<System.Drawing.Point>();
        Pen redPen = new Pen(Color.Red, 4);
        Brush blueBrush = Brushes.Black;
        TriangularMesh mesh;
        Camera cam;
        private System.Windows.Forms.Timer timer;
        Quaternion orientation = new Quaternion(1.0, 0.0, 0.0, 0.0);
        SerialPort serialPort1 = new SerialPort("COM15", 115200);
        SerialPort serialPort2 = new SerialPort("COM18", 115200);
        bool serial_opened = false;
        bool object_selected = false;
        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Control1_MouseClick(Object sender, MouseEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            Console.WriteLine(e.X.ToString() + " " + e.Y.ToString());
            points.Add(new Point(e.X, e.Y));
            if (points.Count > 1)
            {
                int x1 = points[points.Count - 2].X;
                int y1 = points[points.Count - 2].Y;

                int x2 = points[points.Count - 1].X;
                int y2 = points[points.Count - 1].Y;

                System.Drawing.Point A = new System.Drawing.Point(x1, y1);
                System.Drawing.Point B = new System.Drawing.Point(x2, y2);

                g.DrawLine(redPen, A, B);
                if (points.Count > 2)
                {
                    int x3 = points[points.Count - 3].X;
                    int y3 = points[points.Count - 3].Y;
                    System.Drawing.Point C = new System.Drawing.Point(x3, y3);
                    g.DrawLine(redPen, C, B);
                    System.Drawing.Point[] poly = { A, B, C };
                    g.FillPolygon(blueBrush, poly);
                    points.Clear();
                }

            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (object_selected == true)
            {
                this.DoubleBuffered = true;

                // Create an off-screen bitmap to draw the graphics
                Bitmap bitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                Graphics g = Graphics.FromImage(bitmap);

                // Clear the background
                g.Clear(Color.White);
                Quaternion offset = new Quaternion(0.8, 0.004, 0.0012, -0.0010);
                offset.Normalise();
                // Draw the graphics
                //orientation.Multiply(offset);
                orientation.Normalise();
                //Console.WriteLine(orientation.ToStringQ());
                mesh.RotateAt(mesh.Center, offset);
                mesh.ProjectPoints(cam);
                mesh.Draw(g, cam);

                // Display quaternion value in upper right corner
                string quaternionValue = string.Format("w: {0:0.00}, x: {1:0.00}, y: {2:0.00}, z: {3:0.00}", orientation.W, orientation.X, orientation.Y, orientation.Z);
                Font font = new Font("Arial", 10);
                Brush brush = Brushes.Black;
                SizeF size = g.MeasureString(quaternionValue, font);
                float x = this.ClientSize.Width - size.Width - 10;
                float y = 10;
                g.DrawString(quaternionValue, font, brush, x, y);

                // Draw the bitmap to the screen
                Graphics formGraphics = this.CreateGraphics();
                formGraphics.DrawImage(bitmap, 0, 0);

                // Dispose of the graphics objects
                g.Dispose();
                formGraphics.Dispose();
                bitmap.Dispose();
                if (serial_opened)
                {
                    string[] lines = serialPort1.ReadExisting().Split('\n');
                    string lastLine = lines.Last().Trim();
                    string[] values = lastLine.Split(',');
                    int[] integers = new int[values.Length];
                    bool has_controller_data = false;
                    if (values.Length == 4)
                    {
                        has_controller_data = true;
                    }
                    else if (lines.Length > 1)
                    {
                        lastLine = lines[lines.Length - 2].Trim();
                        values = lastLine.Split(',');
                        if (values.Length == 4)
                        {
                            integers = new int[values.Length];
                            has_controller_data = true;
                        }
                    }
                    if (has_controller_data)
                    {
                        try
                        {
                            for (int i = 0; i < 4; i++)
                            {
                               integers[i] = int.Parse(values[i]);
                            }
                            if (integers[0] == 1)
                            {
                                cam.MoveIn(0.1);
                            }
                            if (integers[1] == 1)
                            {
                                cam.MoveOut(0.1);
                            }
                            if (integers[2] == 1)
                            {
                                cam.MoveLeft(0.1);
                            }
                            if (integers[3] == 1)
                            {
                                cam.MoveRight(0.1);
                            }
                            //Console.WriteLine(cam.Location.X+" "+cam.Location.Y+cam.Location.Z);
                        }
                        catch {
                            Console.WriteLine("catch");
                        }
                    }
                    //g.DrawString(move, font, brush, x, 50);

                    //string data2 = serialPort2.ReadLine();
                    //
                    //Console.WriteLine("1: " + integers[0]);
                    //Console.WriteLine("2: " + integers[1]);
                    //Console.WriteLine("3: " + integers[2]);
                    //Console.WriteLine("4: " + integers[3]);
                    //Console.WriteLine(lastLine + " "+ values.Length);
                }

            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>


        public static void ConvertObjFaceFormat(string filePath)
        {
            var lines = new List<string>();

            // Read the OBJ file
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("v ")) // Process vertex lines
                    {
                        var parts = line.Split(' ');

                        // Multiply the vertex coordinates by 20
                        var x = float.Parse(parts[1]) * 80;
                        var y = float.Parse(parts[2]) * 80;
                        var z = float.Parse(parts[3]) * 80;

                        // Create the new vertex line
                        var newLine = $"v {x} {y} {z}";
                        lines.Add(newLine);
                    }
                    else if (line.StartsWith("f ")) // Process face lines
                    {
                        var parts = line.Split(' ');

                        // Convert the face line to the new format
                        if (parts.Length == 5) // Quad face
                        {
                            var indices = new int[4];
                            for (int i = 1; i < 5; i++)
                            {
                                var subParts = parts[i].Split('/');
                                indices[i - 1] = int.Parse(subParts[0]);
                            }

                            var newLine1 = $"f {indices[0]} {indices[1]} {indices[2]}";
                            var newLine2 = $"f {indices[0]} {indices[2]} {indices[3]}";
                            lines.Add(newLine1);
                            lines.Add(newLine2);
                        }
                        else // Triangle face
                        {
                            var indices = new int[3];
                            for (int i = 1; i < 4; i++)
                            {
                                var subParts = parts[i].Split('/');
                                indices[i - 1] = int.Parse(subParts[0]);
                            }

                            var newLine = $"f {indices[0]} {indices[1]} {indices[2]}";
                            lines.Add(newLine);
                        }
                    }
                    else
                    {
                        // Add non-vertex and non-face lines to the output list
                        lines.Add(line);
                    }
                }
            }

            // Write the new OBJ file
            using (var writer = new StreamWriter(filePath))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //ConvertObjFaceFormat(@"C:\Users\Administrator\source\repos\WinFormsApp2\WinFormsApp2\Data\donut.obj");
            Console.WriteLine("Open_file");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\Users\Administrator\source\repos\WinFormsApp2\WinFormsApp2\Data\";
            openFileDialog1.Filter = "Object files (*.xobj)|*.xobj|All files (*.*)|*.*";
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // The user selected a file - do something with it here
                string filename = openFileDialog1.FileName;
                Console.WriteLine("Selected file: " + filename);
                Obj a = new Obj(filename);
                cam = new Camera();
                cam.Location = new Point3d(-0, -0, -6);
                a.getdata();
                mesh = new TriangularMesh(a.vertices);
                mesh.Center = new Point3d(0, 0, 0);
            }
            object_selected = true;
            try
            {
                serialPort1.Open();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error opening serial port1: " + ex.Message);
            }
            try
            {
                serialPort2.Open();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error opening serial port2: " + ex.Message);
            }
            serial_opened = true;

        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timer = new System.Windows.Forms.Timer(components);
            button1 = new Button();
            SuspendLayout();
            // 
            // timer
            // 
            timer.Enabled = true;
            timer.Interval = 20;
            timer.Tick += Timer_Tick;
            // 
            // button1
            // 
            button1.Location = new Point(691, 388);
            button1.Name = "button1";
            button1.Size = new Size(79, 30);
            button1.TabIndex = 0;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            MouseClick += Control1_MouseClick;
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
    }
}