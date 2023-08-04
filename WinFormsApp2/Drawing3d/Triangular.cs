using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Drawing;
using YLScsDrawing.Drawing3d;

public class TriangularMesh : Shape3d
{
    private PointF[] projectedPoints;
    private bool[] visible;
    private List<Point3d[]> faces ;
    private Color[] faceColor = new Color[0];

    public TriangularMesh(Point3d[] vertices)
    {
        //Console.WriteLine(vertices.Length);
        if (vertices.Length % 3 != 0)
        {
            throw new ArgumentException("The number of vertices must be divisible by 3.");
        }

        pts = vertices;
        projectedPoints = new PointF[vertices.Length];
        visible = new bool[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            projectedPoints[i] = new PointF(float.MaxValue, float.MaxValue);
        }

    }

    public void ProjectPoints(Camera cam)
    {
        for (int i = 0; i < pts.Length; i += 3)
        {
            Vector3d a = new Vector3d (pts[i],pts[i+1]);
            Vector3d b = new Vector3d (pts[i],pts[i + 2]);
            Vector3d n = a.CrossProduct(b);
            Point3d unitCam = new Point3d(0, 0, -1);
            cam.Quaternion.Rotate(unitCam);
            Console.WriteLine(unitCam.X+" "+unitCam.Y+" "+unitCam.Z);
            if (n.DotProduct(new Vector3d(unitCam)) > 0)
            {
                visible[i/3] = true;
            }
            else
            {
                visible[i/3] = false;
            }
        }
        projectedPoints = cam.GetProjection(pts);
    }

    public Color[] FaceColorArray
    {
        set
        {
            int n = Math.Min(value.Length, faces.Count);
            faceColor = new Color[n];
            for (int i = 0; i < n; i++)
            {
                faceColor[i] = value[i];
            }
        }
        get { return faceColor; }
    }

  
    public override void Draw(Graphics g, Camera cam)
    {
        for (int i = 0; i < projectedPoints.Length/3; i++)
        {
            PointF[] face = new PointF[3];
            for (int j = 0; j < 3; j++)
            {
                int pointIndex = i * 3 + j;
                face[j] = projectedPoints[pointIndex];
                //Console.WriteLine(face[j].ToString());
            }

            if (face[0] == new PointF(float.MaxValue, float.MaxValue) ||
                face[1] == new PointF(float.MaxValue, float.MaxValue) ||
                face[2] == new PointF(float.MaxValue, float.MaxValue))
            {
                continue;
            }
            //Console.WriteLine(visible[i]);
            if (visible[i])
            {
                face[0].X += 400;
                face[0].Y += 200;
                face[1].X += 400;
                face[1].Y += 200;
                face[2].X += 400;
                face[2].Y += 200;
                //if (faceColor.Length > i)
                //{}
                //SolidBrush clr = new SolidBrush(faceColor[i]);
                SolidBrush brush = new SolidBrush(Color.Red);
                //Console.WriteLine(clr);
                g.FillPolygon(brush, face);
                
                g.DrawPolygon(new Pen(lineColor), face);
            }
        }
    }
}