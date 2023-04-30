import { HttpClient, HttpHandler, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { Inject } from '@angular/core';
import { Component } from '@angular/core';
import { DCEL, Edge } from "../Models/Models";
import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import * as ChartJs from "chart.js";

ChartJs.Chart.register(...ChartJs.registerables);

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  public http: HttpClient;
  public base: string;

  public downloadText: string;
  private plot: ChartJs.Chart;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.base = baseUrl;
    this.downloadText = '';
  }

  public upload(event): void {

    var file = event.target.files[0];

    const formData: FormData = new FormData();
    formData.append('file', file, file.name);

    this.generateVoronoi(formData);
  }

  generateVoronoi(data) {
    let options_: any = {
      body: data,
      observe: "response",
      responseType: "blob",
      headers: new HttpHeaders({
        "Accept": "application/json"
      })
    };

    this.submitRequest(data, options_).subscribe(x => {
      this.downloadText = '';

      var inputs = x.inputPoints.map(point => { return { x: point.x, y: point.y }; });
      var voronoi = x.voronoiPoints.map(point => { return { x: point.x, y: point.y }; });

      if (this.plot)
        this.plot.destroy();

      var drawEdges = {
        id: 'drawEdges',
        beforeDraw(chart) {
          x.edges.forEach(edge => {
            if (edge.start !== null && edge.end !== null) {
              drawLine(edge.start.x, edge.start.y, edge.end.x, edge.end.y);
            } else if (edge.start !== null) {
              drawAngle(edge.start.x, edge.start.y, edge.angle + Math.PI / 2.0);
            } else if (edge.end !== null) {
              drawAngle(edge.end.x, edge.end.y, edge.angle + Math.PI / 2.0);
            } else {
              drawAngle(edge.midpoint.x, edge.midpoint.y, edge.angle + Math.PI / 2.0);
              drawAngle(edge.midpoint.x, edge.midpoint.y, edge.angle - Math.PI / 2.0);
            }
          });

          function drawLine(x1, y1, x2, y2) {
            const yScale = chart.scales['y'];
            const xScale = chart.scales['x'];
            const ctx = chart.ctx;
            const yStart = yScale.getPixelForValue(y1);
            const xStart = xScale.getPixelForValue(x1);
            const xEnd = xScale.getPixelForValue(x2);
            const yEnd = yScale.getPixelForValue(y2);
            ctx.save();
            ctx.beginPath();
            ctx.moveTo(xStart, yStart);
            ctx.lineTo(xEnd, yEnd);
            ctx.setLineDash([5, 5]);
            ctx.strokeStyle = 'blue';
            ctx.lineWidth = 1;
            ctx.stroke();
            ctx.restore();
          }

          function drawAngle(x1, y1, angle) {
            const yScale = chart.scales['y'];
            const xScale = chart.scales['x'];
            const ctx = chart.ctx;
            const yStart = yScale.getPixelForValue(y1);
            const xStart = xScale.getPixelForValue(x1);
            ctx.save();
            ctx.beginPath();
            ctx.moveTo(xStart, yStart);
            ctx.lineTo(xStart - 10000 * Math.cos(angle), yStart - 10000 * Math.sin(angle));
            ctx.setLineDash([5, 5]);
            ctx.strokeStyle = 'blue';
            ctx.lineWidth = 1;
            ctx.stroke();
            ctx.restore();
          }
        },
      };

      this.plot = new ChartJs.Chart((<HTMLCanvasElement>document.getElementById("plot")).getContext('2d'),
        {
          type: 'scatter',
          data: {
            datasets: [
              {
                data: inputs,
                backgroundColor: '#FF0000',
                label: 'Inputs'
              },
              {
                data: voronoi,
                backgroundColor: '#0000FF',
                label: 'Voronoi Vertices'
              }
            ]
          },
          plugins: [drawEdges]
        });

      x.edges.forEach(edge => {
        if (edge.start !== null && edge.end !== null) {

        }
      });
    });
  }

  submitRequest(data, options_): Observable<DCEL> {
    return this.http.post<DCEL>(this.base + 'voronoi', data);
  }

  blobToText(blob: any): Observable<string> {
    return new Observable<string>((observer: any) => {
      if (!blob) {
        observer.next("");
        observer.complete();
      } else {
        let reader = new FileReader();
        reader.onload = event => {
          observer.next((event.target as any).result);
          observer.complete();
        };
        reader.readAsText(blob);
      }
    });
  }

}
