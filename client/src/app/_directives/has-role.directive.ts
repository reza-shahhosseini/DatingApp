import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit{
  user:User;
  @Input() appHasRole:string[];
  constructor(private accountService:AccountService,private viewContainerRef:ViewContainerRef,private templateRef:TemplateRef<any>) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user = user;
    })
  }
  ngOnInit(): void {
    if(this.user==null||!this.user?.roles){
      this.viewContainerRef.clear();
      return;
    }

    if(this.user?.roles.some(role=>this.appHasRole.includes(role))){
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    }else{
      this.viewContainerRef.clear();
    }
  }

}
