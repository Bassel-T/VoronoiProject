using System.Text.Json;
using System.Text.Json.Serialization;
using VoronoiProject.Services;

namespace VoronoiProject.Models {

    /// <summary>
    /// As simple object with x and y coordinates
    /// </summary>
    public class Point {
		public double X { get; set; } = 0;
		public double Y { get; set; } = 0;

		public override bool Equals(object? obj) {
			if (obj == null)
				return false;

			if (obj.GetType() != typeof(Point)) {
				return false;
			}

			Point other = (Point)obj;
			return X == other.X && Y == other.Y;
		}
    }
}
