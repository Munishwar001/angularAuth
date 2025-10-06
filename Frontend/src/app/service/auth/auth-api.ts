import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment'
import { SignupRequest, LoginRequest, ApiResponse, JwtPayload } from '../../models/auth.models';
import { jwtDecode } from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AuthApi {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  signup(data: SignupRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/Auth/signup`, data);
  }

  login(data: LoginRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.baseUrl}/Auth/login`, data);
  }

  saveToken(token: string) {
    const encodedToken = btoa(token);
    // localStorage.setItem('jwt', token);
    localStorage.setItem('access_token', encodedToken);
  }

  getToken(): string | null {
    // return localStorage.getItem('jwt');
    const encodedToken = localStorage.getItem('access_token');
    if (!encodedToken) return null;
    return atob(encodedToken);
  }

  logout() {
    // localStorage.removeItem('jwt');
    localStorage.removeItem('access_token');
  }

  isLogin(): Observable<any> {
    const token = this.getToken();
    const headers = { Authorization: `Bearer ${token}` };
    return this.http.get(`${this.baseUrl}/Auth/validate-token`, { headers })
  }

  getRole(): string | null {
    const token = this.getToken();
    if (!token) return null;

    const decoded = jwtDecode<JwtPayload>(token);
    console.log(decoded.role, "role");
    return decoded.role || null;
  }

  forgotPassword(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/Auth/forgot-password`, data);
  }
  
  resetPassword(data:any):Observable<any> {
    return this.http.post(`${this.baseUrl}/Auth/resetPassword`, data);
  }
}
