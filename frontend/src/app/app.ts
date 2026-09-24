import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ToastHostComponent } from './shared/components/toast-host/toast-host.component';

/**
 * Komponen root. Hanya merender outlet rute + host notifikasi global.
 * Layout shell (sidebar/topbar) berada di ShellComponent untuk rute terautentikasi.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ToastHostComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
