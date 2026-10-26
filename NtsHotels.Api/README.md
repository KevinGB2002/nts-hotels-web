# NtsHotels.Api

API REST para la gestión de hoteles y usuarios de la cadena NTS Hotels. Está construida con ASP.NET Core sobre .NET 8 y utiliza Entity Framework Core para el acceso a datos, con SQLite como base de datos local.

## Características principales

- **SQLite local:** usa `nts_hotels.db` por defecto. Al iniciar, la aplicación aplica las migraciones pendientes y ejecuta el seed de datos.
- **Datos de prueba:** en una base nueva se crean nueve hoteles de distintas ciudades de Colombia y un usuario administrador.
- **Autenticación JWT:** el endpoint de login emite tokens Bearer firmados con la configuración JWT de la aplicación.
- **Contraseñas protegidas:** las contraseñas se almacenan como hashes BCrypt, no como texto plano ni como cifrado reversible.
- **Autorización por roles:** la creación, edición y eliminación de hoteles, y todos los endpoints de usuarios, requieren el rol `Admin`.
- **Swagger:** la interfaz OpenAPI se habilita en el entorno `Development`.

Credenciales iniciales del administrador en una base nueva:

| Usuario | Contraseña |
| --- | --- |
| `admin@ntshotels.com` | `Admin123*` |

Estas credenciales son solo para desarrollo y pruebas. Cámbialas antes de exponer la API en cualquier entorno compartido o productivo. El seed no reemplaza un usuario administrador ya existente; los hoteles tampoco se vuelven a insertar si la tabla ya contiene registros.

## Arquitectura y estructura

```text
NtsHotels.Api/
├── Controllers/   # Endpoints HTTP para autenticación, hoteles y usuarios
├── Data/          # ApplicationDbContext y DbInitializer
├── DTOs/          # Contratos de entrada y respuesta de la API
├── Middleware/    # Manejo global de excepciones
├── Migrations/    # Migraciones de Entity Framework Core para SQLite
├── Models/        # Entidades Hotel, User y Role
├── Services/      # Lógica de hoteles, usuarios y emisión de tokens
├── Settings/      # Configuración tipada, incluida JwtSettings
├── Program.cs     # Registro de servicios y configuración del pipeline HTTP
├── appsettings.json
└── NtsHotels.Api.csproj
```

Los controladores validan y reciben solicitudes HTTP, y delegan las operaciones de negocio en los servicios. `ApplicationDbContext` configura las entidades y sus relaciones. En cada arranque, `Program.cs` aplica las migraciones y llama a `DbInitializer` para inicializar los datos cuando corresponde.

## Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git, si se va a clonar el repositorio

Comprueba el SDK instalado con:

```bash
dotnet --list-sdks
```

Debe aparecer una versión `8.0.x`.

## Configuración y ejecución

1. Clona el repositorio usando la URL proporcionada para el proyecto y entra en el directorio de la API:

   ```bash
   git clone <URL-del-repositorio> NtsHotels.Api
   cd NtsHotels.Api
   ```

   Si el repositorio contiene la API en una subcarpeta, cambia al directorio que contiene `NtsHotels.Api.csproj`.

2. Revisa `appsettings.json`. La configuración local debe incluir una cadena SQLite, los parámetros JWT y el origen permitido para el cliente Angular. Por ejemplo:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=nts_hotels.db"
     },
     "Cors": {
       "AllowedOrigins": [ "http://localhost:4200" ]
     },
     "Jwt": {
       "Issuer": "NtsHotels.Api",
       "Audience": "NtsHotels.Client",
       "Key": "REEMPLAZAR_POR_UN_SECRETO_ALEATORIO_DE_AL_MENOS_32_CARACTERES",
       "ExpirationMinutes": 60
     }
   }
   ```

   `Jwt:Key` debe tener al menos 32 caracteres. Usa un secreto aleatorio propio y no publiques secretos reales en el repositorio. En entornos compartidos, configura secretos mediante variables de entorno o un gestor de secretos. El origen CORS debe coincidir con la URL desde la que se sirve el cliente.

3. Restaura dependencias y arranca la API desde el directorio del proyecto:

   ```bash
   dotnet restore
   dotnet run --launch-profile http
   ```

   También puedes usar `dotnet run` para iniciar con el perfil predeterminado. La base `nts_hotels.db` se crea en el directorio de ejecución si todavía no existe; las migraciones y el seed se ejecutan automáticamente al arrancar.

El perfil HTTP está configurado en `http://localhost:5147`. El perfil HTTPS está disponible con `dotnet run --launch-profile https` y usa `https://localhost:7102` además del puerto HTTP configurado.

