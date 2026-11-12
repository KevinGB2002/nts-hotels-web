import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { User } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { NotificationService } from '../../services/notification.service';

// Sin endpoint de roles: ajustar a los IDs reales del backend.
export const ROLES = [
  { id: 1, name: 'Administrador' },
  { id: 2, name: 'Usuario' }
];

const PASSWORD_PATTERN = /^(?=.*[A-Z])(?=.*\d).{6,}$/;

@Component({
  selector: 'app-user-management',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './user-management.component.html',
  styleUrl: './user-management.component.css'
})
export class UserManagementComponent implements OnInit {
  private readonly service = inject(UserService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder).nonNullable;
  readonly auth = inject(AuthService);

  readonly roles = ROLES;
  readonly users = signal<User[]>([]);
  readonly loading = signal(false);
  readonly modalOpen = signal(false);
  readonly editing = signal<User | null>(null);
  readonly saving = signal(false);
  readonly error = signal('');

  readonly form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
    roleId: [ROLES[1].id, Validators.required],
    isActive: [true]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getUsers().subscribe({
      next: users => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de usuarios.');
        this.notifications.error(this.error());
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.error.set('');
    this.editing.set(null);
    this.form.reset({ roleId: ROLES[1].id, isActive: true });
    this.setPasswordRequired(true);
    this.modalOpen.set(true);
  }

  openEdit(user: User): void {
    this.error.set('');
    this.editing.set(user);
    this.form.reset({ ...user, password: '' });
    this.setPasswordRequired(false);
    this.modalOpen.set(true);
  }

  close(): void {
    this.modalOpen.set(false);
  }

  invalid(control: 'fullName' | 'email' | 'password' | 'roleId'): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || c.dirty);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const current = this.editing();
    const { password, roleId, ...rest } = this.form.getRawValue();
    const payload: Partial<User> = {
      ...rest,
      roleId: Number(roleId),
      roleName: ROLES.find(r => r.id === Number(roleId))?.name ?? '',
      ...(password ? { password } : {})
    };

    this.saving.set(true);
    this.error.set('');
    const request$: Observable<unknown> = current
      ? this.service.updateUser(current.id, { ...payload, id: current.id })
      : this.service.createUser(payload);
    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.close();
        this.notifications.success(current ? 'Se actualizó correctamente el usuario.' : 'Se guardó correctamente el usuario.');
        this.load();
      },
      error: () => {
        this.saving.set(false);
        this.error.set('No se pudo guardar el usuario.');
        this.notifications.error(this.error());
      }
    });
  }

  toggleActive(user: User): void {
    this.error.set('');
    this.service.updateUser(user.id, { ...user, isActive: !user.isActive }).subscribe({
      next: () => {
        this.notifications.success('Se actualizó correctamente el usuario.');
        this.load();
      },
      error: () => {
        this.error.set('No se pudo cambiar el estado del usuario.');
        this.notifications.error(this.error());
      }
    });
  }

  remove(user: User): void {
    if (!confirm(`¿Eliminar a "${user.fullName}"?`)) return;
    this.error.set('');
    this.service.deleteUser(user.id).subscribe({
      next: () => {
        this.notifications.success('Se eliminó correctamente el usuario.');
        this.load();
      },
      error: () => {
        this.error.set('No se pudo eliminar el usuario.');
        this.notifications.error(this.error());
      }
    });
  }

  // En edición la contraseña es opcional, pero si se escribe debe cumplir el patrón.
  private setPasswordRequired(required: boolean): void {
    const ctrl = this.form.controls.password;
    ctrl.setValidators(
      required ? [Validators.required, Validators.pattern(PASSWORD_PATTERN)] : [Validators.pattern(PASSWORD_PATTERN)]
    );
    ctrl.updateValueAndValidity();
  }
}
