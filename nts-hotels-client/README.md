# NTS Hotels Client

Interfaz web de la cadena **NTS Hotels**, construida con Angular y componentes standalone. Permite a cualquier visitante explorar el catálogo de hoteles y ofrece a los administradores un panel privado para gestionarlos.

> El proyecto usa Angular 22 (`@angular/*` ^22.2). Todo el código es compatible con el modelo standalone introducido en Angular 17.

## Características

- **Landing page pública**
  - Hero de presentación y navbar con acceso para administradores.
  - Buscador por nombre en tiempo real con *debounce* (300 ms).
  - Filtro por calificación (1 a 5 estrellas).
  - Grid responsivo de tarjetas con imagen (con imagen de respaldo si no está disponible), nombre, ciudad, dirección, descripción y estrellas.
- **Autenticación y roles**
  - Login con formulario reactivo y validaciones.
  - El JWT, su expiración y el usuario se guardan en `localStorage`.
  - El rol se obtiene del token (o del usuario) y solo los administradores acceden al panel; otros usuarios reciben "No eres un administrador".
- **Panel de administración (`/admin`)**
  - Tabla CRUD de hoteles con modal reactivo para crear y editar (campos requeridos y calificación de 1 a 5).
  - Eliminación con confirmación.
  - Gestión de usuarios en `/admin/users` (crear, editar, activar/desactivar, eliminar) con contraseña segura (mínimo 6 caracteres, una mayúscula y un número).
- **Seguridad**
  - Interceptor HTTP que añade `Authorization: Bearer <token>` a las peticiones hacia la API y cierra la sesión ante un 401.
  - Guards que protegen las rutas privadas.
- **UX**: spinners de carga, notificaciones al crear, editar o eliminar hoteles y usuarios, mensajes de error y estados de lista vacía.
- **Imágenes de hoteles**: se puede seleccionar un archivo JPEG, PNG o WebP de hasta 10 MB. La API lo convierte a JPEG de hasta 800 px de ancho y lo guarda en SQLite.

## Estructura del proyecto

```
src/
├── environments/              # Configuración por entorno (apiUrl)
└── app/
    ├── guards/                # authGuard y adminGuard
    ├── interceptors/          # authInterceptor (JWT)
    ├── models/                # Hotel, User, LoginRequest/LoginResponse
    ├── pages/
    │   ├── home/              # Landing pública
    │   ├── login/             # Inicio de sesión
    │   ├── admin/             # CRUD de hoteles
    │   └── user-management/   # CRUD de usuarios
    ├── services/              # Autenticación, hoteles, usuarios y notificaciones
    ├── app.config.ts          # Providers (router, HttpClient + interceptor)
    └── app.routes.ts          # Definición de rutas
```

### Rutas

| Ruta | Componente | Acceso |
| --- | --- | --- |
| `/` | `HomeComponent` | Público |
| `/login` | `LoginComponent` | Público |
| `/admin` | `AdminComponent` | Administrador |
| `/admin/users` | `UserManagementComponent` | Administrador |
| `**` | Redirige a `/` | — |

## Prerrequisitos

- [Node.js](https://nodejs.org/) compatible con Angular 22: `^22.22.3`, `^24.15.0` o `>=26.0.0`, y npm.
- [Angular CLI](https://angular.dev/tools/cli): `npm install -g @angular/cli`
- La API de NTS Hotels (.NET) en ejecución.

## Instalación y ejecución

1. Instalar dependencias:

   ```bash
   npm install
   ```

2. Configurar la URL de la API en `src/environments/environment.development.ts`:

   ```ts
   export const environment = {
     production: false,
     apiUrl: 'https://localhost:7102'
   };
   ```

   Esta URL corresponde al perfil `https` de la API. Iníciala con `dotnet run --launch-profile https` y, si el navegador no confía en el certificado de desarrollo, ejecuta `dotnet dev-certs https --trust`. El entorno de producción se configura en `src/environments/environment.ts`.

3. Levantar el servidor de desarrollo:

   ```bash
   ng serve
   ```

4. Abrir `http://localhost:4200/`. La aplicación se recarga automáticamente al modificar el código.

### Endpoints consumidos

- `POST /api/auth/login`
- `GET /api/hotels` (filtros opcionales `name` y `rating`), `GET /api/hotels/{id}`, `POST /api/hotels`, `PUT /api/hotels/{id}`, `DELETE /api/hotels/{id}`
- `POST /api/hotels/upload` y `PUT /api/hotels/{id}/upload` (multipart con archivo `image`); `GET /api/hotels/{id}/image` para servir la imagen guardada.
- `GET|POST /api/users`, `GET|PUT|DELETE /api/users/{id}`

La API debe permitir CORS desde `http://localhost:4200`.

Al editar un hotel, dejar el archivo sin seleccionar conserva la imagen guardada. Al actualizar la API, los enlaces históricos se convierten antes de eliminar la columna; consulta las instrucciones de migración de la API.

## Scripts

| Comando | Descripción |
| --- | --- |
| `npm start` / `ng serve` | Servidor de desarrollo |
| `npm run build` | Build de producción en `dist/` |
| `npm test` | Pruebas unitarias (Vitest) |
