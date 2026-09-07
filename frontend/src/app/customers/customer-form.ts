import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiError, CustomerApi } from '../core/customer-api';
import { AddressRequest, Center, CreateCustomerRequest, Customer, DocumentType, Neighborhood, Treatment, UpdateCustomerRequest } from '../core/models';
import { parseLegalName } from '../core/formatters';
import { ToastService } from '../shared/toast.service';

@Component({
  imports: [ReactiveFormsModule],
  template: `
    <section class="page page-enter form-page">
      <button type="button" class="back-link" (click)="router.navigate([isEdit ? '/clientes/' + customer()?.id : '/clientes'])">← {{ isEdit ? 'Volver al detalle' : 'Volver a clientes' }}</button>
      @if (loading()) {
        <div class="detail-skeleton"><div class="skeleton-block"></div><div class="skeleton-grid"><div></div><div></div><div></div><div></div></div></div>
      } @else if (loadError()) {
        <section class="state-panel"><span class="state-panel__icon">↻</span><h1>No pudimos preparar el formulario</h1><p>{{ loadError() }}</p><button type="button" class="button button--secondary" (click)="load()">Reintentar</button></section>
      } @else if (blocked()) {
        <section class="state-panel state-panel--warning"><span class="state-panel__icon">!</span><h1>Este cliente está bloqueado</h1><p>Los clientes retirados permanecen disponibles para consulta, pero no pueden modificarse.</p><button type="button" class="button button--secondary" (click)="router.navigate(['/clientes', customer()?.id])">Ir al detalle</button></section>
      } @else {
        <header class="page-heading form-heading"><div><p class="eyebrow">{{ isEdit ? 'Actualización controlada' : 'Nuevo registro' }}</p><h1>{{ isEdit ? 'Editar cliente residencial' : 'Crear cliente residencial' }}</h1><p>{{ isEdit ? 'Modifica únicamente los datos habilitados. La identidad fiscal permanece protegida.' : 'Completa la identidad, contacto, ubicación y asignación comercial.' }}</p></div><span class="required-note"><i>*</i> Campos obligatorios</span></header>

        @if (submitError()) {<div class="form-alert" role="alert"><strong>{{ errorTitle() }}</strong><p>{{ submitError() }}</p></div>}

        <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
          <section class="form-section">
            <div class="section-title"><span>01</span><div><p class="eyebrow">Identidad</p><h2>Información general</h2><p>Datos de presentación e identificación del cliente.</p></div></div>
            @if (!isEdit) {
              <fieldset class="field field--wide"><legend>Tratamiento <i>*</i></legend><div class="choice-group">@for (item of treatments; track item.value) {<label><input type="radio" formControlName="treatment" [value]="item.value" (change)="treatmentChanged(item.value)" /><span>{{ item.label }}</span></label>}</div></fieldset>
            } @else {
              <div class="identity-lock"><div><span>Tratamiento</span><strong>{{ treatmentLabel(customer()!.treatment) }}</strong></div><div><span>Documento</span><strong>{{ customer()!.documentType }} {{ customer()!.documentNumber }}{{ customer()!.verificationDigit ? '-' + customer()!.verificationDigit : '' }}</strong></div><small>Estos datos identifican el registro y no pueden editarse.</small></div>
            }
            <div class="form-grid">
              <label class="field"><span>Razón social <i>*</i></span><input formControlName="businessName" autocomplete="organization" [class.invalid]="invalid('businessName')" />@if (invalid('businessName')) {<small class="field-error">Ingresa la razón social.</small>}</label>
              @if (!isEdit) {<label class="field"><span>Nombre legal extendido <i>*</i></span><input formControlName="extendedLegalName" (input)="refreshPreview()" autocomplete="name" [class.invalid]="invalid('extendedLegalName')" />@if (invalid('extendedLegalName')) {<small class="field-error">Ingresa el nombre legal.</small>}</label>}
              @if (isEdit && customer()?.treatment !== 'Empresa') {
                <label class="field"><span>Nombres <i>*</i></span><input formControlName="firstNames" (input)="refreshPreview()" [class.invalid]="invalid('firstNames')" /><small>Separa nombres compuestos con coma si es necesario.</small></label>
                <label class="field"><span>Apellidos</span><input formControlName="lastNames" (input)="refreshPreview()" /></label>
              }
              @if (isEdit && customer()?.treatment === 'Empresa') {<label class="field field--wide"><span>Nombre legal <i>*</i></span><input formControlName="companyLegalName" (input)="refreshPreview()" [class.invalid]="invalid('companyLegalName')" /><small>DataGo mantiene este mismo valor como nombre y apellido legal para conservar la regla del backend.</small></label>}
            </div>
            <div class="preview-box"><span>Vista previa del nombre normalizado</span><strong>{{ namePreview() || 'Se mostrará aquí mientras escribes' }}</strong>@if (!isEdit && form.controls.treatment.value !== 'Empresa' && parsedPreview().fullName) {<small>Nombres: {{ parsedPreview().firstNames || '—' }} · Apellidos: {{ parsedPreview().lastNames || '—' }}</small>}</div>
          </section>

          <section class="form-section">
            <div class="section-title"><span>02</span><div><p class="eyebrow">Contacto</p><h2>Comunicación</h2><p>Registra al menos un teléfono o celular.</p></div></div>
            <div class="form-grid form-grid--3"><label class="field"><span>Teléfono</span><input formControlName="phone" inputmode="tel" autocomplete="tel" /></label><label class="field"><span>Extensión</span><input formControlName="phoneExtension" inputmode="numeric" /></label><label class="field"><span>Celular</span><input formControlName="mobilePhone" inputmode="tel" autocomplete="tel-national" /></label><label class="field field--wide"><span>Email @if (documentType() === 'NIT') {<i>*</i>}</span><input formControlName="email" type="email" autocomplete="email" [class.invalid]="invalid('email')" />@if (invalid('email')) {<small class="field-error">Ingresa un email válido{{ documentType() === 'NIT' ? ' y obligatorio para NIT' : '' }}.</small>}</label></div>
            @if (contactMissing()) {<p class="field-error standalone-error">Debes indicar un teléfono o un celular.</p>}
          </section>

          @if (!isEdit) {
            <section class="form-section">
              <div class="section-title"><span>03</span><div><p class="eyebrow">Tributario</p><h2>Identificación fiscal</h2><p>El tipo depende del tratamiento seleccionado.</p></div></div>
              <div class="form-grid form-grid--3"><label class="field"><span>Tipo de documento <i>*</i></span><select formControlName="documentType" (change)="documentChanged()">@for (type of documentTypes(); track type) {<option [value]="type">{{ type }}</option>}</select></label><label class="field"><span>Número de documento <i>*</i></span><input formControlName="documentNumber" inputmode="numeric" [class.invalid]="invalid('documentNumber')" />@if (invalid('documentNumber')) {<small class="field-error">Ingresa el número del documento.</small>}</label>@if (documentType() === 'NIT') {<label class="field"><span>Dígito de verificación <i>*</i></span><input formControlName="verificationDigit" inputmode="numeric" maxlength="1" [class.invalid]="invalid('verificationDigit')" /></label>}</div>
              <div class="readonly-strip"><div><span>Clase fiscal</span><strong>{{ taxClass() }}</strong></div><div><span>Condición de pago</span><strong>0010 Contado</strong></div></div>
            </section>
          }

          <section class="form-section">
            <div class="section-title"><span>{{ isEdit ? '03' : '04' }}</span><div><p class="eyebrow">Ubicación</p><h2>Dirección</h2><p>Selecciona el barrio y compón una dirección urbana o rural.</p></div></div>
            <div class="form-grid"><label class="field field--wide"><span>Barrio <i>*</i></span><input formControlName="neighborhoodName" list="neighborhoods" autocomplete="off" (input)="neighborhoodChanged()" [class.invalid]="invalid('neighborhoodId')" placeholder="Escribe para buscar un barrio" /><datalist id="neighborhoods">@for (item of filteredNeighborhoods(); track item.id) {<option [value]="item.name"></option>}</datalist>@if (invalid('neighborhoodId')) {<small class="field-error">Selecciona un barrio válido de la lista.</small>}</label></div>
            @if (selectedNeighborhood(); as place) {<div class="geography-strip"><div><span>Municipio</span><strong>{{ place.municipality }}</strong></div><div><span>Departamento</span><strong>{{ place.department }}</strong></div><div><span>País</span><strong>{{ place.country }}</strong></div><div><span>Zona</span><strong>{{ place.transportZone }}</strong></div></div>}
            <label class="switch-row"><input type="checkbox" formControlName="isRural" /><span class="switch" aria-hidden="true"></span><span><strong>Dirección rural</strong><small>Actívala para registrar vereda, finca o indicaciones.</small></span></label>
            @if (form.controls.isRural.value) {
              <label class="field"><span>Descripción de la dirección rural <i>*</i></span><textarea formControlName="ruralAddress" rows="3" [class.invalid]="invalid('ruralAddress')" placeholder="Ej. Vereda El Plan, finca Los Pinos"></textarea></label>
            } @else {
              <div class="address-grid"><label class="field"><span>Tipo de vía <i>*</i></span><select formControlName="mainRoadType"><option value="">Selecciona</option>@for (road of roadTypes; track road) {<option [value]="road">{{ road }}</option>}</select></label><label class="field"><span>Número vía principal <i>*</i></span><input formControlName="mainRoadNumber" /></label><label class="field"><span>Letra</span><input formControlName="mainRoadLetter" maxlength="3" /></label><label class="field"><span>Cardinalidad</span><select formControlName="mainRoadCardinality"><option value="">—</option>@for (cardinal of cardinals; track cardinal) {<option [value]="cardinal">{{ cardinal }}</option>}</select></label><label class="field"><span>Número vía secundaria <i>*</i></span><input formControlName="secondaryRoadNumber1" /></label><label class="field"><span>Letra secundaria</span><input formControlName="secondaryRoadLetter" maxlength="3" /></label><label class="field"><span>Cardinalidad</span><select formControlName="secondaryRoadCardinality1"><option value="">—</option>@for (cardinal of cardinals; track cardinal) {<option [value]="cardinal">{{ cardinal }}</option>}</select></label><label class="field"><span>Número de placa <i>*</i></span><input formControlName="secondaryRoadNumber2" /></label><label class="field"><span>Cardinalidad final</span><select formControlName="secondaryRoadCardinality2"><option value="">—</option>@for (cardinal of cardinals; track cardinal) {<option [value]="cardinal">{{ cardinal }}</option>}</select></label></div>
            }
            <div class="preview-box"><span>Vista previa de dirección</span><strong>{{ addressPreview() || 'Completa los campos para construirla' }}</strong></div>
          </section>

          <section class="form-section">
            <div class="section-title"><span>{{ isEdit ? '04' : '05' }}</span><div><p class="eyebrow">Asignación</p><h2>Información comercial</h2><p>Define el centro responsable y el estrato.</p></div></div>
            <div class="form-grid"><label class="field"><span>Centro <i>*</i></span><select formControlName="centerId" [class.invalid]="invalid('centerId')"><option value="">Selecciona un centro</option>@for (center of centers(); track center.id) {<option [value]="center.id">{{ center.code }} · {{ center.name }}</option>}</select></label><label class="field"><span>Estrato <i>*</i></span><select formControlName="stratum">@for (value of strata; track value) {<option [value]="value">{{ value }}</option>}</select></label></div>
          </section>

          <footer class="form-actions"><button type="button" class="button button--ghost" [disabled]="submitting()" (click)="router.navigate([isEdit ? '/clientes/' + customer()?.id : '/clientes'])">Cancelar</button><button type="submit" class="button button--primary" [disabled]="submitting()">{{ submitting() ? 'Guardando…' : isEdit ? 'Guardar cambios' : 'Crear cliente' }}</button></footer>
        </form>
      }
    </section>
  `
})
export class CustomerForm {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(CustomerApi);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  readonly router = inject(Router);
  readonly customer = signal<Customer | null>(null);
  readonly centers = signal<Center[]>([]);
  readonly neighborhoods = signal<Neighborhood[]>([]);
  readonly loading = signal(true);
  readonly loadError = signal('');
  readonly submitError = signal('');
  readonly errorTitle = signal('Revisa la información');
  readonly submitting = signal(false);
  readonly blocked = signal(false);
  readonly namePreview = signal('');
  readonly parsedPreview = signal(parseLegalName(''));
  readonly isEdit = !!this.route.snapshot.paramMap.get('id');
  readonly treatments = [{ value: 'Sr' as Treatment, label: 'Sr.' }, { value: 'Sra' as Treatment, label: 'Sra.' }, { value: 'Empresa' as Treatment, label: 'Empresa' }];
  readonly roadTypes = ['Calle', 'Carrera', 'Circular', 'Diagonal', 'Transversal', 'Avenida'];
  readonly cardinals = ['Norte', 'Sur', 'Este', 'Oeste'];
  readonly strata = [1, 2, 3, 4, 5, 6];

