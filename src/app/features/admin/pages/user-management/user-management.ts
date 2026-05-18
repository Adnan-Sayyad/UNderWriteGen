import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { PageHeader } from '../../../../shared/components/page-header/page-header';
import { StatusBadge } from '../../../../shared/components/status-badge/status-badge';
import { EmptyState } from '../../../../shared/components/empty-state/empty-state';
import { Pagination } from '../../../../shared/components/pagination/pagination';
import { IamApiService, UserDto } from '../../../../core/services/iam-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { PartyApiService } from '../../../party/services/party-api.service';

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
  imports: [CommonModule, RouterModule, ReactiveFormsModule, PageHeader, StatusBadge, EmptyState, Pagination],
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
  readonly currentPage = signal(0);
  readonly pageSize    = signal(10);

  readonly filtered = computed(() => {
    const q    = this.searchTerm().toLowerCase();
    const role = this.roleFilter();
    return this.users().filter(u =>
      (!q    || `${u.firstName} ${u.lastName} ${u.email}`.toLowerCase().includes(q)) &&
      (!role || u.role === role)
    );
  });

  readonly totalElements = computed(() => this.filtered().length);
  readonly totalPages    = computed(() => Math.max(1, Math.ceil(this.totalElements() / this.pageSize())));

  readonly paged = computed(() => {
    const size  = this.pageSize();
    const page  = Math.min(this.currentPage(), this.totalPages() - 1);
    const start = page * size;
    return this.filtered().slice(start, start + size);
  });

  setSearch(val: string): void     { this.searchTerm.set(val); this.currentPage.set(0); }
  setRoleFilter(val: string): void { this.roleFilter.set(val); this.currentPage.set(0); }
  onPageChange(p: number): void    { this.currentPage.set(p); }
  onSizeChange(s: number): void    { this.pageSize.set(s); this.currentPage.set(0); }

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
    private partySvc: PartyApiService,
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
              // If role is Agent → also create an Agent record in Distribution service
              if (v.role === 'Agent') {
                const fullName = `${v.firstName} ${v.lastName}`;
                this.partySvc.createAgent({
                  name:        fullName,
                  contactInfo: v.email!,
                  region:      'South India',
                } as any).subscribe({
                  next: () => {
                    this.saving.set(false);
                    this.closeModal();
                    this.loadUsers();
                    this.flash('success', `Agent ${fullName} created with agent profile.`);
                  },
                  error: () => {
                    // User created successfully even if agent record fails
                    this.saving.set(false);
                    this.closeModal();
                    this.loadUsers();
                    this.flash('success', `User ${newUser.firstName} ${newUser.lastName} created. (Agent profile creation failed — add manually in Agents page.)`);
                  },
                });
              } else {
                this.saving.set(false);
                this.closeModal();
                this.loadUsers();
                this.flash('success', `User ${newUser.firstName} ${newUser.lastName} created.`);
              }
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

  /** Manually creates an Agent profile in the Distribution service for an existing Agent-role user. */
  createAgentProfile(u: UserDto): void {
    const fullName = `${u.firstName} ${u.lastName}`.trim();
    if (!confirm(`Create an Agent profile in Distribution for "${fullName}"?`)) return;
    this.partySvc.createAgent({
      name:        fullName,
      contactInfo: u.email ?? '',
      region:      'South India',
    } as any).subscribe({
      next: () => this.flash('success', `Agent profile created for ${fullName}.`),
      error: err => this.flash('danger', err?.error?.message ?? err?.error?.Message ?? `Failed to create agent profile for ${fullName}.`),
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
