import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly error = signal('');

  readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  invalid(control: 'email' | 'password'): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || c.dirty);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        if (!this.auth.isAdmin()) {
          this.auth.logout();
          this.loading.set(false);
          this.error.set('No eres un administrador');
          return;
        }
        this.router.navigate(['/admin']);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Correo o contraseña incorrectos.');
      }
    });
  }
}
