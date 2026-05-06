import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../../core/auth/auth.service';

function passwordStrength(ctrl: AbstractControl): ValidationErrors | null {
  const v = ctrl.value as string;
  if (!v) return null;
  const ok = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/.test(v);
  return ok ? null : { weakPassword: true };
}

function mustMatch(a: string, b: string) {
  return (group: AbstractControl): ValidationErrors | null => {
    const pass    = group.get(a)?.value;
    const confirm = group.get(b)?.value;
    return pass === confirm ? null : { mismatch: true };
  };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group(
    {
      firstName:   ['', [Validators.required, Validators.maxLength(100)]],
      lastName:    ['', [Validators.required, Validators.maxLength(100)]],
      email:       ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.pattern(/^\+?[0-9\s\-().]{7,20}$/)]],
      password:    ['', [Validators.required, Validators.minLength(8), passwordStrength]],
      confirmPass: ['', Validators.required],
    },
    { validators: mustMatch('password', 'confirmPass') }
  );

  readonly loading  = signal(false);
  readonly showPass = signal(false);
  readonly showConf = signal(false);
  readonly error    = signal<string | null>(null);
  readonly success  = signal<string | null>(null);

  constructor(private auth: AuthService, private router: Router) {}

  get f() { return this.form.controls; }

  private get pwd(): string { return this.f['password'].value ?? ''; }
  hasLength(): boolean  { return this.pwd.length >= 8; }
  hasUpper(): boolean   { return /[A-Z]/.test(this.pwd); }
  hasLower(): boolean   { return /[a-z]/.test(this.pwd); }
  hasDigit(): boolean   { return /\d/.test(this.pwd); }
  hasSymbol(): boolean  { return /[\W_]/.test(this.pwd); }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);

    const v = this.form.value;
    this.auth.register({
      firstName:   v.firstName!,
      lastName:    v.lastName!,
      email:       v.email!,
      password:    v.password!,
      phoneNumber: v.phoneNumber || undefined,
    }).subscribe({
      next: res => {
        this.loading.set(false);
        this.success.set('Account created! Redirecting to login…');
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
      },
      error: err => {
        this.loading.set(false);
        this.error.set(
          err?.error?.message ?? err?.error?.Message ?? 'Registration failed. Please try again.'
        );
      },
    });
  }
}
