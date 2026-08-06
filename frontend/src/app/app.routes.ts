import { Routes } from '@angular/router';
import { ProfilePanel } from './features/profile/profile-panel/profile-panel';
import { MainLayout } from './layout/main-layout/main-layout';
import { SettingsPanel } from './features/profile/settings-panel/settings-panel';
import { NewChat } from './features/new-chat/new-chat';

export const routes: Routes = [
  {
    path: 'profile',
    component: ProfilePanel,
  },
  { path: 'chat',
    component: MainLayout,
  },
  {
    path: 'Settings',
    component: SettingsPanel,
  },
  {
    path: 'new-chat',
    component: NewChat,
  },
  
  { path: '', redirectTo: 'chat', pathMatch: 'full' },
];