import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member';
import { PaginatedResults, Pagination } from '../models/pagination';
import { User } from '../models/user';
import { UserParameters } from '../models/userParameters';
import { AccountService } from './account.service';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User;
  userParameters: UserParameters;

  constructor(private httpClient: HttpClient,
    private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
      this.userParameters = new UserParameters(user);
    });
  }
  
  getUserParameters() {
    return this.userParameters;
  }

  setUserParameters(userParameters: UserParameters) {
    this.userParameters = userParameters;
  }

  resetUserParameters() {
    this.userParameters = new UserParameters(this.user);
    return this.userParameters;
  }

  getMembers(userParameters: UserParameters) {
    var response = this.memberCache.get(Object.values(userParameters).join('-'));

    if (response)
      return of(response);

    let params = this.getPaginationHeaders(userParameters.pageNumber, userParameters.pageSize);

    params = params.append('minAge', userParameters.minAge.toString());
    params = params.append('maxAge', userParameters.maxAge.toString());
    params = params.append('gender', userParameters.gender.toString());
    params = params.append('orderBy', userParameters.orderBy.toString());

    return this.getPaginatedResults<Member[]>(this.baseUrl + 'users', params)
      .pipe(map(response => {
        this.memberCache.set(Object.values(userParameters).join('-'), response);
        return response;
      }));
  }

  private getPaginatedResults<T>(url, params) {
    const paginatedResults: PaginatedResults<T> = new PaginatedResults<T>();

    return this.httpClient.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        paginatedResults.results = response.body;

        if (response.headers.get('Pagination') !== null) {
          paginatedResults.pagination = JSON.parse(response.headers.get('Pagination'));
        }

        return paginatedResults;
      })
    );
  }

  getMember(userName: string) {
    const member = [...this.memberCache.values()]
      .reduce((previousArray, element) => previousArray.concat(element.results), [])
      .find((member: Member) => member.userName === userName);

    if (member) {
      return of(member);
    }

    return this.httpClient.get<Member>(this.baseUrl + 'users/' + userName);
  }

  updateMember(member: Member) {
    return this.httpClient.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        this.members[index] = member;
      }),
    );
  }

  setMainPhoto(photoId: number) {
    return this.httpClient.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.httpClient.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }

  private getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;

  }
}
