import { Component, inject } from '@angular/core';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast-stack',
  template: `
    <section class="toast-stack" aria-label="Notificaciones" aria-live="polite">
      @for (toast of service.messages(); track toast.id) {
        <article class="toast" [class]="'toast toast--' + toast.kind">
          <span class="toast__mark" aria-hidden="true"></span>
          <div><strong>{{ toast.title }}</strong>@if (toast.message) {<p>{{ toast.message }}</p>}</div>
          <button type="button" class="icon-button" (click)="service.dismiss(toast.id)" aria-label="Cerrar notificación">×</button>
        </article>
      }
    </section>
  `
})
export class ToastStack { readonly service = inject(ToastService); }
