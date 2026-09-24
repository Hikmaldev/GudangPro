import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { AuthService } from '../services/auth.service';

/**
 * Menyisipkan token JWT ke header Authorization pada setiap request.
 * Sesuai PRD FR-AUTH-02 (JWT dengan refresh token) — interceptor ini adalah
 * titik tunggal untuk melampirkan access token sebelum request dikirim.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.accessToken();

  if (!token) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(cloned);
};
