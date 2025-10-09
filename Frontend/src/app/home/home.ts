import { Component } from '@angular/core';
import { AuthApi } from "../service/auth/auth-api"
import { Router } from '@angular/router';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Api } from '../service/api';
import { Observable } from 'rxjs';
@Component({
  selector: 'app-home',
  imports: [RouterOutlet, RouterLink, CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {

  role$!: Observable<string | null>;

  constructor(private authService: AuthApi, private api: Api, private router: Router) { }

 ngOnInit() {
  const jwtRole = this.authService.getRoleJwt();
  console.log('Role from JWT:', jwtRole); 
  this.authService.setRole(jwtRole?.toLowerCase() || null);
  this.role$ = this.authService.role$;
}

  adminData() {
    this.api.admin().subscribe({
      next: (res) => {
        console.log(res);
      }, error(err) {
        console.log(err);
      },
    })
  }

  handleLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}

