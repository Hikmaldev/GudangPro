import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';
@Component({ selector: 'app-toast-host', changeDetection: ChangeDetectionStrategy.OnPush, template: `<div class="toast-host" aria-live="polite">@for (toast of toastService.toasts(); track toast.id) {<div class="toast" [class.success]="toast.kind === 'success'" [class.error]="toast.kind === 'error'">{{ toast.message }}</div>}</div>` })
export class ToastHostComponent { readonly toastService = inject(ToastService); }
