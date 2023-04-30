namespace VoronoiProject.Models {
	
	/// <summary>
	/// Represents an edge in the DCEL, connects two points to one another
	/// </summary>
	public class Edge {
		// Endpoints 
		public Point? Start { get; set; }
		public Point? End { get; set; }

		// For infinite rays
		public double? Angle { get; set; }

		// Temporary, for when there are only two input points
		public Point? Midpoint { get; set; }

		// The two "faces" it separates
		public Point? Point1 { get; set; }
		public Point? Point2 { get; set; }

		// Data about the intersection position
		public double IntersectX { get; set; }
		public double IntersectY { get; set; }

		public override bool Equals(object? obj) {
			if (obj?.GetType() == typeof(Edge)) {
				
				Edge other = (Edge)obj;
				var equal = true;
				if (Start != null && End != null) {
					equal = Start.Equals(other.Start) && End.Equals(other.End);
					equal |= Start.Equals(other.End) && End.Equals(other.Start);
				} else if (Start != null && Angle != null)
					equal = Start.Equals(other.Start) && Angle == other.Angle;
				else if (Midpoint != null && Angle != null)
					equal = Midpoint.Equals(other.Midpoint) && Angle == other.Angle;

				return equal;
			}

			return false;
		}
	}
}
