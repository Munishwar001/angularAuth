import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment'
import { Observable } from 'rxjs';
import { AuthApi } from './auth/auth-api';

@Injectable({
  providedIn: 'root'
})
export class Api {

  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient, private authservice: AuthApi) { }

  admin(): Observable<any> {

    return this.http.get(`${this.baseUrl}/Dashboard/admin-data`)
  }

  getAllUsers(): Observable<any> {
    return this.http.get(`${this.baseUrl}/User/Users`);
  }

  updateUser(userId: string, updatedUser: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/User/Update/${userId}`, updatedUser);
  }
  
  deleteUser(userId:string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/User/${userId}`);
  }

  // profile():Observable<any> {
  //   return this.http.get(`${this.baseUrl}/User/Users`, param:{"munishwar@gmil.om"});
  // }
  
}
