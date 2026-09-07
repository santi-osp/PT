import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { debounceTime, distinctUntilChanged, Subject, switchMap, tap } from 'rxjs';
import { CustomerApi } from '../core/customer-api';
import { Center, Customer, CustomerStatus } from '../core/models';
import { centerLabel, displayName, documentLabel } from '../core/formatters';
import { ConfirmDialog } from '../shared/confirm-dialog';
import { ToastService } from '../shared/toast.service';

@Component({
  imports: [FormsModule, RouterLink, ConfirmDialog],
  template: `
    <section class="page page-enter">
      <header class="page-heading">
        <div><p class="eyebrow">Maestro comercial</p><h1 tabindex="-1">Clientes residenciales</h1><p>Consulta, crea y administra los clientes del canal residencial.</p></div>
        <a routerLink="/clientes/nuevo" class="button button--primary"><span aria-hidden="true">＋</span> Nuevo cliente</a>
      </header>

      <section class="toolbar" aria-label="Búsqueda y filtros">
        <label class="search-field"><span class="sr-only">Buscar clientes</span><span aria-hidden="true">⌕</span><input type="search" [(ngModel)]="query" (ngModelChange)="searchChanged.next($event)" placeholder="Buscar por código, nombre, razón social o documento" /></label>
        <div class="segmented" role="group" aria-label="Filtrar por estado">
          @for (option of statuses; track option.value) {
            <button type="button" [class.segmented__active]="status() === option.value" (click)="setStatus(option.value)">{{ option.label }}</button>
          }
        </div>
      </section>

      @if (loading()) {
        <div class="table-shell" aria-busy="true" aria-label="Cargando clientes">
          @for (row of skeletonRows; track row) { <div class="skeleton-row"><span></span><span></span><span></span><span></span></div> }
        </div>
      } @else if (error()) {
        <section class="state-panel"><span class="state-panel__icon" aria-hidden="true">↻</span><h2>No pudimos cargar los clientes</h2><p>{{ error() }}</p><button type="button" class="button button--secondary" (click)="load()">Reintentar</button></section>
      } @else if (!customers().length && (query || status() !== 'all')) {
        <section class="state-panel"><span class="state-panel__icon" aria-hidden="true">⌕</span><h2>Sin coincidencias</h2><p>Prueba con otra búsqueda o limpia los filtros aplicados.</p><button type="button" class="button button--secondary" (click)="clearFilters()">Limpiar filtros</button></section>
      } @else if (!customers().length) {
        <section class="state-panel"><span class="state-panel__icon" aria-hidden="true">＋</span><h2>Aún no hay clientes</h2><p>Crea el primer registro residencial para comenzar.</p><a routerLink="/clientes/nuevo" class="button button--primary">Crear cliente</a></section>
      } @else {
        <div class="results-meta" aria-live="polite"><strong>{{ customers().length }}</strong> {{ customers().length === 1 ? 'registro' : 'registros' }} <span>·</span> {{ activeFilterLabel() }}</div>
        <div class="table-shell desktop-table">
          <table>
            <thead><tr><th>Código</th><th>Cliente</th><th>Documento</th><th>Centro</th><th>Estrato</th><th>Estado</th><th><span class="sr-only">Acciones</span></th></tr></thead>
            <tbody>
              @for (customer of customers(); track customer.id) {
                <tr>
                  <td><a [routerLink]="['/clientes', customer.id]" class="code-link">{{ customer.code }}</a></td>
                  <td><strong>{{ name(customer) }}</strong><small>{{ customer.businessName }}</small></td>
                  <td>{{ document(customer) }}</td><td>{{ center(customer.centerId) }}</td><td>{{ customer.stratum }}</td>
                  <td><span class="badge" [class.badge--blocked]="customer.isBlocked"><span></span>{{ customer.isBlocked ? 'Bloqueado' : 'Activo' }}</span></td>
                  <td><div class="row-actions"><a [routerLink]="['/clientes', customer.id]" class="text-link">Ver</a>@if (!customer.isBlocked) {<a [routerLink]="['/clientes', customer.id, 'editar']" class="text-link">Editar</a><button type="button" class="text-link text-link--danger" (click)="askRetire(customer)">Retirar</button>}</div></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <div class="mobile-list">
          @for (customer of customers(); track customer.id) {
            <article class="customer-row">
              <div class="customer-row__top"><a [routerLink]="['/clientes', customer.id]" class="code-link">{{ customer.code }}</a><span class="badge" [class.badge--blocked]="customer.isBlocked"><span></span>{{ customer.isBlocked ? 'Bloqueado' : 'Activo' }}</span></div>
              <h2>{{ name(customer) }}</h2><p>{{ document(customer) }}</p><dl><div><dt>Centro</dt><dd>{{ center(customer.centerId) }}</dd></div><div><dt>Estrato</dt><dd>{{ customer.stratum }}</dd></div></dl>
              <div class="row-actions"><a [routerLink]="['/clientes', customer.id]" class="text-link">Ver detalle</a>@if (!customer.isBlocked) {<a [routerLink]="['/clientes', customer.id, 'editar']" class="text-link">Editar</a><button type="button" class="text-link text-link--danger" (click)="askRetire(customer)">Retirar</button>}</div>
            </article>
          }
        </div>
      }
    </section>

    <app-confirm-dialog [(open)]="retireOpen" [busy]="retiring()" [customerName]="selected() ? name(selected()!) : ''" [code]="selected()?.code ?? ''" [document]="selected() ? document(selected()!) : ''" (confirmed)="retire()" />
  `
})
export class CustomerList {
  private readonly api = inject(CustomerApi);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  readonly customers = signal<Customer[]>([]);
  readonly centers = signal<Center[]>([]);
  readonly loading = signal(true);
  readonly retiring = signal(false);
  readonly error = signal('');
  readonly status = signal<CustomerStatus>('all');
  readonly selected = signal<Customer | null>(null);
  readonly statuses: { value: CustomerStatus; label: string }[] = [{ value: 'all', label: 'Todos' }, { value: 'active', label: 'Activos' }, { value: 'blocked', label: 'Bloqueados' }];
  readonly skeletonRows = [1, 2, 3, 4, 5];
  readonly searchChanged = new Subject<string>();
  readonly activeFilterLabel = computed(() => this.status() === 'all' ? 'Todos los estados' : this.status() === 'active' ? 'Solo activos' : 'Solo bloqueados');
  query = '';
  retireOpen = false;

