import { Component, computed } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './navbar.html',
})
export class Navbar {
  readonly user = computed(() => this.auth.currentUser());
  constructor(readonly auth: AuthService) {}
}
