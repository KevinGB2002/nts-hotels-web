import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiUrl)) return next(req);

  const auth = inject(AuthService);
  const token = auth.getToken();
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      // Token expirado o inválido en una petición autenticada.
      if (err.status === 401 && token) auth.logout();
      return throwError(() => err);
    })
  );
};
