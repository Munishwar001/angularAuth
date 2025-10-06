import { Component } from '@angular/core';
import { AuthApi } from "../service/auth/auth-api"
import { Router } from '@angular/router';
import { RouterOutlet , RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Api } from '../service/api';
@Component({
  selector: 'app-home',
  imports: [RouterOutlet , RouterLink , CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {
   
  role: string | null = null;

  constructor(private authService : AuthApi , private api  :Api , private router :Router) { }
   
  ngOnInit(){
    //  alert("a dettu6ui7o8p")
    this.role =  this.authService.getRole();
    console.log("role is ", this.role)
  }
  
   isAdmin(): boolean {
    return this.role?.toLowerCase() === 'admin';
  }

  isUser(): boolean {
    return this.role?.toLowerCase() === 'user';
  }
  
  adminData(){
     this.api.admin().subscribe({
      next : (res) =>{
          console.log(res);
      }, error(err) {
          console.log(err);
      },
     })
  }

  handleLogout(){
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

