import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private httpClient: HttpClient) { }

  getUserWithRoles() {
    return this.httpClient.get<Partial<User[]>>(this.baseUrl + 'admin/users-with-roles');
  }

  updateUserRoles(userName: string, roles: string[]) {
    return this.httpClient.post(this.baseUrl + 'admin/edit-roles/' + userName + '?roles=' + roles, {});
  }
}
