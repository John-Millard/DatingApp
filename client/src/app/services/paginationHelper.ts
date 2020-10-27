import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { PaginatedResults } from '../models/pagination';

export function getPaginatedResults<T>(httpClient: HttpClient, url, params) {
    const paginatedResults: PaginatedResults<T> = new PaginatedResults<T>();

    return httpClient.get<T>(url, { observe: 'response', params }).pipe(
        map(response => {
        paginatedResults.results = response.body;

        if (response.headers.get('Pagination') !== null) {
            paginatedResults.pagination = JSON.parse(response.headers.get('Pagination'));
        }

        return paginatedResults;
        })
    );
}

export function getPaginationHeaders(pageNumber: number, pageSize: number) {
    let params = new HttpParams();

    params = params.append('pageNumber', pageNumber.toString());
    params = params.append('pageSize', pageSize.toString());

    return params;
}