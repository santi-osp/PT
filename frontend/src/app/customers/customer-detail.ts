import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CustomerApi } from '../core/customer-api';
import { Center, Customer, Neighborhood } from '../core/models';
import { centerLabel, displayName, documentLabel, geography } from '../core/formatters';
import { ConfirmDialog } from '../shared/confirm-dialog';
import { ToastService } from '../shared/toast.service';

@Component({
  imports: [ConfirmDialog],
  template: `
    <section class="page page-enter">
      @if (loading()) {
        <div class="detail-skeleton"><div class="skeleton-block"></div><div class="skeleton-grid"><div></div><div></div><div></div><div></div></div></div>
      } @else if (error()) {
        <section class="state-panel"><span class="state-panel__icon">↻</span><h1>No pudimos cargar el cliente</h1><p>{{ error() }}</p><button type="button" class="button button--secondary" (click)="load()">Reintentar</button></section>
      } @else if (customer(); as item) {
        <header class="detail-heading">
          <button type="button" class="back-link" (click)="router.navigate(['/clientes'])">← Volver a clientes</button>
          <div class="detail-heading__row"><div><div class="detail-heading__meta"><span class="code-pill">{{ item.code }}</span><span class="badge" [class.badge--blocked]="item.isBlocked"><span></span>{{ item.isBlocked ? 'Bloqueado' : 'Activo' }}</span></div><h1 tabindex="-1">{{ name(item) }}</h1><p>{{ item.businessName }}</p></div>
          @if (!item.isBlocked) {<div class="page-actions"><button type="button" class="button button--secondary" (click)="router.navigate(['/clientes', item.id, 'editar'])">Editar cliente</button><button type="button" class="button button--danger-soft" (click)="retireOpen = true">Retirar</button></div>}</div>
        </header>
        @if (item.isBlocked) {<div class="blocked-banner" role="status"><span aria-hidden="true">!</span><div><strong>Cliente bloqueado</strong><p>Este registro fue retirado{{ item.blockedAt ? ' el ' + formatDate(item.blockedAt) : '' }} y permanece disponible para consulta. No admite modificaciones.</p></div></div>}

        <div class="detail-grid">
          <section class="info-card"><div class="section-title"><span>01</span><div><p class="eyebrow">Identidad</p><h2>Información general</h2></div></div><dl class="info-list"><div><dt>Tratamiento</dt><dd>{{ treatment(item.treatment) }}</dd></div><div><dt>Nombre completo</dt><dd>{{ item.fullName }}</dd></div><div><dt>Razón social</dt><dd>{{ item.businessName }}</dd></div><div><dt>Nombre legal extendido</dt><dd>{{ item.extendedLegalName }}</dd></div><div><dt>Documento</dt><dd>{{ document(item) }}</dd></div></dl></section>
          <section class="info-card"><div class="section-title"><span>02</span><div><p class="eyebrow">Contacto</p><h2>Comunicación</h2></div></div><dl class="info-list"><div><dt>Teléfono</dt><dd>{{ item.phone || 'No informado' }}{{ item.phoneExtension ? ' · Ext. ' + item.phoneExtension : '' }}</dd></div><div><dt>Celular</dt><dd>{{ item.mobilePhone || 'No informado' }}</dd></div><div><dt>Email</dt><dd>{{ item.email || 'No informado' }}</dd></div></dl></section>
          <section class="info-card info-card--wide"><div class="section-title"><span>03</span><div><p class="eyebrow">Ubicación</p><h2>Dirección</h2></div></div><dl class="info-list info-list--grid"><div><dt>Dirección formateada</dt><dd>{{ item.address.formattedAddress }}</dd></div><div><dt>Barrio</dt><dd>{{ geo(item)?.name || 'No disponible' }}</dd></div><div><dt>Municipio</dt><dd>{{ geo(item)?.municipality || 'No disponible' }}</dd></div><div><dt>Departamento</dt><dd>{{ geo(item)?.department || 'No disponible' }}</dd></div><div><dt>País</dt><dd>{{ geo(item)?.country || 'No disponible' }}</dd></div><div><dt>Zona de transporte</dt><dd>{{ geo(item)?.transportZone || 'No disponible' }}</dd></div><div><dt>Tipo</dt><dd>{{ item.address.isRural ? 'Rural' : 'Urbana' }}</dd></div></dl></section>
          <section class="info-card"><div class="section-title"><span>04</span><div><p class="eyebrow">Tributario</p><h2>Información fiscal</h2></div></div><dl class="info-list"><div><dt>Clase fiscal</dt><dd><span class="readonly-value">{{ taxClass(item.taxClass) }}</span></dd></div><div><dt>Tipo de documento</dt><dd>{{ item.documentType }}</dd></div><div><dt>Dígito de verificación</dt><dd>{{ item.verificationDigit || 'No aplica' }}</dd></div></dl></section>
          <section class="info-card"><div class="section-title"><span>05</span><div><p class="eyebrow">Asignación</p><h2>Información comercial</h2></div></div><dl class="info-list"><div><dt>Centro</dt><dd>{{ center(item.centerId) }}</dd></div><div><dt>Estrato</dt><dd>{{ item.stratum }}</dd></div><div><dt>Condición de pago</dt><dd><span class="readonly-value">{{ item.paymentCondition }}</span></dd></div><div><dt>Última actualización</dt><dd>{{ formatDate(item.updatedAt) }}</dd></div></dl></section>
        </div>
        <app-confirm-dialog [(open)]="retireOpen" [busy]="retiring()" [customerName]="name(item)" [code]="item.code" [document]="document(item)" (confirmed)="retire()" />
      }
    </section>
  `
})
export class CustomerDetail {
  private readonly api = inject(CustomerApi);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  readonly router = inject(Router);
  readonly customer = signal<Customer | null>(null);
  readonly centers = signal<Center[]>([]);
  readonly neighborhoods = signal<Neighborhood[]>([]);
  readonly loading = signal(true);
  readonly retiring = signal(false);
  readonly error = signal('');
  retireOpen = false;
  constructor() { this.load(); }

  load(): void {
    this.loading.set(true); this.error.set('');
    const id = this.route.snapshot.paramMap.get('id')!;
    forkJoin({ customer: this.api.get(id), centers: this.api.centers(), neighborhoods: this.api.neighborhoods() }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: ({ customer, centers, neighborhoods }) => { this.customer.set(customer); this.centers.set(centers); this.neighborhoods.set(neighborhoods); this.loading.set(false); },
      error: (error) => { this.error.set(error.message); this.loading.set(false); }
    });
  }
  name = displayName;
  document = documentLabel;
  center = (id: string) => centerLabel(id, this.centers());
  geo = (item: Customer) => geography(item.address.neighborhoodId, this.neighborhoods());
  treatment(value: string): string { return value === 'Sr' ? 'Señor' : value === 'Sra' ? 'Señora' : 'Empresa'; }
  taxClass(value: string): string { return value === 'PersonaJuridica' ? 'Persona jurídica' : 'Persona natural'; }
  formatDate(value: string): string { return new Intl.DateTimeFormat('es-CO', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)); }
  retire(): void {
    const item = this.customer(); if (!item) return;
    this.retiring.set(true);
    this.api.retire(item.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: (updated) => { this.customer.set(updated); this.retiring.set(false); this.retireOpen = false; this.toast.success('Cliente retirado', 'El registro quedó bloqueado y continúa disponible para consulta.'); window.setTimeout(() => document.querySelector<HTMLElement>('main h1')?.focus()); }, error: (error) => { this.retiring.set(false); this.toast.error('No fue posible retirar el cliente', error.message); } });
  }
}
