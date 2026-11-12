export interface Hotel {
  id: string;
  name: string;
  description: string;
  address: string;
  city: string;
  rating: number;
  hasStoredImage?: boolean;
  isActive?: boolean;
}
