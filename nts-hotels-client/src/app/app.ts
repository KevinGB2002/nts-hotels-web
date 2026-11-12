import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NotificationService } from './services/notification.service';

@Component({
  imports: [RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('nts-hotels-client');
  protected readonly notifications = inject(NotificationService);
}
