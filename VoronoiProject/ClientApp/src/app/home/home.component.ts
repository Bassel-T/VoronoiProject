import { HttpClient, HttpHandler, HttpHeaders } from '@angular/common/http';
import { Inject } from '@angular/core';
import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  public http: HttpClient;
  public base: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.http = http;
    this.base = baseUrl;
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
    this.http.request("post", this.base + 'voronoi', options_).subscribe(x => {
      console.log("Test");
    });
  }

}
