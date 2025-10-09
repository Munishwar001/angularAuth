import { Component } from '@angular/core';
import { AuthApi } from '../../service/auth/auth-api';

@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class Profile {
   
  constructor(private AuthService:AuthApi){
    
  }
 
  ngOnInit(){
       console.log(this.AuthService.getEmailJwt());
       
  }
}
