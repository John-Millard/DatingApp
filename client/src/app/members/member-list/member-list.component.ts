import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/models/member';
import { Pagination } from 'src/app/models/pagination';
import { User } from 'src/app/models/user';
import { UserParameters } from 'src/app/models/userParameters';
import { AccountService } from 'src/app/services/account.service';
import { MembersService } from 'src/app/services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  members: Member[];
  pagination: Pagination;
  userParameters: UserParameters;
  user: User;
  genderList = [
    { value: 'male', display: 'Males' },
    { value: 'female', display: 'Females' },
  ];

  constructor(private memberService: MembersService) { 
    this.userParameters = this.memberService.getUserParameters();
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.setUserParameters(this.userParameters);
    this.memberService.getMembers(this.userParameters).subscribe(response => {
      this.members = response.results;
      this.pagination = response.pagination;
    })
  }

  resetFilters() {
    this.userParameters = this.memberService.resetUserParameters();
    this.loadMembers();
  }

  pageChanged(event: any) {
    this.userParameters.pageNumber = event.page;
    this.memberService.setUserParameters(this.userParameters);
    this.loadMembers();
  }
}
