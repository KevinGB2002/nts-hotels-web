import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { vi } from 'vitest';
import { Hotel } from '../../models/hotel.model';
import { AuthService } from '../../services/auth.service';
import { HotelService } from '../../services/hotel.service';
import { NotificationService } from '../../services/notification.service';
import { AdminComponent } from './admin.component';

describe('AdminComponent save messages', () => {
  const hotel: Hotel = {
    id: '1', name: 'Hotel', description: 'Descripción', address: 'Dirección',
    city: 'Ciudad', rating: 3, isActive: true
  };
  const service = {
    getHotels: vi.fn(),
    createHotel: vi.fn(),
    updateHotel: vi.fn(),
    deleteHotel: vi.fn()
  };

  beforeEach(() => {
    vi.resetAllMocks();
    service.getHotels.mockReturnValue(of([hotel]));
    TestBed.configureTestingModule({
      imports: [AdminComponent],
      providers: [
        provideRouter([]),
        { provide: HotelService, useValue: service },
        { provide: AuthService, useValue: {} }
      ]
    });
  });

  it('shows a confirmation after creating and editing a hotel', () => {
    service.createHotel.mockReturnValue(of(hotel));
    service.updateHotel.mockReturnValue(of(undefined));
    const fixture = TestBed.createComponent(AdminComponent);
    const component = fixture.componentInstance;
    component.openCreate();
    component.form.patchValue(hotel);
    component.save();
    fixture.detectChanges();
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.message).toContain('Se guardó correctamente el hotel.');

    component.openEdit(hotel);
    component.save();
    fixture.detectChanges();
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.message).toContain('Se actualizó correctamente el hotel.');
  });

  it('shows an error in the open form when saving fails', () => {
    service.updateHotel.mockReturnValue(throwError(() => new Error('failed')));
    const fixture = TestBed.createComponent(AdminComponent);
    const component = fixture.componentInstance;
    component.openEdit(hotel);
    component.save();
    fixture.detectChanges();
    expect(component.modalOpen()).toBe(true);
    expect(fixture.nativeElement.querySelector('.modal [role="alert"]').textContent).toContain('No se pudo guardar el hotel.');
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.type).toBe('error');
  });

  it('notifies after deletion only when the API succeeds', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    service.deleteHotel.mockReturnValue(of(undefined));
    const component = TestBed.createComponent(AdminComponent).componentInstance;
    component.remove(hotel);
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.message).toContain('Se eliminó correctamente');
    service.deleteHotel.mockReturnValue(throwError(() => new Error('failed')));
    component.remove(hotel);
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.type).toBe('error');
    vi.restoreAllMocks();
  });

  it('does not save after an oversized image is selected', () => {
    const component = TestBed.createComponent(AdminComponent).componentInstance;
    component.openEdit(hotel);
    const file = new File([new Uint8Array(10 * 1024 * 1024 + 1)], 'big.png');
    const input = document.createElement('input');
    input.type = 'file';
    Object.defineProperty(input, 'files', { value: [file] });
    input.addEventListener('change', event => component.selectImage(event));
    input.dispatchEvent(new Event('change'));
    component.save();
    expect(service.updateHotel).not.toHaveBeenCalled();
    expect(TestBed.inject(NotificationService).notifications().at(-1)?.type).toBe('error');
  });
});
