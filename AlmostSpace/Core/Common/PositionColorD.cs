using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlmostSpace.Core.Common
{
    // An extremely simplified version of VertexPositionColor using Double vectors
    public struct PositionColorD
    {
        public Vector2D Position;
        public Color Color;

        // Constructs a new PositionColorD from the given vector and color
        public PositionColorD(Vector2D Position, Color Color)
        {
            this.Position = Position;
            this.Color = Color;
        }

        // Returns an array of VertexPositionColor objects to be drawn to the screen based on the given array of
        // PositionColorD objects and transform.
        public static VertexPositionColor[] getVertexPositionColorArr(PositionColorD[] points, Matrix transform)
        {
            VertexPositionColor[] output = new VertexPositionColor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector2D newPoint = points[i].Position.Transform(transform);
                output[i] = new VertexPositionColor(new Vector3((float)newPoint.X, (float)newPoint.Y, 0), points[i].Color);
            }
            return output;
        }
    }

}
