export interface User {
  id: string;
  fullName: string;
  email: string;
  password?: string;
  roleId: number;
  roleName: string;
  isActive: boolean;
  createdAt: string;
}
