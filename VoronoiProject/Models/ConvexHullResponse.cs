namespace VoronoiProject.Models {
	public class ConvexHullResponse {

		// The points on the Convex Hull
		public List<ConvexHullPoints> Points { get; set; } = new List<ConvexHullPoints>();

		// The far endpoint of the lower bridge
		public int LowerBridgeIndex { get; set; }

		// The far endpoint of the upper bridge
		public int UpperBridgeIndex { get; set; }
	}
}
