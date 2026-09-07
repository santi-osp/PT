import { Component, ElementRef, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  template: `
    @if (open) {
      <div class="dialog-backdrop" (click)="cancel()">
        <section #panel class="dialog" role="dialog" aria-modal="true" aria-labelledby="retire-title" aria-describedby="retire-copy" tabindex="-1" (click)="$event.stopPropagation()" (keydown)="handleKeydown($event)">
          <div class="dialog__icon" aria-hidden="true">!</div>
          <div>
            <p class="eyebrow">Retiro lógico</p>
            <h2 id="retire-title">Retirar cliente</h2>
          </div>
          <p id="retire-copy">{{ customerName }} · {{ code }} · {{ document }}</p>
          <p>El registro seguirá disponible para consulta, pero quedará bloqueado y no podrá modificarse ni retirarse nuevamente.</p>
          <div class="dialog__actions">
            <button type="button" class="button button--ghost" [disabled]="busy" (click)="cancel()">Cancelar</button>
            <button type="button" class="button button--danger" [disabled]="busy" (click)="confirmed.emit()">{{ busy ? 'Retirando…' : 'Sí, retirar cliente' }}</button>
          </div>
        </section>
      </div>
    }
  `
})
export class ConfirmDialog implements OnChanges {
  @Input() open = false;
  @Input() busy = false;
  @Input() customerName = '';
  @Input() code = '';
  @Input() document = '';
  @Output() openChange = new EventEmitter<boolean>();
  @Output() confirmed = new EventEmitter<void>();
  @ViewChild('panel') panel?: ElementRef<HTMLElement>;
  private opener?: HTMLElement;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['open']?.currentValue) {
      this.opener = document.activeElement as HTMLElement;
      window.setTimeout(() => this.panel?.nativeElement.focus());
    }
  }

  cancel(): void {
    if (this.busy) return;
    this.openChange.emit(false);
    window.setTimeout(() => this.opener?.focus());
  }

  handleKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') { this.cancel(); return; }
    if (event.key !== 'Tab' || !this.panel) return;
    const focusable = Array.from(this.panel.nativeElement.querySelectorAll<HTMLElement>('button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'));
    if (!focusable.length) { event.preventDefault(); return; }
    const first = focusable[0]; const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
  }
}
