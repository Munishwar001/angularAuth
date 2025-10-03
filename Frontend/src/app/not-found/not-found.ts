import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AnimationOptions, LottieComponent } from 'ngx-lottie'

@Component({
  selector: 'app-not-found',
  imports: [RouterLink , LottieComponent],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css'
})
export class NotFound {
  
   options: AnimationOptions = {
    path: '404.json',
  };
}
