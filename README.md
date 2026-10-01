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

Crea el recurso como **Docker Compose**. Coolify detecta los servicios que declaran `expose`
(`api` y `web`) y le da a cada uno un campo **Domains** en su UI.

1. En el panel, define las variables de la base de datos:

   ```
   POSTGRES_DB=todos
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=loquesea
   ```

2. Despliega.

3. En **Settings → Domains** de cada servicio (`api` y `web`), asigna el dominio real
   o pulsa **Generate Domain** para una URL `sslip.io` temporal.

Coolify configura Traefik y el TLS solo. No hacen falta variables `SERVICE_FQDN_` en el compose:
los dominios se asignan desde la UI.

## Notas

Como Blazor Server renderiza en el servidor, el contenedor `web` llama al API por la red interna
de Docker (`API_URL: http://api:8080`), no por la URL publica. Es un salto de red menos y no sale
a internet. Si prefieres que vaya por la publica, define `API_URL` en el panel con la URL que
Coolify le asigno al API.

Los healthchecks estan encadenados: el API no arranca hasta que Postgres responde `pg_isready`,
y el web no arranca hasta que `/health` del API da 200.

El API corre `EnsureCreated()` al arrancar para crear la tabla sin migraciones — suficiente para
una demo, no para produccion.

## Endpoints

| Metodo | Ruta              |
|--------|-------------------|
| GET    | `/api/todos`      |
| GET    | `/api/todos/{id}` |
| POST   | `/api/todos`      |
| PUT    | `/api/todos/{id}` |
| DELETE | `/api/todos/{id}` |
| GET    | `/health`         |
