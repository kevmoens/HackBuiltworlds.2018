import { Component } from '@angular/core';
import { NavController, AlertController } from 'ionic-angular';
import { ApiProvider } from '../../providers/api/api';

@Component({
  selector: 'page-ratetransform',
  templateUrl: 'ratetransform.html'
})
export class RateTransformPage {

  criterias;
  constructor(public navCtrl: NavController, 
    public api: ApiProvider, public alertCtrl: AlertController) {
    
  }

  ngOnInit() {
    console.log('ngOnInit hit')
    this.api.GetCriteria().then(criteriaList => {
      this.criterias = criteriaList;
      console.log(this.criterias);
    }).catch(err => {
      console.log('Error calling GetCriteria: ' + err.Error);
    })
  }

  onFunctionChange() {

  }

  rateTransformation() {
    let alert = this.alertCtrl.create({
      title: 'Success',
      subTitle: 'Rating Complete',
      message: 'Your feedback has been saved.',
      buttons: ['OK'],
      cssClass: 'profalert'
    });
    
    alert.present();
  }
}
