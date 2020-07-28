﻿using System;
namespace FredrickTechDemo.FredsMath
{
    public struct Vector3F
    {
        public float x, y, z;

        //constructors
        /*public Vector3F()
        {
            this.x = 0;
            this.y = 0;
            this.z = 0;
        }*/
        public Vector3F(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3F(float w)
        {
            this.x = this.y = this.z = w;
        }
        public Vector3F(Vector3F copyVector)
        {
            this.x = copyVector.x;
            this.y = copyVector.y;
            this.z = copyVector.z;
        }

        //vector vector operators
        public static Vector3F operator + (Vector3F a, Vector3F b)
        {
            return new Vector3F(a.x+b.x,a.y+b.y,a.z+b.z);
        }
        public static Vector3F operator - (Vector3F a, Vector3F b)
        {
            return new Vector3F(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static Vector3F operator * (Vector3F a, Vector3F b)
        {
            return new Vector3F(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        //Float operators, Post
        public static Vector3F operator + (Vector3F a, float b)
        {
            return new Vector3F(a.x + b, a.y + b, a.z + b);
        }

        public static Vector3F operator - (Vector3F a, float b)
        {
            return new Vector3F(a.x - b, a.y - b, a.z - b);
        }

        public static Vector3F operator * (Vector3F a, float b)
        {
            return new Vector3F(a.x * b, a.y * b, a.z * b);
        }

        //Float operators, Pre
        public static Vector3F operator + (float b, Vector3F a)
        {
            return new Vector3F(a.x + b, a.y + b, a.z + b);
        }

        public static Vector3F operator - (float b, Vector3F a)
        {
            return new Vector3F(a.x - b, a.y - b, a.z - b);
        }

        public static Vector3F operator * (float b, Vector3F a)
        {
            return new Vector3F(a.x * b, a.y * b, a.z * b);
        }

        //matrix vector operators
        public static Vector3F operator * (Matrix3F mat, Vector3F vec)//row major? (originates from aie tests)
        {
            return new Vector3F(
                    /*X*/mat.m1 * vec.x + mat.m4 * vec.y + mat.m7 * vec.z,
                    /*Y*/mat.m2 * vec.x + mat.m5 * vec.y + mat.m8 * vec.z,
                    /*Z*/mat.m3 * vec.x + mat.m6 * vec.y + mat.m9 * vec.z);
        }

        public static Vector3F operator *(Vector3F vec, Matrix4F mat)//row major? (originates from aie tests)
        {
            return new Vector3F(
                    /*X*/mat.m1 * vec.x + mat.m5 * vec.y + mat.m9 * vec.z,
                    /*Y*/mat.m2 * vec.x + mat.m6 * vec.y + mat.m10 * vec.z,
                    /*Z*/mat.m3 * vec.x + mat.m7 * vec.y + mat.m11 * vec.z);
        }
        //funcs
        public float Dot(Vector3F vec)
        {
            return this.x * vec.x + this.y * vec.y + this.z * vec.z;
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }

        public void Normalize()
        {
            float length = this.Magnitude();
            if (length != 0)
            {
                this.x /= length;
                this.y /= length;
                this.z /= length;
            }
            else
            {
                this.x = this.y = this.z = 0;
            }
        }
        
        public static float magnitude(Vector3F vec)
        {
            return (float)Math.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
        }
        public Vector3F Cross(Vector3F vec)
        {
            return new Vector3F(this.y * vec.z - this.z * vec.y, this.z * vec.x - this.x * vec.z, this.x * vec.y - this.y * vec.x);
        }
        public static Vector3F normalize(Vector3F vec)
        {
            float length = magnitude(vec);
            if (length != 0)
            {
                vec.x /= length;
                vec.y /= length;
                vec.z /= length;
            }
            else
            {
                vec.x = vec.y = vec.z = 0;
            }
            return vec;
        }

        public static Vector3F cross(Vector3F vecA, Vector3F vecB)
        {
            return new Vector3F(vecA.y * vecB.z - vecA.z * vecB.y, vecA.z * vecB.x - vecA.x * vecB.z, vecA.x * vecB.y - vecA.y * vecB.x);
        }
        public static float dot(Vector3F vecA, Vector3F vecB)
        {
            return vecA.x * vecB.x + vecA.y * vecB.y + vecA.z * vecB.z;
        }
    }
}
