# Evaluación Técnica 

## 📦 Requisitos

- .NET SDK 8
- Docker (para levantar SQL Server)
- Visual Studio
- Postman o Swagger para probar la API

---

## 🐳 Configurar SQL Server con Docker

Ejecuta el siguiente comando en tu terminal para levantar una instancia de SQL Server en Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
```

Conéctate a:

- **Servidor**: `localhost,1433`
- **Usuario**: `sa`
- **Contraseña**: `YourStrong!Passw0rd`

---

## 🔧 Configurar la base de datos

1. Clona este repositorio
2. Asegúrate de que la cadena de conexión en `appsettings.json` apunte a tu contenedor:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=NuxibaDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
}
```

3. Ejecuta las migraciones para crear las tablas:

```bash
dotnet ef database update
```

---

## 🚀 Ejecutar la API

Desde Visual Studio o la terminal:

```bash
dotnet run
```

Esto abrirá **Swagger UI** en tu navegador en:

```
https://localhost:xxxx/swagger
```

---

## 📚 Endpoints implementados

### `GET /api/login`
Devuelve todos los registros de logins/logouts.

### `POST /api/logins`
Registra un nuevo login/logout.  
Incluye validaciones:
- El `User_id` debe existir
- No permite logins múltiples sin logout previo
- No permite logout sin login anterior

### `PUT /api/logins/{id}`
Actualiza un registro existente, validando la secuencia lógica login-logout.

### `DELETE /api/logins/{id}`
Elimina un registro específico por ID.

### `GET /api/logins/report`
Genera un archivo CSV descargable con:
- Login del usuario
- Nombre completo
- Área

---

## 📊 Consultas SQL desarrolladas

### 🔹 Consulta 1 – Usuario que más tiempo ha estado logueado

```sql
WITH ParesLoginLogout AS (
    SELECT
        l1.User_id,
        l1.Fecha AS LoginFecha,
        MIN(l2.Fecha) AS LogoutFecha
    FROM ccloglogin l1
    JOIN ccloglogin l2
        ON l1.User_id = l2.User_id
        AND l1.TipoMov = 1
        AND l2.TipoMov = 0
        AND l2.Fecha > l1.Fecha
    GROUP BY l1.User_id, l1.Fecha
),
TiemposPorUsuario AS (
    SELECT
        User_id,
        SUM(DATEDIFF(SECOND, LoginFecha, LogoutFecha)) AS TotalSegundos
    FROM ParesLoginLogout
    GROUP BY User_id
),
FormatoTiempo AS (
    SELECT
        User_id,
        TotalSegundos,
        TotalSegundos / 86400 AS Dias,
        (TotalSegundos % 86400) / 3600 AS Horas,
        (TotalSegundos % 3600) / 60 AS Minutos,
        (TotalSegundos % 60) AS Segundos
    FROM TiemposPorUsuario
)
SELECT TOP 1
    User_id,
    CONCAT(
        Dias, ' días, ',
        Horas, ' horas, ',
        Minutos, ' minutos, ',
        Segundos, ' segundos'
    ) AS TiempoTotal
FROM FormatoTiempo
ORDER BY TotalSegundos DESC;
```

Devuelve el `User_id` con mayor tiempo acumulado (en días, horas, minutos, segundos).

### 🔹 Consulta 2 – Usuario que menos tiempo ha estado logueado

```sql
WITH ParesLoginLogout AS (
    SELECT
        l1.User_id,
        l1.Fecha AS LoginFecha,
        MIN(l2.Fecha) AS LogoutFecha
    FROM ccloglogin l1
    JOIN ccloglogin l2
        ON l1.User_id = l2.User_id
        AND l1.TipoMov = 1
        AND l2.TipoMov = 0
        AND l2.Fecha > l1.Fecha
    GROUP BY l1.User_id, l1.Fecha
),
TiemposPorUsuario AS (
    SELECT
        User_id,
        SUM(DATEDIFF(SECOND, LoginFecha, LogoutFecha)) AS TotalSegundos
    FROM ParesLoginLogout
    GROUP BY User_id
),
FormatoTiempo AS (
    SELECT
        User_id,
        TotalSegundos,
        TotalSegundos / 86400 AS Dias,
        (TotalSegundos % 86400) / 3600 AS Horas,
        (TotalSegundos % 3600) / 60 AS Minutos,
        (TotalSegundos % 60) AS Segundos
    FROM TiemposPorUsuario
)
SELECT TOP 1
    User_id,
    CONCAT(
        Dias, ' días, ',
        Horas, ' horas, ',
        Minutos, ' minutos, ',
        Segundos, ' segundos'
    ) AS TiempoTotal
FROM FormatoTiempo
ORDER BY TotalSegundos ASC;
```

Misma lógica que la anterior, pero en orden ascendente.


---