  constructor() {
    this.api.centers().pipe(takeUntilDestroyed()).subscribe({ next: (items) => this.centers.set(items) });
    this.searchChanged.pipe(debounceTime(300), distinctUntilChanged(), tap(() => { this.loading.set(true); this.error.set(''); }), switchMap((query) => this.api.search(query, this.status())), takeUntilDestroyed()).subscribe({ next: (items) => { this.customers.set(items); this.loading.set(false); }, error: (error) => { this.error.set(error.message); this.loading.set(false); } });
    this.load();
  }

  load(): void {
    this.loading.set(true); this.error.set('');
    this.api.search(this.query, this.status()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: (items) => { this.customers.set(items); this.loading.set(false); }, error: (error) => { this.error.set(error.message); this.loading.set(false); } });
  }
  setStatus(status: CustomerStatus): void { this.status.set(status); this.load(); }
  clearFilters(): void { this.query = ''; this.status.set('all'); this.load(); }
  name = displayName;
  document = documentLabel;
  center = (id: string) => centerLabel(id, this.centers());
  askRetire(customer: Customer): void { this.selected.set(customer); this.retireOpen = true; }
  retire(): void {
    const customer = this.selected(); if (!customer) return;
    this.retiring.set(true);
    this.api.retire(customer.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (updated) => { this.customers.update((items) => items.map((item) => item.id === updated.id ? updated : item)); this.retiring.set(false); this.retireOpen = false; this.toast.success('Cliente retirado', `${updated.code} quedó bloqueado para modificaciones.`); window.setTimeout(() => document.querySelector<HTMLElement>('main h1')?.focus()); },
      error: (error) => { this.retiring.set(false); this.toast.error('No fue posible retirar el cliente', error.message); }
    });
  }
}
