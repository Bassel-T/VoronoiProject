namespace VoronoiProject.Models {
	/// <summary>
	/// Doubly-Connected Edge List
	/// </summary>
	public class DCEL {

		// List of Voronoi Points in the DCEL
		public List<Point> VoronoiPoints { get; set; } = new List<Point>();

		// List of Edges in the DCEL
		public List<Edge> Edges { get; set; } = new List<Edge>();

		// List of Faces in the DCEL
		public List<Point> InputPoints { get; set; } = new List<Point>();

	}
}
