namespace VoronoiProject.Models {
	
	/// <summary>
	/// Represents an edge in the DCEL, connects two points to one another
	/// </summary>
	public class Edge {
		// The endpoints 
		public Point? P1 { get; set; } = null;
		public Point? P2 { get; set; } = null;

		// For a point to infinity, save the angle of the edge
		public double? Angle1 { get; set; } = null;
		public double? Angle2 { get; set; } = null;

		// The next counter-clockwise edge to traverse
		public Edge? E1 { get; set; } = null;
		public Edge? E2 { get; set; } = null;

		// The left and right faces (voronoi cells) of the edge
		public Face? F1 { get; set; } = null;
		public Face? F2 { get; set; } = null;
	}
}
