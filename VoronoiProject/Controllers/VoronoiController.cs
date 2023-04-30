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

			// Read the input file
			var reader = new StreamReader(file.OpenReadStream());
			var points = new List<Point>();
			var regex = new Regex(@"\d+(\.\d+)?");
			while (!reader.EndOfStream) {
				var line = reader.ReadLine();
				var matches = regex.Matches(line ?? "");
				if (matches.Count != 2) {
					return BadRequest("Cannot find numbers in line. Two numbers per line in the file.");
				}

				points.Add(new Point() {
					X = double.Parse(matches[0].Value),
					Y = double.Parse(matches[1].Value)
				});
			}

			// Pre-processing sorts points by x coordinate. Uses stable quick-sort, O(n log n)
			points.Sort((x, y) => x.X.CompareTo(y.X) == 0 ? x.Y.CompareTo(y.Y) : x.X.CompareTo(y.X));

			// Call recursive function
			var voronoi = await RecursiveVoronoi(points);

			// The algorithm is done, this is just to make the frontend visible
			voronoi.Edges.ForEach(edge => {
				if (edge.Start == null
				&& edge.End != null
				&& edge.Angle > Math.PI
				&& edge.Angle <= 2 * Math.PI
				&& edge.End.Y <= edge.Point1.Y
				&& edge.End.Y <= edge.Point2.Y) {
					// Angle is negative but shouldn't be
					edge.Angle -= Math.PI;
				}
			});

			return Ok(voronoi);
		}

		public async Task<DCEL> RecursiveVoronoi(IEnumerable<Point> points) {
			_logger.LogInformation($"Recursing the voronoi with {points.Count()} points");

			// Base Case
			if (points.Count() == 1) {
				// Base case, return DCEL with one face
				return new DCEL() { InputPoints = new List<Point>() { points.First() } };
			}

			// Recursion
			var length = points.Count();
			var leftHalf = points.Take(length / 2);
			var rightHalf = points.Skip(length / 2);

			var leftVoronoi = await RecursiveVoronoi(leftHalf);
			var rightVoronoi = await RecursiveVoronoi(rightHalf);

			// Merge
			return MergeVoronoi(leftVoronoi, rightVoronoi);
		}

		private DCEL MergeVoronoi(DCEL left, DCEL right) {
			// Overall convex hull -- O(n)
			_logger.LogInformation("Generating overall convex hull");
			var convexHull = GenerateConvexHull(left, true, right);

			_logger.LogInformation("Generating smaller hulls");
			// Individual convex hulls -- O(n)
			var leftHull = GenerateConvexHull(left, true);
			var rightHull = GenerateConvexHull(right, false);

			// Collect the points on the monotone chain
			var chainPoints = new List<Point>();
			var chainEdges = new List<Edge>();

			// Find corresponding index of upper bridge in left/right hulls
			var leftIndex = leftHull.Points.IndexOf(convexHull.Points[convexHull.UpperBridgeIndex % convexHull.Points.Count]);
			var rightIndex = rightHull.Points.IndexOf(convexHull.Points[convexHull.UpperBridgeIndex % convexHull.Points.Count == 0 ? convexHull.Points.Count - 1 : convexHull.UpperBridgeIndex - 1]);

			_logger.LogInformation($"Hull indices are {leftIndex}, {rightIndex}");

			// Upper bridge, will be traversing
			var current = new Edge {
				Start = convexHull.Points[convexHull.UpperBridgeIndex == 0 ? convexHull.Points.Count - 1 : convexHull.UpperBridgeIndex - 1],
				End = convexHull.Points[convexHull.UpperBridgeIndex % convexHull.Points.Count % convexHull.Points.Count]
			};

			current.Angle = Math.Atan2(current.End.Y - current.Start.Y, current.End.X - current.Start.X);

			var lowerBridge = new Edge {
				Start = convexHull.Points[convexHull.LowerBridgeIndex == 0 ? convexHull.Points.Count - 1 : convexHull.LowerBridgeIndex - 1],
				End = convexHull.Points[convexHull.LowerBridgeIndex % convexHull.Points.Count % convexHull.Points.Count]
			};

			var previousBridge = current;

			lowerBridge.Angle = Math.Atan2(lowerBridge.End.Y - lowerBridge.Start.Y, lowerBridge.End.X - lowerBridge.Start.X);

			if (current.Equals(lowerBridge)) {
				// Default case
				_logger.LogInformation("Generating DCEL for two points");
				var bisect = Bisector(current);
				return new DCEL {
					InputPoints = left.InputPoints.Union(right.InputPoints).ToList(),
					Edges = new List<Edge> {
						bisect
					},
					VoronoiPoints = new List<Point>()
				};
			}

			_logger.LogInformation("Entering loop");

			// Keeping track of the latest intersection
			var lastIntersection = new Point { X = 0, Y = double.MaxValue };
			
			// Get the bisector
			var bisector = Bisector(current);

			while (!previousBridge.Equals(lowerBridge)) {

				// Get all the edges we want to compare intersections with
				var edges = left.Edges.Where(e => e.Point1.Equals(current.Start) || e.Point2.Equals(current.Start)
											|| e.Point1.Equals(current.End) || e.Point2.Equals(current.End))
							.Union(right.Edges.Where(e => e.Point1.Equals(current.Start) || e.Point2.Equals(current.Start)
											|| e.Point1.Equals(current.End) || e.Point2.Equals(current.End)))
							.ToList();

				// Because the point is necessarily coming from above, we want the maximum y coordinate for intersections
				edges.ForEach(edge => edge.IntersectY = CalculateIntersectionY(bisector, edge));

				var firstIntersection = edges.Where(edge => edge.IntersectY < lastIntersection.Y - 1e-4)
											.MaxBy(edge => edge.IntersectY);

				if (firstIntersection == null) {
					_logger.LogInformation("No more intersections found");
					// No more intersections. Likely bottom end
					bisector.Point1 = current.Start;
					bisector.Point2 = current.End;
					chainEdges.Add(bisector);
					break;
				}

				_logger.LogInformation($"Intersection found! Angle = {firstIntersection.Angle}, Point1 = {firstIntersection.Point1}, End = {firstIntersection.Point2}");

				var firstInterpoint = firstIntersection.Start ?? firstIntersection.Midpoint;

				firstIntersection.IntersectX = firstInterpoint.X + (firstIntersection.IntersectY - firstInterpoint.Y)
																	/ Math.Tan(firstIntersection.Angle.Value);

				if (Math.Ceiling(firstIntersection.Angle.Value / Math.PI) - firstIntersection.Angle - Math.PI < 1e-4) {
					_logger.LogInformation("Angle is multiple of pi");
					firstIntersection.IntersectX = bisector.Midpoint.X + (firstIntersection.IntersectY - bisector.Midpoint.Y)
																/ Math.Tan(bisector.Angle.Value);
				}

				lastIntersection = new Point {
					X = firstIntersection.IntersectX,
					Y = firstIntersection.IntersectY
				};

				bisector.End = new Point {
					X = firstIntersection.IntersectX,
					Y = firstIntersection.IntersectY
				};

				chainEdges.Add(bisector);

				_logger.LogInformation("Finding existing data in sub-diagrams");

				// Checking partial edges that have endpoint on other side
				var edgeIndexInLeftHalf = left.VoronoiPoints.IndexOf(firstIntersection.End);
				var edgeInRightHalf = right.VoronoiPoints.IndexOf(firstIntersection.End);

				if (firstIntersection.End != null) {
					if (edgeIndexInLeftHalf > -1) {
						left.VoronoiPoints.RemoveAt(edgeIndexInLeftHalf);
					} else if (edgeInRightHalf > -1) {
						right.VoronoiPoints.RemoveAt(edgeInRightHalf);
					}
				}

				if (firstIntersection.End == null)
					firstIntersection.End = lastIntersection;
				else if (firstIntersection.Start == null)
					firstIntersection.Start = lastIntersection;

				chainPoints.Add(lastIntersection);

				// Traverse
				if (left.Edges.Contains(firstIntersection)) {
					leftIndex = (leftIndex - 1);
					if (leftIndex == -1) { leftIndex = leftHull.Points.Count - 1; }
				} else {
					rightIndex = (rightIndex + 1) % rightHull.Points.Count;
				}

				_logger.LogInformation($"Iterating! L = {leftIndex}, R = {rightIndex}");

				previousBridge = current;
				current = new Edge {
					Start = leftHull.Points[leftIndex],
					End = rightHull.Points[rightIndex]
				};

				current.Angle = Math.Atan2(current.End.Y - current.Start.Y, current.End.X - current.Start.X);

				bisector = Bisector(current);
				bisector.Start = lastIntersection;
			}

			var toReturn = new DCEL() {
				InputPoints = left.InputPoints.Union(right.InputPoints).ToList(),
				Edges = left.Edges.Union(chainEdges).Union(right.Edges).ToList(),
				VoronoiPoints = left.VoronoiPoints.Union(chainPoints).Union(right.VoronoiPoints).ToList()
			};

			return toReturn;
		}

		/// <summary>
		/// Generate the upper and lower convex hulls via graham scan
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		private ConvexHullResponse GenerateConvexHull(DCEL left, bool storeLeft, DCEL? right = null) {

			var lowerBridge = 0;
			var upperBridge = 0;

			// Get the input points with the flag
			var inputs = left.InputPoints.Select(point => new ConvexHullPoints {
				X = point.X,
				Y = point.Y,
				InputPoint = point,
				IsLeftHalf = storeLeft
			}).Union(right?.InputPoints?.Select(point => new ConvexHullPoints {
				X = point.X,
				Y = point.Y,
				InputPoint = point,
				IsLeftHalf = false
			}) ?? new List<ConvexHullPoints>()).ToList();

			if (inputs.Count < 3) {
				return new ConvexHullResponse {
					LowerBridgeIndex = 1,
					UpperBridgeIndex = 1,
					Points = inputs
				};
			}

			// These are sorted by x coordinate because of the initial divide and conquer
			var lowerHull = new List<ConvexHullPoints>() {
				inputs[0],
				inputs[1]
			};

			for (int i = 2; i < inputs.Count; i++) {
				var latest = inputs[i];

				// While it's a right turn, pop
				while (lowerHull.Count > 1 && Area(lowerHull[lowerHull.Count - 2], lowerHull.Last(), latest) < 0) {
					lowerHull.RemoveAt(lowerHull.Count - 1);
				}

				lowerHull.Add(latest);

				if (lowerHull.Last().IsLeftHalf != lowerHull[lowerHull.Count - 2].IsLeftHalf) {
					lowerBridge = lowerHull.Count - 1;
				}
			}

			var upperHull = new List<ConvexHullPoints>() {
				inputs[inputs.Count - 1],
				inputs[inputs.Count - 2]
			};

			for (int i = inputs.Count - 3; i >= 0; i--) {
				var latest = inputs[i];

				// While it's a right turn, pop
				while (upperHull.Count > 1 && Area(upperHull[upperHull.Count - 2], upperHull.Last(), latest) < 0) {
					upperHull.RemoveAt(upperHull.Count - 1);
				}

				upperHull.Add(latest);

				if (upperHull.Last().IsLeftHalf != upperHull[upperHull.Count - 2].IsLeftHalf) {
					upperBridge = upperHull.Count + lowerHull.Count - 2;
				}
			}

			lowerHull.RemoveAt(lowerHull.Count - 1);
			lowerHull.AddRange(upperHull);
			lowerHull.RemoveAt(lowerHull.Count - 1);
			return new ConvexHullResponse {
				Points = lowerHull,
				LowerBridgeIndex = lowerBridge,
				UpperBridgeIndex = upperBridge
			};
		}

		private Edge Bisector(Edge edge, Point? start = null) {
			// Get angle, force to face downward
			var angle = Math.Atan2(edge.End.Y - edge.Start.Y, edge.End.X - edge.Start.X);
			angle += Math.PI / 2.0;

			while (angle < Math.PI)
				angle += Math.PI;

			while (angle >= Math.PI * 2)
				angle -= Math.PI;

			var midpoint = new Point {
				X = (edge.Start.X + edge.End.X) / 2.0,
				Y = (edge.Start.Y + edge.End.Y) / 2.0
			};

			return new Edge {
				Angle = angle,
				Midpoint = midpoint,
				Point1 = edge.Start,
				Point2 = edge.End,
				Start = start ?? null
			};
		}

		private double Area(Point p1, Point p2, Point p3) {
			// Positive if counter-clockwise, negative if clockwise, based on matrix
			return 0.5 * (p1.X * (p2.Y - p3.Y) + p1.Y * (p3.X - p2.X) + p2.X * p3.Y - p2.Y * p3.X);
		}

		private double CalculateIntersectionY(Edge source, Edge target) {
			var targ = target.Start ?? target.Midpoint;
			var start = source.Start ?? source.Midpoint;

			// If the angles match but are not the same line, return negative infinity
			if (source.Angle == target.Angle && !start.Equals(targ)) { return double.NegativeInfinity; }

			// Horizontals break the formula
			if (source.Angle == Math.PI) { return start.Y; }
			if (target.Angle == Math.PI) { return targ.Y; }

			if (targ == null) { return double.NegativeInfinity; }

			var sourceArctan = 1.0 / Math.Tan(source.Angle.Value);
			var targetArctan = 1.0 / Math.Tan(target.Angle.Value);

			return (start.X - targ.X + targ.Y * targetArctan - start.Y * sourceArctan)
					/ (targetArctan - sourceArctan);
		}
	}
}