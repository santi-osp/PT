import { Component, DestroyRef, ElementRef, HostListener, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AssistantApi } from '../core/assistant-api';
import { AssistantContext, AssistantMessageResponse } from '../core/models';

interface ChatMessage {
  id: number;
  role: 'user' | 'assistant';
  text: string;
  result?: AssistantMessageResponse;
  resolved?: boolean;
}

@Component({
  selector: 'app-assistant-chat',
  imports: [FormsModule, RouterLink],
  template: `
    <button type="button" class="assistant-launcher" [class.assistant-launcher--hidden]="open()" (click)="show()" aria-label="Abrir Asistente DataGo">
      <span class="assistant-launcher__mark" aria-hidden="true">D</span><span>Asistente DataGo</span>
    </button>

    @if (open()) {
      <button type="button" class="assistant-scrim" aria-label="Cerrar asistente" (click)="close()"></button>
      <aside class="assistant-drawer" role="dialog" aria-modal="true" aria-labelledby="assistant-title">
        <header class="assistant-header">
          <span class="assistant-avatar" aria-hidden="true">D</span>
          <div><h2 id="assistant-title">Asistente DataGo</h2><p><span></span> Consulta segura de clientes</p></div>
          <button type="button" class="assistant-close" (click)="close()" aria-label="Cerrar asistente">×</button>
        </header>

        <div #conversation class="assistant-conversation" aria-live="polite" [attr.aria-busy]="loading()">
          @if (!messages().length) {
            <section class="assistant-welcome">
              <span class="assistant-welcome__seal" aria-hidden="true">DG</span>
              <h3>¿Qué necesitas consultar?</h3>
              <p>Puedo buscar, contar y consultar clientes. Para modificar o retirar, siempre te pediré confirmación.</p>
              <div class="assistant-prompts" aria-label="Consultas sugeridas">
                @for (prompt of quickPrompts; track prompt) {
                  <button type="button" (click)="send(prompt)">{{ prompt }}</button>
                }
              </div>
            </section>
          }

          @for (item of messages(); track item.id) {
            <article class="chat-message" [class.chat-message--user]="item.role === 'user'">
              <p>{{ item.text }}</p>
              @if (item.result?.customers?.length) {
                <div class="assistant-customer-list">
                  @for (card of item.result!.customers!; track card.customer.id) {
                    <section class="assistant-customer-card">
                      <div class="assistant-customer-card__top"><code>{{ card.customer.code }}</code><span class="badge" [class.badge--blocked]="card.customer.isBlocked"><span></span>{{ card.customer.isBlocked ? 'Bloqueado' : 'Activo' }}</span></div>
                      <strong>{{ card.customer.fullName }}</strong><small>{{ card.customer.businessName }}</small>
                      <dl><div><dt>Barrio</dt><dd>{{ card.neighborhood || 'Sin barrio' }}</dd></div><div><dt>Estrato</dt><dd>{{ card.customer.stratum }}</dd></div></dl>
                      <a [routerLink]="['/clientes', card.customer.id]" (click)="close()">Ver cliente <span aria-hidden="true">→</span></a>
                    </section>
                  }
                </div>
              }
              @if (item.result?.responseType === 'Count') {
                <section class="assistant-count-card"><strong>{{ item.result?.count }}</strong><span>Clientes encontrados</span></section>
              }
              @if (item.result?.pendingAction; as pending) {
                <section class="assistant-confirm-card" [class.assistant-confirm-card--retire]="pending.actionType === 'RETIRE_CUSTOMER'">
                  <span class="assistant-confirm-card__label">Confirmación requerida</span>
                  <strong>{{ pending.customerName }}</strong><code>{{ pending.customerCode }} · {{ pending.document }}</code>
                  @for (change of pending.changes; track change.field) {
                    <div class="assistant-change"><span>{{ change.field }}</span><del>{{ change.previousValue || 'Sin dato' }}</del><span aria-hidden="true">→</span><ins>{{ change.proposedValue || 'Sin dato' }}</ins></div>
                  }
                  <p>{{ pending.summary }}</p>
                  <div class="assistant-confirm-card__actions">
                    <button type="button" class="button button--ghost" [disabled]="confirming() || item.resolved" (click)="cancel(item.id)">Cancelar</button>
                    <button type="button" class="button" [class.button--danger]="pending.actionType === 'RETIRE_CUSTOMER'" [class.button--primary]="pending.actionType !== 'RETIRE_CUSTOMER'" [disabled]="confirming() || item.resolved" (click)="confirm(item.id, pending.token)">{{ confirming() ? 'Confirmando…' : pending.actionType === 'RETIRE_CUSTOMER' ? 'Confirmar retiro' : 'Confirmar cambio' }}</button>
                  </div>
                </section>
              }
              @if (item.result?.openCreateForm) { <a class="assistant-create-link button button--secondary" routerLink="/clientes/nuevo" (click)="close()">Abrir formulario de creación</a> }
            </article>
          }
          @if (loading()) { <div class="assistant-typing" aria-label="El asistente está respondiendo"><span></span><span></span><span></span></div> }
        </div>

        <form class="assistant-composer" (ngSubmit)="send()">
          <label class="sr-only" for="assistant-message">Mensaje para el asistente</label>
          <textarea #messageInput id="assistant-message" name="message" [(ngModel)]="draft" rows="1" maxlength="1000" placeholder="Escribe una consulta…" (keydown.enter)="onEnter($event)"></textarea>
          <button type="submit" [disabled]="loading() || !draft.trim()" aria-label="Enviar mensaje">↑</button>
          <small>La IA interpreta; DataGo valida y ejecuta.</small>
        </form>
      </aside>
    }
  `
})
export class AssistantChat {
  private readonly api = inject(AssistantApi);
  private readonly destroyRef = inject(DestroyRef);
  @ViewChild('conversation') conversation?: ElementRef<HTMLElement>;
  @ViewChild('messageInput') messageInput?: ElementRef<HTMLTextAreaElement>;
  readonly open = signal(false);
  readonly loading = signal(false);
  readonly confirming = signal(false);
  readonly messages = signal<ChatMessage[]>([]);
  readonly quickPrompts = ['Clientes de Aranjuez', 'Clientes activos', 'Clientes bloqueados', 'Empresas de El Poblado', '¿Cuántos clientes tengo?', 'Buscar por documento'];
  draft = '';
  private context: AssistantContext | null = null;
  private sequence = 0;

