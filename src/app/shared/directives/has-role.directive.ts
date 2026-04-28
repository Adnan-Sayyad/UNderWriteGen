import { Directive, Input, TemplateRef, ViewContainerRef, OnInit } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

@Directive({ selector: '[hasRole]', standalone: true })
export class HasRoleDirective implements OnInit {
  @Input() hasRole: string | string[] = [];

  constructor(
    private tpl: TemplateRef<unknown>,
    private vcr: ViewContainerRef,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const roles = Array.isArray(this.hasRole) ? this.hasRole : [this.hasRole];
    if (this.auth.hasRole(...roles)) {
      this.vcr.createEmbeddedView(this.tpl);
    }
  }
}
