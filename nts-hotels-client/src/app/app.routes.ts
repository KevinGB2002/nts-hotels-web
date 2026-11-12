import { Routes } from '@angular/router';
import { adminGuard } from './guards/auth.guard';
import { HomeComponent } from './pages/home/home.component';
import { LoginComponent } from './pages/login/login.component';
import { AdminComponent } from './pages/admin/admin.component';
import { UserManagementComponent } from './pages/user-management/user-management.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'admin', component: AdminComponent, canActivate: [adminGuard] },
  { path: 'admin/users', component: UserManagementComponent, canActivate: [adminGuard] },
  { path: '**', redirectTo: '' }
];