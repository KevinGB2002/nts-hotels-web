import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Hotel } from '../models/hotel.model';
import { HotelService } from './hotel.service';

describe('HotelService image requests', () => {
  const hotel: Hotel = {
    id: '1', name: 'Hotel', description: 'Descripción', address: 'Dirección',
    city: 'Ciudad', rating: 4, isActive: true
  };
  beforeEach(() => TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting()]
  }));
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('uses multipart for creation and replacement, and JSON to preserve an image', () => {
    const service = TestBed.inject(HotelService);
    const http = TestBed.inject(HttpTestingController);
    const image = new File(['image'], 'hotel.png', { type: 'image/png' });
    service.createHotel(hotel, image).subscribe();
    const create = http.expectOne(req => req.url.endsWith('/api/hotels/upload'));
    expect(create.request.body).toBeInstanceOf(FormData);
    expect(create.request.body.get('image')).toBe(image);
    create.flush(hotel);
    service.updateHotel(hotel.id, hotel, image).subscribe();
    const replace = http.expectOne(req => req.url.endsWith('/api/hotels/1/upload'));
    expect(replace.request.body.get('image')).toBe(image);
    replace.flush(null);
    service.updateHotel(hotel.id, hotel).subscribe();
    const keep = http.expectOne(req => req.url.endsWith('/api/hotels/1'));
    expect(keep.request.body).toBe(hotel);
    keep.flush(null);
  });

  it('resolves stored images from the API and falls back when absent', () => {
    const service = TestBed.inject(HotelService);
    expect(service.imageSource({ ...hotel, hasStoredImage: true })).toContain('/api/hotels/1/image');
    expect(service.imageSource(hotel)).toBe('');
  });
});
