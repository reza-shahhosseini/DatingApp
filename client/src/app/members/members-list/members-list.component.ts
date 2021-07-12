import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import {IPagination} from 'src/app/_models/pagination';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-members-list',
  templateUrl: './members-list.component.html',
  styleUrls: ['./members-list.component.css']
})
export class MembersListComponent implements OnInit {
  members:Member[];
  paginationInformation:IPagination;
  userParams:UserParams;
  user:User;
  genderList=[
    {value:'male',display:"Males"},
    {value:'female',display:"Females"},
  ];

  constructor(private memberService:MembersService) {
     this.userParams=memberService.getUserParams();
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(){
    //alert("Member-List Min Age: "+this.userParams.minAge);
    this.memberService.setUserParams(this.userParams);
    this.memberService.getMembers(this.userParams).subscribe(response=>{
      this.members=response.result;
      this.paginationInformation=response.paginationInformation;
    });
  }

  pageChanged(event:any){
    this.userParams.pageNumber=event.page;
    this.memberService.setUserParams(this.userParams);
    this.loadMembers();
  }

  resetFilters(){
    this.userParams=this.memberService.resetUserParams();
    this.loadMembers();
  }

}
