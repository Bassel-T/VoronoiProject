export class Point {
  x: number;
  y: number;
}

export class Edge {
  // Endpoints of the edge
  public start: Point;
  public end: Point;

  // For single infinite edges
  public angle: number;
  public midpoint: Point;

  // Faces of the graph
  public point1: Point;
  public point2: Point;

  // Backend helpers
  public intersectX: number;
  public intersectY: number;
}

export class DCEL {
  voronoiPoints: Point[];
  inputPoints: Point[];
  edges: Edge[];
}
