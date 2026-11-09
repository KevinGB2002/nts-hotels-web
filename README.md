# NTS Hotels

Aplicación de gestión hotelera con una API REST en ASP.NET Core y un cliente web Angular. Los visitantes consultan hoteles y los administradores gestionan hoteles y usuarios.

## Arquitectura

```text
NtsHotels-Project/
├── NtsHotels.Api/           # .NET 8, Entity Framework Core, SQLite y JWT
│   ├── Controllers/         # Rutas HTTP
│   ├── DTOs/                # Contratos de entrada y salida
│   ├── Data/                # Contexto, datos iniciales e importación histórica
│   ├── Migrations/          # Migraciones de SQLite
│   ├── Models/              # Entidades
│   └── Services/            # Lógica de negocio y tratamiento de imágenes
└── nts-hotels-client/       # Angular 22, componentes standalone
    └── src/app/             # Páginas, servicios, guards e interceptor
```

Consulta las guías de [la API](NtsHotels.Api/README.md) y [el cliente](nts-hotels-client/README.md) para más detalles.

## Requisitos

- SDK de .NET 8.
- Node.js compatible con Angular 22: `^22.22.3`, `^24.15.0` o `>=26.0.0`, y npm.
- SQLite se utiliza a través del proveedor de Entity Framework Core; no requiere un servidor independiente.
- Docker es opcional; cada proyecto dispone de un Dockerfile.

## Ejecución local

En una terminal, desde la raíz:

```powershell
cd NtsHotels.Api
$env:NTSHOTELS_ADMIN_PASSWORD = '<contraseña-segura>'
dotnet restore
dotnet run --launch-profile https
```

La API escucha en `https://localhost:7102` y `http://localhost:5147`. Swagger está disponible en `https://localhost:7102/swagger` durante el desarrollo. Si es necesario, confía en el certificado local con `dotnet dev-certs https --trust`.

En otra terminal, desde la raíz:

```powershell
cd nts-hotels-client
npm ci
npm start
```

Abre `http://localhost:4200`. El cliente de desarrollo apunta a `https://localhost:7102`; ajusta los entornos de Angular y los orígenes CORS de la API si utilizas otras direcciones. La URL configurada en el cliente de producción también debe corresponder al servidor real: `localhost` se refiere al dispositivo del visitante.

La API aplica migraciones y datos iniciales al arrancar. En una base nueva crea el administrador `admin@ntshotels.com` con la contraseña suministrada mediante `NTSHOTELS_ADMIN_PASSWORD`; esta variable no es necesaria si ya existe un administrador. Configura también la clave JWT mediante variables de entorno o un gestor de secretos; no uses claves ni credenciales de desarrollo en entornos compartidos o productivos.

## Imágenes de hoteles

Los hoteles almacenan imágenes como bytes JPEG en SQLite, no como enlaces externos. El formulario acepta archivos JPEG, PNG y WebP de hasta 10 MB y 25 megapíxeles. La API utiliza **Magick.NET** para corregir la orientación, limitar el ancho a 800 píxeles y generar un JPEG con calidad 80. Al editar sin seleccionar un archivo nuevo, se conserva la imagen guardada. Si no hay imagen, el cliente muestra una imagen de respaldo.

La creación y actualización con archivos usan `multipart/form-data`; los endpoints JSON siguen disponibles para guardar datos sin adjuntar archivos. Los bytes se recuperan mediante `GET /api/hotels/{id}/image`.

Antes de actualizar una base existente, realiza un respaldo. La migración convierte los enlaces históricos a imágenes almacenadas antes de eliminar la columna anterior. Si una descarga o conversión falla, el arranque se detiene y se conservan los datos originales. La guía de la API describe las restricciones de la importación.

## Notificaciones y funcionalidades

- Catálogo público con búsqueda por nombre y filtro por estrellas.
- Inicio de sesión con JWT, contraseñas almacenadas como hashes BCrypt y rutas administrativas protegidas.
- Gestión de hoteles y usuarios desde `/admin` y `/admin/users`.
- Notificaciones tipo toast de éxito o error para operaciones de creación, actualización y eliminación.

## API principal

| Método | Ruta | Acceso |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Público |
| `GET` | `/api/hotels`, `/api/hotels/{id}`, `/api/hotels/{id}/image` | Público |
| `POST` | `/api/hotels`, `/api/hotels/upload` | Admin |
| `PUT` | `/api/hotels/{id}`, `/api/hotels/{id}/upload` | Admin |
| `DELETE` | `/api/hotels/{id}` | Admin |
| `GET`, `POST` | `/api/users` | Admin |
| `GET`, `PUT`, `DELETE` | `/api/users/{id}` | Admin |

`DELETE` desactiva hoteles y usuarios en lugar de eliminarlos físicamente. Para el contrato completo, formatos de entrada y ejemplos, consulta la guía de la API.

## Validación

```powershell
cd NtsHotels.Api
dotnet build
cd ..\nts-hotels-client
npm run build
npm test -- --watch=false
```

Los contenedores requieren configuración específica de producción, incluyendo secretos, CORS, URL pública de la API y persistencia de SQLite.
