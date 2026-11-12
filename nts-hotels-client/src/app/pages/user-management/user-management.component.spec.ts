import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { User } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { NotificationService } from '../../services/notification.service';
import { UserManagementComponent } from './user-management.component';

describe('UserManagementComponent save messages', () => {
  const user: User = {
    id: '1', fullName: 'Usuario', email: 'usuario@example.com', roleId: 2,
    roleName: 'User', isActive: true, createdAt: '2026-01-01'
  };
  const service = {
    getUsers: vi.fn(),
    createUser: vi.fn(),
    updateUser: vi.fn(),
    deleteUser: vi.fn()
  };

  beforeEach(() => {
    vi.resetAllMocks();
    service.getUsers.mockReturnValue(of([user]));
    TestBed.configureTestingModule({
      imports: [UserManagementComponent],
      providers: [
        provideRouter([]),
        { provide: UserService, useValue: service },
        { provide: AuthService, useValue: {} }
      ]
    });
  });

  it('shows a confirmation after creating and editing a user', () => {
    service.createUser.mockReturnValue(of(user));
    service.updateUser.mockReturnValue(of(undefined));
    const fixture = TestBed.createComponent(UserManagementComponent);
    const component = fixture.componentInstance;
    component.openCreate();
    component.form.patchValue({ ...user, password: 'Example1' });
    component.save();
    fixture.detectChanges();
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.message).toContain('Se guardó correctamente el usuario.');

    component.openEdit(user);
    component.save();
    fixture.detectChanges();
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.message).toContain('Se actualizó correctamente el usuario.');
  });

  it('shows an error in the open form when saving fails', () => {
    service.updateUser.mockReturnValue(throwError(() => new Error('failed')));
    const fixture = TestBed.createComponent(UserManagementComponent);
    const component = fixture.componentInstance;
    component.openEdit(user);
    component.save();
    fixture.detectChanges();
    expect(component.modalOpen()).toBe(true);
    expect(fixture.nativeElement.querySelector('.modal [role="alert"]').textContent).toContain('No se pudo guardar el usuario.');
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.type).toBe('error');
  });

  it('notifies after deletion only when the API succeeds', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    service.deleteUser.mockReturnValue(of(undefined));
    const component = TestBed.createComponent(UserManagementComponent).componentInstance;
    component.remove(user);
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.message).toContain('Se eliminó correctamente');
    service.deleteUser.mockReturnValue(throwError(() => new Error('failed')));
    component.remove(user);
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.type).toBe('error');
    vi.restoreAllMocks();
  });
});
