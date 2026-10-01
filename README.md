# ToDo List — Blazor + .NET 10 + Postgres

Demo basica para charla: un CRUD de ToDo List con frontend Blazor, backend Minimal API y Postgres, listo para desplegar en Coolify.

## Estructura

```
.
├── docker-compose.yml   # db + api + web, con healthchecks
├── TodoApi/             # Minimal API + EF Core + Postgres
└── TodoWeb/             # Blazor Web App (Interactive Server)
```

## Correr local

```bash
cp .env.example .env
docker compose up --build
```

El frontend queda en `http://localhost:8080` (segun el puerto que mapees).

## Desplegar en Coolify

Coolify genera las URLs publicas solo. La convencion es `SERVICE_FQDN_<SERVICIO>`, con sufijo de puerto cuando el proxy debe enrutar a un puerto interno concreto. Cada servicio publico declara ademas su puerto con `expose`:

- `SERVICE_FQDN_API_8080` → URL del API
- `SERVICE_FQDN_WEB_8080` → URL del frontend

Las unicas variables que defines tu en el panel son las de la base de datos:

```
POSTGRES_DB=todos
POSTGRES_USER=postgres
POSTGRES_PASSWORD=loquesea
```

## Notas

Como Blazor Server renderiza en el servidor, el contenedor `web` llama al API por la red interna de Docker (`API_URL: http://api:8080`), no por la URL publica. Si prefieres que vaya por la publica, cambia esa linea por `API_URL: ${SERVICE_URL_API_8080}`.

Los healthchecks estan encadenados: el API no arranca hasta que Postgres responde `pg_isready`, y el web no arranca hasta que `/health` del API da 200.

El API corre `EnsureCreated()` al arrancar para crear la tabla sin migraciones — suficiente para una demo, no para produccion.

## Endpoints

| Metodo | Ruta              |
|--------|-------------------|
| GET    | `/api/todos`      |
| GET    | `/api/todos/{id}` |
| POST   | `/api/todos`      |
| PUT    | `/api/todos/{id}` |
| DELETE | `/api/todos/{id}` |
| GET    | `/health`         |
