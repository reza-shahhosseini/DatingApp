import { Component, OnInit } from '@angular/core';
import { Member } from '../_models/member';
import { IPagination } from '../_models/pagination';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {

  members:Partial<Member[]>;
  predicate:string='liked';

  pageNumber=1;
  pageSize=5;

  paginationInformation:IPagination;

  constructor(private memberService:MembersService) { }

  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes(){
    this.memberService.getLikes(this.predicate,this.pageNumber,this.pageSize).subscribe(response=>{
      this.members=response.result;
      this.paginationInformation=response.paginationInformation;
    });
  }

  pageChanged(event:any){
    this.pageNumber=event.page;
    this.loadLikes();
  }

}
