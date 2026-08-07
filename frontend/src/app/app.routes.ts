import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';
import { authGuard, guestGuard } from './core/guards/auth-guard';
import { Register } from './features/auth/register/register';
import { Login } from './features/auth/login/login';

export const routes: Routes = [
  {
    path: 'login',
    component: Login,
    canActivate: [guestGuard],
  },
  {
    path: 'register',
    component: Register,
    canActivate: [guestGuard],
  },
  { path: 'chat',
    component: MainLayout,
    canActivate: [authGuard],
  },
  { path: '',
    redirectTo: 'chat', 
    pathMatch: 'full' ,
  },
  {
    path: '**',
    redirectTo: 'chat',
  },
];