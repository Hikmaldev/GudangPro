import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
export const authGuard: CanActivateFn = () => { const auth = inject(AuthService); return auth.isAuthenticated() ? true : inject(Router).createUrlTree(['/login']); };
export const adminGuard: CanActivateFn = () => { const auth = inject(AuthService); const user = auth.currentUser(); return user?.role === 'Admin Gudang' ? true : inject(Router).createUrlTree(['/dashboard']); };
