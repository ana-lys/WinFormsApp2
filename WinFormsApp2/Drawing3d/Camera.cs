using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace YLScsDrawing.Drawing3d
{ 
    //orintation vector (0,0,-1)
    public class Camera
    {
        public 
        Point3d loc = new Point3d(0, 0, 0);
        double _d = 110.0;
        Quaternion quan = new Quaternion(1, 0, 0, 0);

        public Point3d Location
        {
            set { loc = value; }
            get { return loc; }
        }

        public double FocalDistance
        {
            set { _d = value; }
            get { return _d; }
        }

        public Quaternion Quaternion
        {
            set { quan = value; }
            get { return quan; }
        }

        public void MoveRight(double d)
        {
            loc.X += d;
        }

        public void MoveLeft(double d)
        {
            loc.X -= d;
        }

        public void MoveUp(double d)
        {
            loc.Y -= d;
        }

        public void MoveDown(double d)
        {
            loc.Y += d;
        }

        public void MoveIn(double d)
        {
            loc.Z += d;
        }

        public void MoveOut(double d)
        {
            loc.Z -= d;
        }

        public void Roll(int degree) // rotate around Z axis
        {
            Quaternion q = new Quaternion();
            q.FromAxisAngle(new Vector3d(0, 0, 1), degree * Math.PI / 180.0);
            quan = q * quan;
        }

        public void Yaw(int degree)  // rotate around Y axis
        {
            Quaternion q = new Quaternion();
            q.FromAxisAngle(new Vector3d(0, 1, 0), degree * Math.PI / 180.0);
            quan = q * quan;
        }

        public void Pitch(int degree) // rotate around X axis
        {
            Quaternion q = new Quaternion();
            q.FromAxisAngle(new Vector3d(1, 0, 0), degree * Math.PI / 180.0);
            quan = q * quan;
        }

        public void TurnUp(int degree)
        {
            Pitch(-degree);
        }

        public void TurnDown(int degree)
        {
            Pitch(degree);
        }

        public void TurnLeft(int degree)
        {
            Yaw(degree);
        }

        public void TurnRight(int degree)
        {
            Yaw(-degree);
        }

        public PointF[] GetProjection(Point3d[] pts)
        {


            PointF[] pt2ds = new PointF[pts.Length];

            // transform to new coordinates system which origin is camera location
            Point3d[] pts1 = Point3d.Copy(pts);
            Point3d.Offset(pts1, -loc.X, -loc.Y, -loc.Z);

            // rotate
            quan.Rotate(pts1);

     
            //project
            for (int i = 0; i < pts.Length; i++)
            {
                if (pts1[i].Z > 0.1)
                {
                    pt2ds[i] = new PointF((float)(pts1[i].X * _d / pts1[i].Z),
                        (float)(pts1[i].Y * _d / pts1[i].Z));
                }
                else
                {
                    pt2ds[i] = new PointF(float.MaxValue, float.MaxValue);
                }
            }
            return pt2ds;

        }

        public PointF[] GetProjection2(Point3d[] pts ,  out float[] distance )
        {
            PointF[] pt2ds = new PointF[pts.Length];
            distance = new float[pts.Length/3];
            // transform to new coordinates system which origin is camera location
            Point3d[] pts1 = Point3d.Copy(pts);
            Point3d.Offset(pts1, -loc.X, -loc.Y, -loc.Z);
            // rotate
            quan.Rotate(pts1);

            for (int i = 0; i < pts1.Length; i += 3)
            {
                float value = (float)(pts1[i].Z + pts1[i + 1].Z + pts1[i + 2].Z) / 3.0f;
                if (value < 0)
                    distance[i / 3] = float.MaxValue;
                else
                    distance[i / 3] = value;
             }
            //project
            for (int i = 0; i < pts.Length; i++)
            {
                if (pts1[i].Z > 0.1)
                {
                    pt2ds[i] = new PointF((float)(pts1[i].X * _d / pts1[i].Z),
                        (float)(pts1[i].Y * _d / pts1[i].Z));
                }
                else
                {
                    pt2ds[i] = new PointF(float.MaxValue, float.MaxValue);
                }
            }
            return pt2ds;
        }

    }
}