  show(): void { this.open.set(true); window.setTimeout(() => this.messageInput?.nativeElement.focus()); }
  close(): void { this.open.set(false); }
  @HostListener('document:keydown.escape') onEscape(): void { if (this.open()) this.close(); }
  onEnter(event: Event): void {
    const keyboard = event as KeyboardEvent;
    if (!keyboard.shiftKey) { keyboard.preventDefault(); this.send(); }
  }

  send(value?: string): void {
    const message = (value ?? this.draft).trim();
    if (!message || this.loading()) return;
    this.draft = '';
    this.messages.update((items) => [...items, { id: ++this.sequence, role: 'user', text: message }]);
    this.loading.set(true); this.scroll();
    this.api.message({ message, context: this.context }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.context = result.context ?? this.context;
        this.messages.update((items) => [...items, { id: ++this.sequence, role: 'assistant', text: result.message, result }]);
        this.loading.set(false); this.scroll();
      },
      error: (error: Error) => {
        this.messages.update((items) => [...items, { id: ++this.sequence, role: 'assistant', text: error.message || 'No fue posible contactar al asistente.' }]);
        this.loading.set(false); this.scroll();
      }
    });
  }

  confirm(messageId: number, token: string): void {
    if (this.confirming()) return;
    this.confirming.set(true);
    this.api.confirm(token).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.resolve(messageId);
        this.context = result.context ?? this.context;
        this.messages.update((items) => [...items, { id: ++this.sequence, role: 'assistant', text: result.message, result }]);
        this.confirming.set(false); this.scroll();
      },
      error: (error: Error) => {
        this.messages.update((items) => [...items, { id: ++this.sequence, role: 'assistant', text: error.message || 'No fue posible confirmar la operación.' }]);
        this.confirming.set(false); this.scroll();
      }
    });
  }

  cancel(messageId: number): void {
    this.resolve(messageId);
    this.messages.update((items) => [...items, { id: ++this.sequence, role: 'assistant', text: 'Operación cancelada. No se modificó ningún dato.' }]);
    this.scroll();
  }

  private resolve(messageId: number): void {
    this.messages.update((items) => items.map((item) => item.id === messageId ? { ...item, resolved: true } : item));
  }
  private scroll(): void { window.setTimeout(() => this.conversation?.nativeElement.scrollTo({ top: this.conversation.nativeElement.scrollHeight, behavior: 'smooth' })); }
}
