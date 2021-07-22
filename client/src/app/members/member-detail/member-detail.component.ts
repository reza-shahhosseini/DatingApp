import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions,NgxGalleryImage, NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  member:Member;
  galleryOptions:NgxGalleryOptions[];
  galleryImages:NgxGalleryImage[];
  @ViewChild('memberTabs',{static:true}) memberTabs:TabsetComponent;
  activeTab:TabDirective;
  messages:Message[]=[];

  constructor(private memberService:MembersService,private router:ActivatedRoute,private messageService:MessageService) { }

  ngOnInit(): void {
    this.router.data.subscribe(data=>{
      this.member=data.member;
    });
    this.galleryOptions=[
      {
        width:'500px',
        height:'500px',
        imagePercent:100,
        thumbnailsColumns:4,
        imageAnimation:NgxGalleryAnimation.Slide,
        preview:false,
      }
    ]

    this.galleryImages=this.getImages();

    this.router.queryParams.subscribe(params=>{
      params.tab?this.selectTab(params.tab):this.selectTab(0);
    })
  }

  
  getImages():NgxGalleryImage[]{
    const imageUrls=[];
    for(const photo of this.member.photos){
      imageUrls.push({
        small:photo?.url,
        medium:photo?.url,
        big:photo?.url,
      })
    }
    return imageUrls;
  }

  onTabActivated(tab:TabDirective){
    this.activeTab=tab;
    if(this.activeTab.heading==='Messages' && this.messages.length===0){
      this.loadMessages();
    }
  }

  selectTab(tabId:number){
    this.memberTabs.tabs[tabId].active=true;
  }

  loadMessages(){
    this.messageService.getMessageThread(this.member.username).subscribe(response=>{
      this.messages=response;
    });
  }

}
