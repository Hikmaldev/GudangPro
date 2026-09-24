import { Injectable, signal } from '@angular/core';
export type ToastKind = 'default' | 'success' | 'error' | 'info';
export interface Toast { id: number; message: string; kind: ToastKind; }
@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<Toast[]>([]);
  private nextId = 0;
  show(message: string, kind: ToastKind = 'default'): void { const id = ++this.nextId; this.toasts.update(items => [...items, { id, message, kind }]); setTimeout(() => this.toasts.update(items => items.filter(item => item.id !== id)), 2800); }
}
