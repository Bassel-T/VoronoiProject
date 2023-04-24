namespace VoronoiProject.Models {
	/// <summary>
	/// A very simple Face model, storing the first edge that contains it
	/// </summary>
	public class Face {
		public Edge? Edge { get; set; } = null;
	}
}
