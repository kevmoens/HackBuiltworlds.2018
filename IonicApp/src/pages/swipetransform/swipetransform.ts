import { Component } from '@angular/core';
import { NavController } from 'ionic-angular';
import { RateTransformPage } from '../ratetransform/ratetransform';
import { ApiProvider } from '../../providers/api/api';

@Component({
  selector: 'page-swipetransform',
  templateUrl: 'swipetransform.html'
})

export class SwipeTransformPage {
  imgList: String[] = [];
  transforms;

  constructor(public navCtrl: NavController, public api: ApiProvider) {
    this.imgList.push("/assets/imgs/floorplan.jpg")
    this.imgList.push("/assets/imgs/floorplan2.jpg")
    this.imgList.push("/assets/imgs/floorplan3.jpg")
    this.imgList.push("/assets/imgs/floorplan4.jpg")
    this.imgList.push("/assets/imgs/floorplan2.jpg")
    this.imgList.push("/assets/imgs/three-bedroom-floorplan.png")
    this.imgList.push("/assets/imgs/floorplan.jpg")
    this.imgList.push("/assets/imgs/floorplan2.jpg")
    this.imgList.push("/assets/imgs/floorplan3.jpg")
    this.imgList.push("/assets/imgs/floorplan4.jpg")
    this.imgList.push("/assets/imgs/floorplan2.jpg")
    this.imgList.push("/assets/imgs/three-bedroom-floorplan.png")
  }

  ngOnInit() {
    this.api.GetTransforms().then(transformList => {
      this.transforms = transformList;
      let index = 0
      for(let i=0; i<this.transforms.length; i++){
        console.log(this.transforms[i]); //use i instead of 0
        this.transforms[i]["img"] = this.imgList[index]
        if(this.transforms[i]["description"] == "undefined") {
          this.transforms[i]["description"] = "The best Floorplan";
        }
      }
    }).catch(err => {
      console.log('Error calling GetTransforms: ' + err.Error);
    })
  }

  ionViewDidLoad() {
    this.api.GetTransforms().then(transformList => {
      this.transforms = transformList;
      let index = 0
      for(let i=0; i<this.transforms.length; i++){
        console.log(this.transforms[i]); //use i instead of 0
        this.transforms[i]["img"] = this.imgList[index]
        if(this.transforms[i]["description"] == "undefined") {
          this.transforms[i]["description"] = "The best Floorplan";
        }
      }
    }).catch(err => {
      console.log('Error calling GetTransforms: ' + err.Error);
    })
  }

  imgClick(id) {
    console.log(id)
    this.navCtrl.push(RateTransformPage, {transformid: id });
  }
}
