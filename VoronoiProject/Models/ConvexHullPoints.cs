using Newtonsoft.Json;
using VoronoiProject.Services;

namespace VoronoiProject.Models {

	/// <summary>
	/// An input point with a flag associated with it to indicate which half
	/// </summary>
	public class ConvexHullPoints : Point {

        // The input point associated with the convex hull input
        [JsonConverter(typeof(PointConverter))]
        public Point InputPoint { get; set; }
		
		// Set if the object is on the left half of the convex hull, used for creating bridge
		public bool IsLeftHalf { get; set; }

		public override bool Equals(object? obj) {
			if (obj.GetType().BaseType != typeof(Point))
				return false;

			Point p = obj as Point;
			return X == p.X && Y == p.Y;
		}
	}
}
