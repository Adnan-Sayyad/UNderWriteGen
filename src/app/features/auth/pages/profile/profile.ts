import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../../core/auth/auth.service';
import { IamApiService } from '../../../../core/services/iam-api.service';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';

function passwordStrength(ctrl: AbstractControl): ValidationErrors | null {
  const v = ctrl.value as string;
  if (!v) return null;
  const ok = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,20}$/.test(v);
  return ok ? null : { weakPassword: true };
}

function passwordMatchValidator(ctrl: AbstractControl) {
  const np = ctrl.get('newPassword')?.value;
  const cp = ctrl.get('confirmNewPassword')?.value;
  return np && cp && np !== cp ? { mismatch: true } : null;
}

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class ProfilePage implements OnInit {
  readonly user    = computed(() => this.auth.currentUser());
  readonly profile = signal<any>(null);
  readonly loading = signal(false);
  readonly saving  = signal(false);

  readonly showCurrentPass = signal(false);
  readonly showNewPass     = signal(false);
  readonly showConfirmPass = signal(false);

  readonly successMsg = signal<string | null>(null);
  readonly errorMsg   = signal<string | null>(null);

  private readonly fb = inject(FormBuilder);

  readonly pwForm = this.fb.group({
    currentPassword:    ['', [Validators.required, Validators.minLength(8), Validators.maxLength(20),
                              passwordStrength]],
    newPassword:        ['', [Validators.required, Validators.minLength(8), Validators.maxLength(20),
                              passwordStrength]],
    confirmNewPassword: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(20),
                              passwordStrength]],
  }, { validators: passwordMatchValidator });

  readonly breadcrumbs = [{ label: 'Home', route: '/submissions' }, { label: 'My Profile' }];

  constructor(readonly auth: AuthService, private iam: IamApiService) {}

  ngOnInit(): void {
    const u = this.user();
    if (u?.userId) {
      this.loading.set(true);
      this.iam.getMyProfile(u.userId).subscribe({
        next: dto => {
          this.profile.set(dto);
          if (dto) this.auth.updateStoredName(`${dto.firstName} ${dto.lastName}`);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    }
  }

  changePassword(): void {
    if (this.pwForm.invalid) { this.pwForm.markAllAsTouched(); return; }
    this.saving.set(true);
    this.successMsg.set(null);
    this.errorMsg.set(null);
    this.auth.changePassword({
      currentPassword:    this.pwForm.value.currentPassword!,
      newPassword:        this.pwForm.value.newPassword!,
      confirmNewPassword: this.pwForm.value.confirmNewPassword!,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.pwForm.reset();
        this.successMsg.set('Password changed successfully.');
        setTimeout(() => this.successMsg.set(null), 4000);
      },
      error: err => {
        this.saving.set(false);
        this.errorMsg.set(err?.error?.message ?? 'Failed to change password.');
      },
    });
  }

  get newPwCtrl()     { return this.pwForm.controls.newPassword; }
  get currPwCtrl()    { return this.pwForm.controls.currentPassword; }
  get confirmPwCtrl() { return this.pwForm.controls.confirmNewPassword; }

  // Current password checklist
  private get curPwd(): string { return this.currPwCtrl.value ?? ''; }
  hasCurLength():  boolean { return this.curPwd.length >= 8 && this.curPwd.length <= 20; }
  hasCurUpper():   boolean { return /[A-Z]/.test(this.curPwd); }
  hasCurLower():   boolean { return /[a-z]/.test(this.curPwd); }
  hasCurDigit():   boolean { return /\d/.test(this.curPwd); }
  hasCurSymbol():  boolean { return /[\W_]/.test(this.curPwd); }

  // New password checklist
  private get newPwd(): string { return this.newPwCtrl.value ?? ''; }
  hasNewLength():  boolean { return this.newPwd.length >= 8 && this.newPwd.length <= 20; }
  hasNewUpper():   boolean { return /[A-Z]/.test(this.newPwd); }
  hasNewLower():   boolean { return /[a-z]/.test(this.newPwd); }
  hasNewDigit():   boolean { return /\d/.test(this.newPwd); }
  hasNewSymbol():  boolean { return /[\W_]/.test(this.newPwd); }

  // Confirm password checklist
  private get confPwd(): string { return this.confirmPwCtrl.value ?? ''; }
  hasConfLength():  boolean { return this.confPwd.length >= 8 && this.confPwd.length <= 20; }
  hasConfUpper():   boolean { return /[A-Z]/.test(this.confPwd); }
  hasConfLower():   boolean { return /[a-z]/.test(this.confPwd); }
  hasConfDigit():   boolean { return /\d/.test(this.confPwd); }
  hasConfSymbol():  boolean { return /[\W_]/.test(this.confPwd); }
}
