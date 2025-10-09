import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthApi } from '../service/auth/auth-api';
import { Observable } from 'rxjs';
@Component({
  selector: 'app-home-component',
  imports: [CommonModule],
  templateUrl: './home-component.html',
  styleUrl: './home-component.css'
})
export class HomeComponent {

  role$!: Observable<string | null>;

  constructor(private authService: AuthApi) { }

  ngOnInit() {
    const jwtRole = this.authService.getRoleJwt();
    console.log('Role from JWT:', jwtRole);
    this.authService.setRole(jwtRole?.toLowerCase() || null);
    this.role$ = this.authService.role$;
  }
}
