import { Routes } from '@angular/router';
import { ProfilePanel } from './features/profile/profile-panel/profile-panel';
import { MainLayout } from './layout/main-layout/main-layout';
import { authGuard, guestGuard } from './core/guards/auth-guard';
import { SettingsPanel } from './features/profile/settings-panel/settings-panel';
import { NewChat } from './features/new-chat/new-chat';
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
  {
    path: 'profile',
    component: ProfilePanel,
    canActivate: [authGuard],

  },
  { path: 'chat',
    component: MainLayout,
    canActivate: [authGuard],
  },
  {
    path: 'setting',
    component: SettingsPanel,
    canActivate: [authGuard],
  },
  {
    path: 'new-chat',
    component: NewChat,
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