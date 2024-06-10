using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AlmostSpace.Core.Common
{
    // A simplified implementation of Vector2 making use of doubles for greater precision
    public struct Vector2D
    {
        public double X;
        public double Y;

        // Constructs a new Vector2D based on the given X and Y components
        public Vector2D(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        // Constructs an empty Vector2D
        public Vector2D()
        {
            X = 0;
            Y = 0;
        }

        // Checks if this Vector2D is equal to another
        public bool Equals(Vector2D other)
        {
            return other.X == X && other.Y == Y;
        }

        // Returns a string representing this Vector2D
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        // Returns the magnitude of this Vector2D
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        // Addition
        public static Vector2D Add(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2D operator +(Vector2D a, Vector2D b)
        {
            return Add(a, b);
        }

        // Subtraction
        public static Vector2D Subtract(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X - b.X, a.Y - b.Y);
        }

        public static Vector2D operator -(Vector2D a, Vector2D b)
        {
            return Subtract(a, b);
        }

        // Multiplication
        public static Vector2D Multiply(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X * b.X, a.Y * b.Y);
        }

        public static Vector2D Multiply(Vector2D a, double b)
        {
            return new Vector2D(a.X * b, a.Y * b);
        }

        public static Vector2D operator *(Vector2D a, Vector2D b)
        {
            return Multiply(a, b);
        }

        public static Vector2D operator *(Vector2D a, double b)
        {
            return Multiply(a, b);
        }

        // Division
        public static Vector2D Divide(Vector2D a, Vector2D b)
        {
            return new Vector2D(a.X / b.X, a.Y / b.Y);
        }

        public static Vector2D operator /(Vector2D a, Vector2D b)
        {
            return Divide(a, b);
        }

        // Returns a new Vector2D representing this Vector2D transformed by the given matrix
        public Vector2D Transform(Matrix matrix)
        {
            return new Vector2D(X * matrix.M11 + Y * matrix.M21 + matrix.M41, X * matrix.M12 + Y * matrix.M22 + matrix.M42);
        }

        // Returns a new Vector2D representing the given Vector2D transformed by the given matrix
        public static Vector2D Transform(Vector2D position, Matrix matrix)
        {
            return new Vector2D(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);
        }

        // Returns this Vector2D as a Vector2, reducing its precision
        public Vector2 getVector2()
        {
            return new Vector2((float)X, (float)Y);
        }

    }
}
