using Microsoft.AspNetCore.Mvc;
using VoronoiProject.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace VoronoiProject.Controllers {
	[ApiController]
	[Route("[controller]")]
	public class VoronoiController : ControllerBase {

		private readonly ILogger<VoronoiController> _logger;

		public VoronoiController(ILogger<VoronoiController> logger) {
			_logger = logger;
		}

		[HttpPost]
		public async Task<ActionResult> GenerateVoronoi([FromForm] IFormFile file) {

			if (file == null) {
				return BadRequest("Must attach a file");
			}

			var reader = new StreamReader(file.OpenReadStream());
			var points = new List<Point>();
			var regex = new Regex(@"\d+(\.\d+)?");
			while (!reader.EndOfStream) {
				var line = reader.ReadLine();
				var matches = regex.Matches(line);
				if (matches.Count != 2) {
					return BadRequest("Cannot find numbers in line. Two numbers per line in the file.");
				}

				points.Add(new Point() {
					X = double.Parse(matches[0].Value),
					Y = double.Parse(matches[1].Value)
				});
			}

			// Pre-processing sorts points by x coordinate. Uses stable quick-sort, O(n log n)
			var sorted = points.OrderBy(point => point.X);

			// Call recursive function
			var voronoi = await RecursiveVoronoi(sorted);

			return Ok(points);
		}

		public async Task<DCEL> RecursiveVoronoi(IEnumerable<Point> points) {
			return await Task.FromResult(new DCEL());
		}
	}
}