  readonly form = this.fb.group({
    treatment: ['Sr' as Treatment, Validators.required], businessName: ['', Validators.required], extendedLegalName: ['', Validators.required], firstNames: [''], lastNames: [''], companyLegalName: [''],
    phone: [''], phoneExtension: [''], mobilePhone: [''], email: ['', Validators.email], documentType: ['CC' as DocumentType, Validators.required], documentNumber: ['', Validators.required], verificationDigit: [''],
    neighborhoodId: ['', Validators.required], neighborhoodName: ['', Validators.required], isRural: [false], ruralAddress: [''], mainRoadType: [''], mainRoadNumber: [''], mainRoadLetter: [''], mainRoadCardinality: [''], secondaryRoadNumber1: [''], secondaryRoadLetter: [''], secondaryRoadCardinality1: [''], secondaryRoadNumber2: [''], secondaryRoadCardinality2: [''],
    centerId: ['', Validators.required], stratum: [1, [Validators.required, Validators.min(1), Validators.max(6)]]
  });

  constructor() {
    if (this.isEdit) {
      this.form.controls.extendedLegalName.clearValidators();
      this.form.controls.documentNumber.clearValidators();
      this.form.controls.documentType.clearValidators();
    }
    this.load();
    this.form.valueChanges.pipe(takeUntilDestroyed()).subscribe(() => { this.submitError.set(''); this.refreshPreview(); });
  }

