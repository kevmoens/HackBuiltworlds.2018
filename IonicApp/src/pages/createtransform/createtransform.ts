import { Component } from '@angular/core';
import { NavController, AlertController } from 'ionic-angular';
import { ApiProvider } from '../../providers/api/api';
import { Transform } from '../../models/api';

@Component({
  selector: 'page-createtransform',
  templateUrl: 'createtransform.html'
})
export class CreateTransformPage {
transformName:string;
transformDescription:string;

  
  constructor(public navCtrl: NavController, public alertCtrl: AlertController, public api: ApiProvider) {

  }

  onFunctionChange() {

  }

  createTransform() {
    const transform:Transform = new Transform();
    transform.name = this.transformName;
    transform.description = this.transformDescription;
    
    this.api.CreateTransform(transform).then(response => {
      response
    });
    let alert = this.alertCtrl.create({
      title: 'Success',
      subTitle: 'Transform Submitted',
      message: 'Your new transform template has been saved.',
      buttons: ['OK'],
      cssClass: 'profalert'
    });
    
    alert.present();
  }

  uploadClick() {

  }
}
