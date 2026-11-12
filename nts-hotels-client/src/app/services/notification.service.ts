import { Injectable, signal } from '@angular/core';

export interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  readonly notifications = signal<Notification[]>([]);
  private nextId = 0;

  success(message: string): void {
    this.show(message, 'success');
  }

  error(message: string): void {
    this.show(message, 'error');
  }

  dismiss(id: number): void {
    this.notifications.update(items => items.filter(item => item.id !== id));
  }

  private show(message: string, type: Notification['type']): void {
    const id = this.nextId++;
    this.notifications.update(items => [...items, { id, message, type }]);
    setTimeout(() => this.dismiss(id), type === 'error' ? 8000 : 5000);
  }
}
