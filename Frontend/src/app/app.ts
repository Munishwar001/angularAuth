// import { Component, signal } from '@angular/core';
// import { RouterOutlet } from '@angular/router';
// import { AnimationOptions ,LottieComponent } from 'ngx-lottie';

// @Component({
//   selector: 'app-root',
//   imports:[RouterOutlet , LottieComponent],
//   templateUrl: './app.html',
//   styleUrl: './app.css'

// })
// export class App {
//   protected readonly title = signal('Frontend');
//   options: AnimationOptions = {
//     path: '/assets/login.json', // 👈 animation file
//   };
// }
import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
// import { AnimationOptions, LottieComponent } from 'ngx-lottie';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    // LottieComponent   // 👈 add this so Angular knows <ng-lottie>
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  protected readonly title = signal('Frontend');

  // options: AnimationOptions = {
  //   path: 'login.json', // 👈 animation file
  // };
}