  load(): void {
    this.loading.set(true); this.loadError.set('');
    const catalogs = { centers: this.api.centers(), neighborhoods: this.api.neighborhoods() };
    const source = this.isEdit ? forkJoin({ ...catalogs, customer: this.api.get(this.route.snapshot.paramMap.get('id')!) }) : forkJoin(catalogs);
    source.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: (result) => { this.centers.set(result.centers); this.neighborhoods.set(result.neighborhoods); if ('customer' in result) this.populate(result.customer as Customer); this.loading.set(false); this.refreshPreview(); }, error: (error) => { this.loadError.set(error.message); this.loading.set(false); } });
  }

  populate(item: Customer): void {
    this.customer.set(item); this.blocked.set(item.isBlocked);
    const place = this.neighborhoods().find((entry) => entry.id === item.address.neighborhoodId);
    this.form.patchValue({ businessName: item.businessName, firstNames: item.firstNames, lastNames: item.lastNames, companyLegalName: item.extendedLegalName, phone: item.phone || '', phoneExtension: item.phoneExtension || '', mobilePhone: item.mobilePhone || '', email: item.email || '', neighborhoodId: item.address.neighborhoodId, neighborhoodName: place?.name || '', isRural: item.address.isRural, ruralAddress: item.address.ruralAddress || '', mainRoadType: item.address.mainRoadType || '', mainRoadNumber: item.address.mainRoadNumber || '', mainRoadLetter: item.address.mainRoadLetter || '', mainRoadCardinality: item.address.mainRoadCardinality || '', secondaryRoadNumber1: item.address.secondaryRoadNumber1 || '', secondaryRoadLetter: item.address.secondaryRoadLetter || '', secondaryRoadCardinality1: item.address.secondaryRoadCardinality1 || '', secondaryRoadNumber2: item.address.secondaryRoadNumber2 || '', secondaryRoadCardinality2: item.address.secondaryRoadCardinality2 || '', centerId: item.centerId, stratum: item.stratum });
  }

  treatmentChanged(treatment: Treatment): void { this.form.patchValue({ documentType: treatment === 'Empresa' ? 'NIT' : 'CC', verificationDigit: '' }); this.documentChanged(); }
  documentChanged(): void { const control = this.form.controls.verificationDigit; this.documentType() === 'NIT' ? control.setValidators([Validators.required, Validators.pattern(/^\d$/)]) : control.clearValidators(); control.updateValueAndValidity(); const email = this.form.controls.email; this.documentType() === 'NIT' ? email.setValidators([Validators.required, Validators.email]) : email.setValidators([Validators.email]); email.updateValueAndValidity(); }
  documentType(): DocumentType { return this.form.controls.documentType.value || 'CC'; }
  documentTypes(): DocumentType[] { return this.form.controls.treatment.value === 'Empresa' ? ['NIT'] : ['CC', 'CE']; }
  taxClass(): string { return this.form.controls.treatment.value === 'Empresa' ? 'Persona jurídica' : 'Persona natural'; }
  treatmentLabel(value: Treatment): string { return value === 'Sr' ? 'Señor' : value === 'Sra' ? 'Señora' : 'Empresa'; }
  refreshPreview(): void { const treatment = this.isEdit ? this.customer()?.treatment : this.form.controls.treatment.value; if (treatment === 'Empresa') { const value = this.isEdit ? this.form.controls.companyLegalName.value || '' : this.form.controls.extendedLegalName.value || ''; this.namePreview.set(value.trim()); this.parsedPreview.set({ firstNames: value.trim(), lastNames: value.trim(), fullName: value.trim() }); } else if (this.isEdit) { const first = (this.form.controls.firstNames.value || '').split(',').map((x) => x.trim()).filter(Boolean); const last = (this.form.controls.lastNames.value || '').split(',').map((x) => x.trim()).filter(Boolean); this.namePreview.set([...first, ...last].join(' ')); this.parsedPreview.set({ firstNames: first.join(', '), lastNames: last.join(', '), fullName: [...first, ...last].join(' ') }); } else { const parsed = parseLegalName(this.form.controls.extendedLegalName.value || ''); this.parsedPreview.set(parsed); this.namePreview.set(parsed.fullName); } }
  filteredNeighborhoods(): Neighborhood[] { const query = (this.form.controls.neighborhoodName.value || '').toLocaleLowerCase('es'); return query ? this.neighborhoods().filter((item) => item.name.toLocaleLowerCase('es').includes(query)).slice(0, 12) : this.neighborhoods().slice(0, 12); }
  neighborhoodChanged(): void { const name = (this.form.controls.neighborhoodName.value || '').trim(); const match = this.neighborhoods().find((item) => item.name.localeCompare(name, 'es', { sensitivity: 'base' }) === 0); this.form.controls.neighborhoodId.setValue(match?.id || '', { emitEvent: false }); }
  selectedNeighborhood(): Neighborhood | undefined { return this.neighborhoods().find((item) => item.id === this.form.controls.neighborhoodId.value); }
  addressPreview(): string { const value = this.form.getRawValue(); if (value.isRural) return (value.ruralAddress || '').trim(); return [value.mainRoadType, value.mainRoadNumber, value.mainRoadLetter, value.mainRoadCardinality, value.secondaryRoadNumber1 ? `# ${value.secondaryRoadNumber1}` : '', value.secondaryRoadLetter, value.secondaryRoadCardinality1, value.secondaryRoadNumber2 ? `- ${value.secondaryRoadNumber2}` : '', value.secondaryRoadCardinality2].filter(Boolean).join(' '); }
  contactMissing(): boolean { return this.form.touched && !this.form.controls.phone.value?.trim() && !this.form.controls.mobilePhone.value?.trim(); }
  invalid(name: keyof CustomerForm['form']['controls']): boolean { const control = this.form.controls[name]; return control.invalid && (control.touched || this.submitting()); }

  private nullable(value: string | null | undefined): string | null { const normalized = value?.trim(); return normalized ? normalized : null; }
  private address(): AddressRequest { const value = this.form.getRawValue(); return { neighborhoodId: value.neighborhoodId!, isRural: !!value.isRural, ruralAddress: value.isRural ? this.nullable(value.ruralAddress) : null, mainRoadType: value.isRural ? null : this.nullable(value.mainRoadType), mainRoadNumber: value.isRural ? null : this.nullable(value.mainRoadNumber), mainRoadLetter: value.isRural ? null : this.nullable(value.mainRoadLetter), mainRoadCardinality: value.isRural ? null : this.nullable(value.mainRoadCardinality), secondaryRoadNumber1: value.isRural ? null : this.nullable(value.secondaryRoadNumber1), secondaryRoadLetter: value.isRural ? null : this.nullable(value.secondaryRoadLetter), secondaryRoadCardinality1: value.isRural ? null : this.nullable(value.secondaryRoadCardinality1), secondaryRoadNumber2: value.isRural ? null : this.nullable(value.secondaryRoadNumber2), secondaryRoadCardinality2: value.isRural ? null : this.nullable(value.secondaryRoadCardinality2) }; }
  private validateConditional(): boolean { const v = this.form.getRawValue(); if (!v.phone?.trim() && !v.mobilePhone?.trim()) return false; if (v.isRural && !v.ruralAddress?.trim()) return false; if (!v.isRural && (!v.mainRoadType || !v.mainRoadNumber?.trim() || !v.secondaryRoadNumber1?.trim() || !v.secondaryRoadNumber2?.trim())) return false; if (this.isEdit && this.customer()?.treatment === 'Empresa' && !v.companyLegalName?.trim()) return false; if (this.isEdit && this.customer()?.treatment !== 'Empresa' && !v.firstNames?.trim()) return false; return true; }

  submit(): void {
    this.form.markAllAsTouched(); this.documentChanged();
    if (this.form.invalid || !this.validateConditional()) { this.submitError.set('Hay campos obligatorios o inválidos. Revisa las secciones marcadas antes de continuar.'); this.errorTitle.set('Formulario incompleto'); document.querySelector<HTMLElement>('.invalid, .field-error')?.focus?.(); return; }
    this.submitting.set(true); this.submitError.set(''); const value = this.form.getRawValue();
    const operation = this.isEdit ? this.api.update(this.customer()!.id, { businessName: value.businessName!, firstNames: this.customer()!.treatment === 'Empresa' ? value.companyLegalName : this.nullable(value.firstNames), lastNames: this.customer()!.treatment === 'Empresa' ? value.companyLegalName : this.nullable(value.lastNames), phone: this.nullable(value.phone), phoneExtension: this.nullable(value.phoneExtension), mobilePhone: this.nullable(value.mobilePhone), email: this.nullable(value.email), stratum: Number(value.stratum), centerId: value.centerId!, address: this.address() } as UpdateCustomerRequest) : this.api.create({ treatment: value.treatment!, businessName: value.businessName!, extendedLegalName: value.extendedLegalName!, phone: this.nullable(value.phone), phoneExtension: this.nullable(value.phoneExtension), mobilePhone: this.nullable(value.mobilePhone), email: this.nullable(value.email), documentType: value.documentType!, documentNumber: value.documentNumber!, verificationDigit: this.documentType() === 'NIT' ? this.nullable(value.verificationDigit) : null, stratum: Number(value.stratum), centerId: value.centerId!, address: this.address() } as CreateCustomerRequest);
    operation.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: (saved) => { this.submitting.set(false); this.toast.success(this.isEdit ? 'Cambios guardados' : 'Cliente creado', `${saved.code} se guardó correctamente.`); this.router.navigate(['/clientes', saved.id]); }, error: (error: ApiError) => { this.submitting.set(false); const modern = error.message.toLocaleLowerCase('es').includes('canal moderno'); this.errorTitle.set(modern ? 'Documento en canal moderno' : error.status === 409 ? 'No se pudo completar la operación' : 'Revisa la información'); this.submitError.set(modern ? 'Este documento ya pertenece al canal moderno y no puede registrarse como cliente residencial.' : error.message); window.scrollTo({ top: 0, behavior: 'smooth' }); } });
  }
}
