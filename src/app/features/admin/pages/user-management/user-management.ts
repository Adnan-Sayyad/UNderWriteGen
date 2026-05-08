import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { IamApiService, UserDto } from '../../../../core/services/iam-api.service';
import { AuthService } from '../../../../core/auth/auth.service';

type ModalMode = 'create' | 'edit' | 'status' | 'assign-role' | null;

const ROLES = ['Agent','Underwriter','UWAssistant','PricingAnalyst','Compliance','Operations','Admin'];

const ROLE_DESC: Record<string, string> = {
  Agent:         'Handles submission intake and client contact',
  Underwriter:   'Reviews risk and makes underwriting decisions',
  UWAssistant:   'Supports underwriters with workbench tasks',
  PricingAnalyst:'Manages pricing parameters and quotes',
  Compliance:    'Monitors compliance, audits and exceptions',
  Operations:    'Manages policy issuance and operations',
  Admin:         'Full system access and user management',
};
const STATUSES: ('Active'|'Locked'|'Disabled')[] = ['Active','Locked','Disabled'];

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css',
})
export class UserManagementPage implements OnInit {
  readonly roles    = ROLES;
  readonly roleDesc = ROLE_DESC;
  readonly statuses = STATUSES;

  readonly users      = signal<UserDto[]>([]);
  readonly loading    = signal(false);
  readonly saving     = signal(false);
  readonly modalMode  = signal<ModalMode>(null);
  readonly selected   = signal<UserDto | null>(null);
  readonly searchTerm = signal('');
  readonly roleFilter = signal('');
  readonly alertMsg   = signal<{ type: 'success'|'danger'; text: string } | null>(null);

  // Pagination
  readonly currentPage = signal(1);
  readonly pageSize    = 10;

