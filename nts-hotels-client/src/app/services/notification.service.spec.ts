import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  it('displays and dismisses notifications independently', () => {
    vi.useFakeTimers();
    try {
      const service = TestBed.inject(NotificationService);
      service.success('Guardado');
      service.error('Error');
      expect(service.notifications().map(item => item.message)).toEqual(['Guardado', 'Error']);
      vi.advanceTimersByTime(5000);
      expect(service.notifications().map(item => item.message)).toEqual(['Error']);
      service.dismiss(service.notifications()[0].id);
      expect(service.notifications()).toEqual([]);
    } finally {
      vi.useRealTimers();
    }
  });
});
