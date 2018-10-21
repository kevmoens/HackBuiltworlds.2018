import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Http, URLSearchParams, Headers, RequestOptions, Response } from '@angular/http';
import { App, NavController } from 'ionic-angular';
import { Storage } from '@ionic/storage';
import { Transform } from '../../models/api';
import 'rxjs/Rx';
import { Observable } from 'rxjs/Observable';
import  'rxjs/add/operator/catch';
import  'rxjs/add/operator/map';

/*
  Generated class for the ApiProvider provider.

  See https://angular.io/guide/dependency-injection for more info on providers
  and Angular DI.
*/
@Injectable()
export class ApiProvider {

  url: string;
  constructor(public http: HttpClient) {
    console.log('Hello ApiProvider Provider');
    this.url = "http://localhost:8100/api/v1/";
  }

  GetTransforms() {
    return new Promise(resolve => {
      this.http.get(`${this.url}transform`).subscribe(data => {
        console.log(data)
        resolve(data["data"]);
      },
      err => {
        console.log(err);
      });
    });
  }

  GetCriteria() {
    return new Promise(resolve => {
      this.http.get(`${this.url}criteria`).subscribe(data => {
        resolve(data["data"])
      },
      err => {
        console.log(err);
      });
    });
  }

  CreateTransform(transform) {
    const body = {
      name: transform.Name,
      type_id: 1, //Interior
      description: transform.description,
    };

    return new Promise(resolve => {
      this.http.post(`${this.url}transform?name=${transform.name}&type_id=${body.type_id}&description=${body.description}`, transform, {})
      .subscribe(data => {
        resolve()
      },
      err => {
        console.log(err)
      });
    });
  }
}