  readonly filtered = computed(() => {
    const q    = this.searchTerm().toLowerCase();
    const role = this.roleFilter();
    return this.users().filter(u =>
      (!q    || `${u.firstName} ${u.lastName} ${u.email}`.toLowerCase().includes(q)) &&
      (!role || u.role === role)
    );
  });

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.filtered().length / this.pageSize)));

  readonly paged = computed(() => {
    const page = Math.min(this.currentPage(), this.totalPages());
    const start = (page - 1) * this.pageSize;
    return this.filtered().slice(start, start + this.pageSize);
  });

  readonly pageNumbers = computed(() =>
    Array.from({ length: this.totalPages() }, (_, i) => i + 1)
  );

  setSearch(val: string): void  { this.searchTerm.set(val); this.currentPage.set(1); }
  setRoleFilter(val: string): void { this.roleFilter.set(val); this.currentPage.set(1); }
  prevPage(): void { if (this.currentPage() > 1) this.currentPage.update(p => p - 1); }
  nextPage(): void { if (this.currentPage() < this.totalPages()) this.currentPage.update(p => p + 1); }
  goToPage(n: number): void { this.currentPage.set(n); }

  readonly breadcrumbs = [{ label: 'Home', route: '/' }, { label: 'Admin', route: '/admin' }, { label: 'User Management' }];

  private readonly fb = inject(FormBuilder);

  readonly showCreatePass = signal(false);

  private get createPwd(): string { return this.createForm.controls.password.value ?? ''; }
  hasCreateLength():  boolean { return this.createPwd.length >= 8 && this.createPwd.length <= 20; }
  hasCreateUpper():   boolean { return /[A-Z]/.test(this.createPwd); }
  hasCreateLower():   boolean { return /[a-z]/.test(this.createPwd); }
  hasCreateDigit():   boolean { return /\d/.test(this.createPwd); }
  hasCreateSymbol():  boolean { return /[\W_]/.test(this.createPwd); }

  readonly createForm = this.fb.group({
    firstName:   ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20),
                       Validators.pattern(/^[a-zA-Z]+$/)]],
    lastName:    ['', [Validators.required, Validators.minLength(1), Validators.maxLength(20),
                       Validators.pattern(/^[a-zA-Z]+$/)]],
    email:       ['', [Validators.required,
                       Validators.pattern(/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/)]],
    phoneNumber: ['', [Validators.pattern(/^[6-9]\d{9}$/)]],
    password:    ['', [Validators.required, Validators.minLength(8), Validators.maxLength(20),
                       Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$/)]],
    role:        ['Agent', Validators.required],
  });

  readonly editForm = this.fb.group({
    firstName:   ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20),
                       Validators.pattern(/^[a-zA-Z]+$/)]],
    lastName:    ['', [Validators.required, Validators.minLength(1), Validators.maxLength(20),
                       Validators.pattern(/^[a-zA-Z]+$/)]],
    email:       ['', [Validators.required,
                       Validators.pattern(/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/)]],
    phoneNumber: ['', [Validators.pattern(/^[6-9]\d{9}$/)]],
  });

  readonly statusForm = this.fb.group({
    status: ['Active', Validators.required],
  });

  readonly assignRoleForm = this.fb.group({
    role: ['Agent', Validators.required],
  });

  private get adminId(): string { return this.auth.currentUser()?.userId ?? ''; }

  constructor(
    private iam: IamApiService,
    readonly auth: AuthService,
  ) {}

  ngOnInit(): void { this.loadUsers(); }

  loadUsers(): void {
    this.loading.set(true);
    this.iam.getUsers(this.adminId).subscribe({
      next: data => { this.users.set(data); this.loading.set(false); },
      error: ()   => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.createForm.reset({ role: 'Agent' });
    this.showCreatePass.set(false);
    this.modalMode.set('create');
  }

  resetCreate(): void {
    this.createForm.reset({ role: 'Agent' });
    this.showCreatePass.set(false);
  }

  openEdit(u: UserDto): void {
    this.selected.set(u);
    this.editForm.patchValue({
      firstName: u.firstName, lastName: u.lastName,
      email: u.email, phoneNumber: u.phoneNumber ?? '',
    });
    this.modalMode.set('edit');
  }

  openAssignRole(u: UserDto): void {
    this.selected.set(u);
    // role may be a comma-separated list — pick the first recognised role
    const current = (u.role ?? '').split(',').map(r => r.trim())
                      .find(r => ROLES.includes(r)) ?? 'Agent';
    this.assignRoleForm.patchValue({ role: current });
    this.modalMode.set('assign-role');
  }

  openStatus(u: UserDto): void {
    this.selected.set(u);
    this.statusForm.patchValue({ status: u.status as any });
    this.modalMode.set('status');
  }

  closeModal(): void { this.modalMode.set(null); this.selected.set(null); }

  saveCreate(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.createForm.value;
    this.iam.registerUser({
      firstName: v.firstName!, lastName: v.lastName!,
      email: v.email!, password: v.password!,
      phoneNumber: v.phoneNumber || undefined,
    }).subscribe({
      next: newUser => {
        if (!newUser) { this.saving.set(false); return; }
        this.iam.assignRole({ adminId: this.adminId, userId: newUser.id, roles: [v.role!] })
          .subscribe({
            next: () => {
              this.saving.set(false);
              this.closeModal();
              this.loadUsers();
              this.flash('success', `User ${newUser.firstName} ${newUser.lastName} created.`);
            },
            error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Role assignment failed.'); },
          });
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Registration failed.'); },
    });
  }

  saveEdit(): void {
    if (this.editForm.invalid || !this.selected()) { this.editForm.markAllAsTouched(); return; }
    this.saving.set(true);
    const v = this.editForm.value;
    this.iam.updateUser(this.selected()!.id, this.adminId, {
      firstName: v.firstName!, lastName: v.lastName!, email: v.email!,
      phoneNumber: v.phoneNumber || undefined,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.loadUsers();
        this.flash('success', 'User updated successfully.');
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Update failed.'); },
    });
  }

  saveAssignRole(): void {
    if (!this.selected()) return;
    this.saving.set(true);
    this.iam.assignRole({
      adminId: this.adminId,
      userId:  this.selected()!.id,
      roles:   [this.assignRoleForm.value.role!],
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.loadUsers();
        this.flash('success', `Role assigned to ${this.selected()?.firstName ?? 'user'}.`);
      },
      error: err => {
        this.saving.set(false);
        this.flash('danger', err?.error?.message ?? err?.error?.Message ?? 'Role assignment failed.');
      },
    });
  }

  saveStatus(): void {
    if (!this.selected()) return;
    this.saving.set(true);
    this.iam.updateUserStatus(this.selected()!.id, this.adminId, {
      status: this.statusForm.value.status as any,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.loadUsers();
        this.flash('success', 'User status updated.');
      },
      error: err => { this.saving.set(false); this.flash('danger', err?.error?.message ?? 'Status update failed.'); },
    });
  }

  deleteUser(u: UserDto): void {
    if (!confirm(`User "${u.firstName} ${u.lastName}" is going to be deleted.`)) return;
    this.iam.deleteUser(u.id, this.adminId).subscribe({
      next: () => { this.loadUsers(); this.flash('success', 'User deleted.'); },
      error: err => this.flash('danger', err?.error?.message ?? 'Delete failed.'),
    });
  }

  statusBtnClass(status: string): string {
    return status === 'Active'   ? 'btn-outline-success'
         : status === 'Locked'   ? 'btn-outline-warning'
         : 'btn-outline-danger';
  }

  statusBtnIcon(status: string): string {
    return status === 'Active'   ? 'bi-toggle-on'
         : status === 'Locked'   ? 'bi-lock'
         : 'bi-slash-circle';
  }

  private flash(type: 'success'|'danger', text: string): void {
    this.alertMsg.set({ type, text });
    setTimeout(() => this.alertMsg.set(null), 4000);
  }
}
