﻿using System.Globalization;
using YLScsDrawing.Drawing3d;
using System.IO.Ports;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Forms;
using System.Security.Cryptography.Xml;

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
        public Point3d[] vertices,colors;
        public List<Point3d> Point = new List<Point3d> { };
        public List<Point3d> Color = new List<Point3d> { };
        public List<Vector3i> SurfInd = new List<Vector3i> { };


        public string fileName;
        public Obj(string f) { fileName = f; }
        public void getdata()
        {

            int i = 0;
            int j = 0;
            int c = 0;
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
                    else if (line.StartsWith("c"))
                    {
                        //Console.WriteLine(i.ToString());
                        // Parse vertex data
                        c++;
                        string[] fields = line.Split(' ');
                        double r = double.Parse(fields[1], CultureInfo.InvariantCulture);
                        double g = double.Parse(fields[2], CultureInfo.InvariantCulture);
                        double b = double.Parse(fields[3], CultureInfo.InvariantCulture);
                        Point3d temp = new Point3d(r, g, b);
                        Color.Add(temp);

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
            List<Point3d> colorList = new List<Point3d>();
            //Console.WriteLine(SurfInd.Count);
            foreach (Vector3i it in SurfInd)
            {
                vertexList.Add(Point[it.x]);
                vertexList.Add(Point[it.y]);
                vertexList.Add(Point[it.z]);
                if (c == i)
                {
                   Point3d facecolor = new Point3d(Color[it.x], Color[it.y], Color[it.z]);
                   colorList.Add(facecolor);
                }
            }
            vertices = vertexList.ToArray();
            colors = colorList.ToArray();
            //Console.WriteLine(vertices.Length);
        }

    }
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        Pen redPen = new Pen(Color.Red, 4);
        Brush blueBrush = Brushes.Black;
        Random random = new Random();
        TriangularMesh mesh;
        List<TriangularMesh> object_;
        List<TriangularMesh> reference_ = new List<TriangularMesh> (4);
        List<Vector3d> pos_, vel_;
        List<int> type;
        Camera cam;
        private System.Windows.Forms.Timer timer;
        Quaternion orientation = new Quaternion(1.0, 0.0, 0.0, 0.0);
        SerialPort serialPort1 = new SerialPort("COM10", 115200);
        //SerialPort serialPort2 = new SerialPort("COM18", 115200);
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
        }
        private void Timer_Tick(object sender, EventArgs e)
        {   if (object_selected == false)
            {
                Quaternion offset = new Quaternion(0.7, 0.000, 0.00, 0.7);
            }
            if (object_selected == true)
            {
                this.DoubleBuffered = true;

                // Create an off-screen bitmap to draw the graphics
                Bitmap bitmap = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
                Graphics g = Graphics.FromImage(bitmap);

                // Clear the background
                g.Clear(Color.White);
                Quaternion offset = new Quaternion(0.6, 0.003, 0.00452, -0.0032);
                offset.Normalise();
                // Draw the graphics
                orientation.Multiply(offset);
                orientation.Normalise();
                //Console.WriteLine(orientation.ToStringQ());
                Point3d trans = new Point3d(0.00, 0, 0.1);
                mesh.RotateAt(mesh.Center, offset);
                //mesh.Translate(trans);
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
                        catch
                        {
                            //Console.WriteLine("catch");
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
                    line = line.TrimEnd();
                    if (line.StartsWith("v ")) // Process vertex lines
                    {
                        var parts = line.Split(' ');

                        // Multiply the vertex coordinates by 20

                        //var x = float.Parse(parts[1]) * 1.5f;
                        //var y = float.Parse(parts[2]) * 1.5f;
                        //var z = float.Parse(parts[3]) * 1.5f;

                        var x = float.Parse(parts[1]);
                        var y = float.Parse(parts[2]);
                        var z = float.Parse(parts[3]);

                        // Create the new vertex line
                        var newLine = $"v {x} {y} {z}";
                        lines.Add(newLine);

                        if (parts.Length == 7)
                        {
                            var r = float.Parse(parts[4]);
                            var g = float.Parse(parts[5]);
                            var b = float.Parse(parts[6]);
                            var newcLine = $"c {r} {g} {b}";
                            lines.Add(newcLine);
                        }
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
                        //lines.Add(line);
                        continue;
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
        private void reference_init()
        {
            string folder = @"C:\Users\Administrator\source\repos\WinFormsApp2\WinFormsApp2\Data\";
            string clock = "clockc.xobj";
            string bomb = "bombc.xobj";
            string apple = "applec.xobj";
            string donnut = "donnutc.xobj";

            List<string> reference = new List<string>();
            reference.Add(folder + apple);
            reference.Add(folder + donnut);
            reference.Add(folder + bomb);
            reference.Add(folder + clock);

            for (int i = 0; i < reference.Count; i++)
            {
                Obj a = new Obj(reference[i]);
                a.getdata();
                reference_.Add(new TriangularMesh(a.vertices,a.colors));
                reference_[i].Center = new Point3d(0, 0, 0);
                Point3d  t = new Point3d((i-1.5) * 5, 0, 0); 
                reference_[i].Translate(t);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ConvertObjFaceFormat(@"C:\Users\Administrator\source\repos\WinFormsApp2\WinFormsApp2\Data\donnutc.xobj");
            //Console.WriteLine("Open_file");
            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.InitialDirectory = @"C:\Users\Administrator\source\repos\WinFormsApp2\WinFormsApp2\Data\";
            //openFileDialog1.Filter = "Object files (*.xobj)|*.xobj|All files (*.*)|*.*";
            //openFileDialog1.Multiselect = false;

            //if (openFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    // The user selected a file - do something with it here
            //    string filename = openFileDialog1.FileName;
            //    Console.WriteLine("Selected file: " + filename);
            //    Obj a = new Obj(filename);
            //    cam = new Camera();
            //    cam.Location = new Point3d(-0, -0, -6);
            //    a.getdata();
            //    mesh = new TriangularMesh(a.vertices);
            //    mesh.Center = new Point3d(0, 0, 0);
            //}
            //object_selected = true;
            //try
            //{
            //    serialPort1.Open();
            //}
            //catch (IOException ex)
            //{
            //    MessageBox.Show("Error opening serial port1: " + ex.Message);
            //}
            ////try
            ////{
            ////    serialPort2.Open();
            ////}
            ////catch (IOException ex)
            ////{
            ////    MessageBox.Show("Error opening serial port2: " + ex.Message);
            ////}
            //serial_opened = true;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.SelectedItem.ToString();
            object_selected = true;
            cam = new Camera();
            cam.Location = new Point3d(-0, -0, -10);
            switch (selectedItem)
            {
                case "Apple":
                    mesh = reference_[0];
                    mesh.Center = reference_[0].Center;
                    break;

                case "Donnut":
                    mesh = reference_[1];
                    mesh.Center = reference_[1].Center;
                    break;

                case "Bomb":
                    mesh = reference_[2];
                    mesh.Center = reference_[2].Center;
                    break;

                case "Clock":
                    mesh = reference_[3];
                    mesh.Center = reference_[3].Center;
                    break;

            default:
                    // Default logic when no specific item is selected
                    break;
            }
            object_selected = true;
        }

        private void InitializeComponent()
        {
            
            components = new System.ComponentModel.Container();
            timer = new System.Windows.Forms.Timer(components);
            button1 = new Button();
            comboBox1 = new ComboBox();
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
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Apple", "Donnut", "Bomb", "Clock" });
            comboBox1.Location = new Point(543, 395);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(121, 23);
            comboBox1.TabIndex = 1;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(comboBox1);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            MouseClick += Control1_MouseClick;
            ResumeLayout(false);
            reference_init();
        }

        #endregion

        private Button button1;
        private ComboBox comboBox1;
    }
}