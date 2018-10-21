import { Component } from '@angular/core';

import { CreateTransformPage } from '../createtransform/createtransform';
import { SwipeTransformPage } from '../swipetransform/swipetransform';
import { HomePage } from '../home/home';

@Component({
  templateUrl: 'tabs.html'
})
export class TabsPage {

  tab1Root = HomePage;
  tab2Root = CreateTransformPage;
  tab3Root = SwipeTransformPage;

  constructor() {

  }
}
