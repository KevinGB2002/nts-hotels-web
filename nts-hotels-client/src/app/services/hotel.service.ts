import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Hotel } from '../models/hotel.model';

@Injectable({ providedIn: 'root' })
export class HotelService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl.replace(/\/+$/, '')}/api/hotels`;

  getHotels(name?: string, rating?: number): Observable<Hotel[]> {
    let params = new HttpParams();
    if (name) params = params.set('name', name);
    if (rating) params = params.set('rating', rating);
    return this.http
      .get<Hotel[]>(this.url, { params })
      .pipe(map(hotels => (Array.isArray(hotels) ? hotels : [])));
  }

  getHotelById(id: string): Observable<Hotel> {
    return this.http.get<Hotel>(`${this.url}/${id}`);
  }

  imageSource(hotel: Hotel): string {
    return hotel.hasStoredImage ? `${this.url}/${hotel.id}/image` : '';
  }

  createHotel(hotel: Hotel, image?: File): Observable<Hotel> {
    return image
      ? this.http.post<Hotel>(`${this.url}/upload`, this.imageForm(hotel, image))
      : this.http.post<Hotel>(this.url, hotel);
  }

  updateHotel(id: string, hotel: Hotel, image?: File): Observable<void> {
    return image
      ? this.http.put<void>(`${this.url}/${id}/upload`, this.imageForm(hotel, image))
      : this.http.put<void>(`${this.url}/${id}`, hotel);
  }

  deleteHotel(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  private imageForm(hotel: Hotel, image: File): FormData {
    const form = new FormData();
    form.append('name', hotel.name);
    form.append('description', hotel.description);
    form.append('address', hotel.address);
    form.append('city', hotel.city);
    form.append('rating', String(hotel.rating));
    form.append('isActive', String(hotel.isActive ?? true));
    form.append('image', image);
    return form;
  }
}
