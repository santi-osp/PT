import { Component, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { ToastStack } from '../shared/toast-stack';

@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ToastStack],
  template: `
    <a class="skip-link" href="#main-content">Saltar al contenido</a>
    <div class="app-shell">
      <aside class="sidebar" [class.sidebar--open]="menuOpen()" aria-label="Navegación principal">
        <a routerLink="/clientes" class="brand" (click)="closeMenu()"><span class="brand__symbol">D</span><span><strong>DataGo</strong><small>Maestros confiables</small></span></a>
        <nav>
          <p class="sidebar__section">Gestión de clientes</p>
          <a routerLink="/clientes" routerLinkActive="nav-link--active" [routerLinkActiveOptions]="{exact:true}" class="nav-link" (click)="closeMenu()"><span aria-hidden="true">▦</span> Clientes</a>
          <a routerLink="/clientes/nuevo" routerLinkActive="nav-link--active" class="nav-link" (click)="closeMenu()"><span aria-hidden="true">＋</span> Nuevo cliente</a>
        </nav>
        <div class="sidebar__footer"><span class="system-dot"></span><span><strong>DataGo</strong><small>Gestión de datos maestros</small></span></div>
      </aside>
      @if (menuOpen()) { <button class="sidebar-scrim" type="button" aria-label="Cerrar menú" (click)="closeMenu()"></button> }
      <div class="workspace">
        <header class="topbar">
          <button type="button" class="menu-button" (click)="menuOpen.set(!menuOpen())" aria-label="Abrir navegación" [attr.aria-expanded]="menuOpen()">☰</button>
          <div><p class="breadcrumb">DataGo / Gestión de clientes</p><strong>Clientes residenciales</strong></div>
          <div class="system-status"><span class="system-dot"></span><span><small>Sistema</small><strong>Operativo</strong></span></div>
        </header>
        <main id="main-content"><router-outlet /></main>
      </div>
    </div>
    <app-toast-stack />
  `
})
export class AppShell {
  readonly menuOpen = signal(false);
  constructor(router: Router) {
    router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => this.closeMenu());
  }
  closeMenu(): void { this.menuOpen.set(false); }
}
