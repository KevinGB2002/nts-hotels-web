import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, catchError, debounceTime, of, switchMap, tap } from 'rxjs';
import { Hotel } from '../../models/hotel.model';
import { AuthService } from '../../services/auth.service';
import { HotelService } from '../../services/hotel.service';

const FALLBACK_IMAGE =
  'data:image/svg+xml;utf8,' +
  encodeURIComponent(
    '<svg xmlns="http://www.w3.org/2000/svg" width="400" height="250"><rect width="100%" height="100%" fill="#dde3ea"/><text x="50%" y="50%" fill="#7a8794" font-family="sans-serif" font-size="20" text-anchor="middle">Sin imagen</text></svg>'
  );

@Component({
  selector: 'app-home',
  imports: [FormsModule, RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  readonly service = inject(HotelService);
  readonly auth = inject(AuthService);
  private readonly search$ = new Subject<void>();

  readonly hotels = signal<Hotel[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');
  readonly ratings = [1, 2, 3, 4, 5];
  readonly fallbackImage = FALLBACK_IMAGE;

  name = '';
  rating?: number;

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(300),
        tap(() => {
          this.loading.set(true);
          this.error.set('');
        }),
        switchMap(() =>
          this.service.getHotels(this.name.trim() || undefined, this.rating).pipe(
            catchError(() => {
              this.error.set('No se pudo cargar la lista de hoteles.');
              return of<Hotel[]>([]);
            })
          )
        )
      )
      .subscribe(hotels => {
        this.hotels.set(hotels);
        this.loading.set(false);
      });
    this.search$.next();
  }

  onFilterChange(): void {
    this.search$.next();
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.onerror = null;
    img.src = FALLBACK_IMAGE;
  }

  stars(rating: number): string {
    const r = Math.max(0, Math.min(5, Math.round(rating)));
    return '★'.repeat(r) + '☆'.repeat(5 - r);
  }
}
