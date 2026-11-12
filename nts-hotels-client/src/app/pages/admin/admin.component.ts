import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Hotel } from '../../models/hotel.model';
import { AuthService } from '../../services/auth.service';
import { HotelService } from '../../services/hotel.service';
import { NotificationService } from '../../services/notification.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-admin',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent implements OnInit {
  private readonly service = inject(HotelService);
  private readonly notifications = inject(NotificationService);
  readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder).nonNullable;

  readonly hotels = signal<Hotel[]>([]);
  readonly loading = signal(false);
  readonly modalOpen = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly saving = signal(false);
  readonly error = signal('');
  private image?: File;
  private imageInvalid = false;

  readonly form = this.fb.group({
    name: ['', Validators.required],
    description: ['', Validators.required],
    address: ['', Validators.required],
    city: ['', Validators.required],
    rating: [3, [Validators.required, Validators.min(1), Validators.max(5)]],
    isActive: [true]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getHotels().subscribe({
      next: hotels => {
        this.hotels.set(hotels);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('No se pudo cargar la lista de hoteles.');
        this.notifications.error('No se pudo cargar la lista de hoteles.');
        this.loading.set(false);
      }
    });
  }

  openCreate(): void {
    this.error.set('');
    this.image = undefined;
    this.imageInvalid = false;
    this.editingId.set(null);
    this.form.reset({ rating: 3, isActive: true });
    this.modalOpen.set(true);
  }

  openEdit(hotel: Hotel): void {
    this.error.set('');
    this.image = undefined;
    this.imageInvalid = false;
    this.editingId.set(hotel.id);
    this.form.reset(hotel);
    this.modalOpen.set(true);
  }

  close(): void {
    this.modalOpen.set(false);
  }

  selectImage(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.image = input.files?.[0];
    this.imageInvalid = false;
    if (this.image && this.image.size > 10 * 1024 * 1024) {
      this.image = undefined;
      this.imageInvalid = true;
      input.value = '';
      this.error.set('La imagen debe tener un tamaño máximo de 10 MB.');
      this.notifications.error(this.error());
    } else {
      this.error.set('');
    }
  }

  invalid(control: 'name' | 'description' | 'address' | 'city' | 'rating'): boolean {
    const c = this.form.controls[control];
    return c.invalid && (c.touched || c.dirty);
  }

  save(): void {
    if (this.imageInvalid) {
      this.error.set('Selecciona una imagen de hasta 10 MB antes de guardar.');
      this.notifications.error(this.error());
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const id = this.editingId();
    const value = { ...this.form.getRawValue(), id: id ?? '' } as Hotel;
    this.saving.set(true);
    this.error.set('');

    const request$: Observable<unknown> = id
      ? this.service.updateHotel(id, value, this.image)
      : this.service.createHotel(value, this.image);
    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.close();
        this.notifications.success(id ? 'Se actualizó correctamente el hotel.' : 'Se guardó correctamente el hotel.');
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.error.set(err.status === 400 && typeof err.error?.message === 'string'
          ? err.error.message : 'No se pudo guardar el hotel.');
        this.notifications.error(this.error());
      }
    });
  }

  remove(hotel: Hotel): void {
    if (!confirm(`¿Eliminar "${hotel.name}"?`)) return;
    this.error.set('');
    this.service.deleteHotel(hotel.id).subscribe({
      next: () => {
        this.notifications.success('Se eliminó correctamente el hotel.');
        this.load();
      },
      error: () => {
        this.error.set('No se pudo eliminar el hotel.');
        this.notifications.error(this.error());
      }
    });
  }
}
