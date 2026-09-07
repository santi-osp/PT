import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'error' | 'warning' | 'info';
export interface ToastMessage { id: number; kind: ToastKind; title: string; message?: string }

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly messages = signal<ToastMessage[]>([]);
  private nextId = 0;

  show(kind: ToastKind, title: string, message?: string): void {
    const item = { id: ++this.nextId, kind, title, message };
    this.messages.update((items) => [...items, item]);
    window.setTimeout(() => this.dismiss(item.id), 5000);
  }

  success(title: string, message?: string): void { this.show('success', title, message); }
  error(title: string, message?: string): void { this.show('error', title, message); }
  warning(title: string, message?: string): void { this.show('warning', title, message); }
  info(title: string, message?: string): void { this.show('info', title, message); }
  dismiss(id: number): void { this.messages.update((items) => items.filter((item) => item.id !== id)); }
}
