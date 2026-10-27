# NTS Hotels - Sistema de Gestion Hotelera (Fullstack)

Este repositorio contiene la solucion Fullstack completa para la cadena NTS Hotels, compuesta por una API REST en .NET 8 con SQLite y un cliente web interactivo desarrollado en Angular 17+ (con componentes standalone).

El proyecto permite a cualquier visitante consultar el catalogo general de hoteles con filtros dinamicos, mientras que ofrece un portal seguro de gestion para usuarios administradores.

\---

## Arquitectura General del Monorepo

```text
NtsHotels-Project/
├── NtsHotels.Api/          # Backend: API REST en .NET 8, EF Core, SQLite y JWT
│   └── Dockerfile          # Contenedor multi-etapa .NET 8 SDK / ASP.NET
└── nts-hotels-client/      # Frontend: SPA en Angular 17+ con Standalone Components
    ├── Dockerfile          # Contenedor multi-etapa Node.js / Nginx
    └── nginx.conf          # Configuracion de Nginx para enrutamiento SPA
```

\---

## Inicio Rapido en Local

### Prerrequisitos

* .NET 8 SDK
* Node.js (version LTS) y npm
* Angular CLI (`npm install -g @angular/cli`)
* Docker Desktop (Opcional, para ejecucion mediante contenedores)

\---

### 1\. Configuracion y Ejecucion del Backend (NtsHotels.Api)

1. Entra a la carpeta del backend:

```bash
   cd NtsHotels.Api
   ```

2. Revisa el archivo `appsettings.json` para verificar la cadena de conexion a SQLite, la clave JWT y los origenes permitidos para CORS:

```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=nts\_hotels.db"
     },
     "Cors": {
       "AllowedOrigins": \[ "http://localhost:4200" ]
     },
     "Jwt": {
       "Issuer": "NtsHotels.Api",
       "Audience": "NtsHotels.Client",
       "Key": "NtsHotels\_SecureSecretKey\_2026\_x9#mK9$vL2pQ8wZ1tY7bF4dX6sC3hJ",
       "ExpirationMinutes": 60
     }
   }
   ```

3. Inicia la aplicacion (la base de datos `nts\_hotels.db` se creara y ejecutara el seed de datos automaticamente):

```bash
   dotnet run --launch-profile http
   ```

   * API: `http://localhost:5147`
   * Swagger UI: `http://localhost:5147/swagger`

\---

### 2\. Configuracion y Ejecucion del Frontend (nts-hotels-client)

1. Abre otra terminal y entra a la carpeta del frontend:

```bash
   cd nts-hotels-client
   ```

2. Instala las dependencias:

```bash
   npm install
   ```

3. Asegurate de que la URL de la API apunte al backend en `src/environments/environment.development.ts`:

```typescript
   export const environment = {
     production: false,
     apiUrl: 'http://localhost:5147'
   };
   ```

4. Levanta el servidor de desarrollo:

```bash
   ng serve
   ```

   * Aplicacion Web: `http://localhost:4200/`

\---

## Despliegue con Docker y Servidor de Produccion

El proyecto incluye configuraciones de Dockerfile optimizadas mediante construcciones multi-etapa (multi-stage builds) para empaquetar y desplegar tanto la API como el cliente en cualquier entorno o servicio en la nube (como Render, Fly.io, AWS o GCP).

### Backend (.NET API) - `NtsHotels.Api/Dockerfile`

Compila el proyecto utilizando el SDK de .NET 8 y genera una imagen ligera optimizada sobre el runtime de ASP.NET Core, exponiendo la aplicacion en el puerto `8080`.

Comando para compilar y ejecutar manualmente:

```bash
cd NtsHotels.Api
docker build -t nts-hotels-api .
docker run -d -p 8080:8080 --name api-container nts-hotels-api
```

### Frontend (Angular Client) - `nts-hotels-client/Dockerfile`

Usa una primera etapa con `node:20-alpine` para compilar la aplicacion en modo produccion (`ng build`), y una segunda etapa basada en `nginx:alpine` para servir los activos estaticos en el puerto `80`. Incluye un archivo `nginx.conf` adaptado para redirigir las peticiones al `index.html` sin romper el enrutamiento interno de Angular.

Comando para compilar y ejecutar manualmente:

```bash
cd nts-hotels-client
docker build -t nts-hotels-client .
docker run -d -p 80:80 --name client-container nts-hotels-client
```

\---

## Credenciales de Prueba (Administrador)

Al ejecutar el proyecto por primera vez, la inicializacion automatica de la base de datos (DbInitializer) crea la siguiente cuenta:

|Usuario|Contraseña|Rol|
|-|-|-|
|`admin@ntshotels.com`|`Admin123\*`|`Admin`|

\---

## Resumen de Componentes y Funcionalidades

### Backend (NtsHotels.Api)

* Persistencia local mediante SQLite (`nts\_hotels.db`) con Entity Framework Core.
* Autenticacion mediante JSON Web Tokens (JWT) y cifrado de contraseñas con BCrypt.
* Inicialización automatica de datos (Seed) con 9 hoteles de prueba en Colombia y usuario administrador.
* Proteccion de rutas por rol (`Admin`) para creacion, edicion y eliminacion de hoteles y gestion de usuarios.
* Documentación interactiva con Swagger en entorno de desarrollo.

### Frontend (nts-hotels-client)

* Landing page publica con listado de hoteles en grid responsivo.
* Buscador en tiempo real con debounce (300 ms) y filtro por calificacion (estrellas).
* Panel de administracion protegido (`/admin`) con formularios reactivos para operacion CRUD de hoteles y usuarios.
* Interceptor HTTP para inyeccion automatica de encabezado `Authorization: Bearer <token>` y manejo de expiracion de sesion (401).
* Guards de navegacion (`adminGuard`) para restringir el acceso a usuarios no autorizados.

\---

## Licencia y Uso

Proyecto desarrollado como prueba tecnica de evaluacion para Cheil Connec+.

