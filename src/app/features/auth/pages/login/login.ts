import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../../../core/auth/auth.service';

function passwordStrength(ctrl: AbstractControl): ValidationErrors | null {
  const v = ctrl.value as string;
  if (!v) return null;
  const ok = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,20}$/.test(v);
  return ok ? null : { weakPassword: true };
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.group({
    email:    ['', [Validators.required,
                    Validators.pattern(/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/)]],
    password: ['', [Validators.required, Validators.minLength(8), Validators.maxLength(20), passwordStrength]],
  });

  readonly loading  = signal(false);
  readonly error    = signal<string | null>(null);
  readonly showPass = signal(false);

  constructor(
    private auth: AuthService,
    private router: Router,
  ) {}

  private get pwd(): string { return this.form.controls.password.value ?? ''; }
  hasLength():  boolean { return this.pwd.length >= 8 && this.pwd.length <= 20; }
  hasUpper():   boolean { return /[A-Z]/.test(this.pwd); }
  hasLower():   boolean { return /[a-z]/.test(this.pwd); }
  hasDigit():   boolean { return /\d/.test(this.pwd); }
  hasSymbol():  boolean { return /[\W_]/.test(this.pwd); }

  reset(): void {
    this.form.reset();
    this.error.set(null);
    this.showPass.set(false);
  }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.loading.set(true);
    this.error.set(null);

    const { email, password } = this.form.value;
    this.auth.login({ email: email!, password: password! }).subscribe({
      next: () => {
        this.loading.set(false);
        const role = this.auth.currentUser()?.role;
        const destination: Record<string, string> = {
          Agent:          '/submissions',     // manage their own submissions
          Underwriter:    '/underwriting',    // UW workbench
          UWAssistant:    '/underwriting',    // assist underwriters
          PricingAnalyst: '/pricing',         // quotes & pricing params
          Compliance:     '/compliance',      // checklists & audit
          Operations:     '/policy',          // policy desk & bind
          Admin:          '/admin',           // user management & system config
        };
        this.router.navigate([destination[role ?? ''] ?? '/submissions']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.error?.message ?? err?.message ?? 'Invalid email or password.');
      },
    });
  }

  get emailCtrl()    { return this.form.controls.email; }
  get passwordCtrl() { return this.form.controls.password; }
}