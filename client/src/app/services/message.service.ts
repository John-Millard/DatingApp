import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../models/message';
import { getPaginatedResults, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;

  constructor(private httpClient: HttpClient) { }

  getMessages(pageNumber, pageSize, container) {
    let parameters = getPaginationHeaders(pageNumber, pageSize);
    parameters = parameters.append('Container', container);
    return getPaginatedResults<Message[]>(this.httpClient, this.baseUrl + 'messages', parameters);
  }

  getMessageThread(userName: string) {
    return this.httpClient.get<Message[]>(this.baseUrl + 'messages/thread/' + userName);
  }

  sendMessage(userName: string, content: string) {
    return this.httpClient.post<Message>(this.baseUrl + 'messages', { recipientUserName: userName, content });
  }

  deleteMessage(id: number) {
    return this.httpClient.delete(this.baseUrl + 'messages/' + id);
  }
}
