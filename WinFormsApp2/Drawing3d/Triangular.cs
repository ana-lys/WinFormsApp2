using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YLScsDrawing.Drawing3d;

public class TriangularMesh : Shape3d
{
    private PointF[] projectedPoints;
    private bool[] visible;
    private bool haveColor;
    private float[] distance;
    private Color[] faceColor;

    public TriangularMesh(Point3d[] vertices, Point3d[] colors)
    {
        //Console.WriteLine(vertices.Length);
        if (vertices.Length % 3 != 0)
        {
            throw new ArgumentException("The number of vertices must be divisible by 3.");
        }
        if (colors.Length != vertices.Length / 3)
        {
            haveColor = false;
        }
        else
        {
            haveColor = true;
            faceColor = new Color[colors.Length];
            for (int i=0; i < colors.Length; i++) {
                int red   = (int)(colors[i].X * 255);
                int green = (int)(colors[i].Y * 255);
                int blue  = (int)(colors[i].Z * 255);
                Color color = Color.FromArgb(red, green, blue);
                faceColor[i] = color;
            }
        }

        pts = vertices;
        projectedPoints = new PointF[vertices.Length];
        visible = new bool[vertices.Length/3];
        distance = new float[vertices.Length / 3];

        for (int i = 0; i < vertices.Length; i++)
        {
            projectedPoints[i] = new PointF(float.MaxValue, float.MaxValue);
        }

    }

    public void ProjectPoints(Camera cam)
    {
        projectedPoints = cam.GetProjection2(pts,out distance);
    }

  
    public override void Draw(Graphics g, Camera cam)
    {
        int[] indices = Enumerable.Range(0, distance.Length).ToArray();
        Array.Sort(indices, (a, b) => distance[b].CompareTo(distance[a]));
        for (int i = 0; i < indices.Length; i++)
        {
            PointF[] face = new PointF[3];
            for (int j = 0; j < 3; j++)
            {
                int pointIndex = indices[i] * 3 + j;
                face[j] = projectedPoints[pointIndex];
                //Console.WriteLine(face[j].ToString());
            }

            if (face[0] == new PointF(float.MaxValue, float.MaxValue) ||
                face[1] == new PointF(float.MaxValue, float.MaxValue) ||
                face[2] == new PointF(float.MaxValue, float.MaxValue))
            {
                continue;
            }
            
            if (distance[indices[i]] != float.MaxValue)
            {
                face[0].X += 400;
                face[0].Y += 200;
                face[1].X += 400;
                face[1].Y += 200;
                face[2].X += 400;
                face[2].Y += 200;
                SolidBrush brush;
                if (haveColor)
                { 
                  brush = new SolidBrush(faceColor[indices[i]]); 
                }
                else
                {
                  brush = new SolidBrush(Color.Red);
                }
                
                //Console.WriteLine(clr);
                g.FillPolygon(brush, face);

                g.DrawPolygon(new Pen(lineColor,0.1f), face);
            }
            else
            {
                continue;
            }
            //    face[0].X += 400;
            //    face[0].Y += 200;
            //    face[1].X += 400;
            //    face[1].Y += 200;
            //    face[2].X += 400;
            //    face[2].Y += 200;
            //    //if (faceColor.Length > i)
            //    //{}
            //    //SolidBrush clr = new SolidBrush(faceColor[i]);
            //    SolidBrush brush = new SolidBrush(Color.Blue);
            //    //Console.WriteLine(clr);
            //    g.FillPolygon(brush, face);

            //    g.DrawPolygon(new Pen(lineColor), face);
            //}
        }
    }
}