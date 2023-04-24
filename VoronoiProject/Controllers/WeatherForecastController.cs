using Microsoft.AspNetCore.Mvc;
using VoronoiProject.Models;
using System.Linq;

namespace VoronoiProject.Controllers {
	[ApiController]
	[Route("api/v1/voronoi")]
	public class WeatherForecastController : ControllerBase {

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger) {
			_logger = logger;
		}

		[HttpPost("generate")]
		public async Task<ActionResult> GenerateVoronoi([FromBody] IEnumerable<Point> points) {

			if (points == null || points.Count() == 0) {
				return BadRequest("List of points must have at least one point");
			}

			// Pre-processing sorts points by x coordinate. Uses stable quick-sort, O(n log n)
			var sorted = points.OrderBy(point => point.X);

			// Call recursive function
			var voronoi = await RecursiveVoronoi(sorted);

			return Ok();
		}

		public async Task<DCEL> RecursiveVoronoi(IEnumerable<Point> points) {
			return await Task.FromResult(new DCEL());
		}
	}
}