import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./layout/app-shell').then((m) => m.AppShell),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'clientes' },
      { path: 'clientes', loadComponent: () => import('./customers/customer-list').then((m) => m.CustomerList) },
      { path: 'clientes/nuevo', loadComponent: () => import('./customers/customer-form').then((m) => m.CustomerForm) },
      { path: 'clientes/:id/editar', loadComponent: () => import('./customers/customer-form').then((m) => m.CustomerForm) },
      { path: 'clientes/:id', loadComponent: () => import('./customers/customer-detail').then((m) => m.CustomerDetail) },
      { path: '**', loadComponent: () => import('./shared/not-found').then((m) => m.NotFound) }
    ]
  }
];