## Documentación de endpoints

En `Development`, abre Swagger UI en:

```text
http://localhost:5147/swagger
```

Usa **Try it out** para enviar solicitudes. Para probar rutas protegidas, primero ejecuta `POST /api/auth/login` con las credenciales de desarrollo. Copia el valor `token` de la respuesta, pulsa **Authorize** en Swagger y pega solo el token JWT. Después podrás llamar a las operaciones de administrador.

### Autenticación

| Método | Ruta | Acceso | Descripción |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Público | Valida las credenciales y devuelve un token JWT y los datos del usuario. |

Ejemplo de solicitud:

```json
{
  "email": "admin@ntshotels.com",
  "password": "Admin123*"
}
```

### Hoteles

| Método | Ruta | Acceso | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/hotels` | Público | Lista hoteles activos. Acepta los filtros opcionales `name` y `rating`. |
| `GET` | `/api/hotels/{id}` | Público | Obtiene un hotel por su GUID. |
| `POST` | `/api/hotels` | `Admin` | Crea un hotel. |
| `PUT` | `/api/hotels/{id}` | `Admin` | Actualiza un hotel existente. |
| `DELETE` | `/api/hotels/{id}` | `Admin` | Elimina un hotel. |

Los campos principales para crear o actualizar un hotel son `name`, `description`, `address`, `city`, `rating` e `imageUrl`. La calificación debe estar entre 1 y 5. La actualización también acepta `isActive`.

Ejemplo de creación:

```json
{
  "name": "Hotel Ejemplo",
  "description": "Hotel de prueba",
  "address": "Calle 10 # 20-30",
  "city": "Bogotá",
  "rating": 4,
  "imageUrl": "https://example.com/hotel.jpg"
}
```

Para filtrar la lista, puedes usar una solicitud como `GET /api/hotels?name=Hotel&rating=4`.

### Usuarios

Todas las rutas de usuarios requieren un token JWT con rol `Admin`.

| Método | Ruta | Acceso | Descripción |
| --- | --- | --- | --- |
| `GET` | `/api/users` | `Admin` | Lista usuarios. |
| `GET` | `/api/users/{id}` | `Admin` | Obtiene un usuario por su GUID. |
| `POST` | `/api/users` | `Admin` | Crea un usuario. |
| `PUT` | `/api/users/{id}` | `Admin` | Actualiza nombre, correo, rol y estado. |
| `DELETE` | `/api/users/{id}` | `Admin` | Elimina un usuario. |

Para crear un usuario se envían `fullName`, `email`, `password` y `roleId`. La contraseña debe tener al menos seis caracteres e incluir una mayúscula, un número y un carácter especial. Los roles iniciales son `Admin` (`roleId: 1`) y `User` (`roleId: 2`).

## Respuestas HTTP habituales

- `200 OK`: consulta o autenticación correcta.
- `201 Created`: recurso creado.
- `204 No Content`: actualización o eliminación correcta.
- `400 Bad Request`: solicitud inválida o datos que no pasan validación.
- `401 Unauthorized`: token ausente o credenciales inválidas.
- `403 Forbidden`: token válido sin el rol requerido.
- `404 Not Found`: recurso inexistente.
- `409 Conflict`: conflicto al crear o actualizar un usuario, por ejemplo, correo duplicado.
