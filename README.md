# TodoApi

API RESTful simple para autenticación de usuarios y gestión de tareas (Todo), construida en **ASP.NET Core 8 + Identity + JWT**.

---

## 🚀 Requisitos

- [.NET SDK 8.0 o superior](https://dotnet.microsoft.com/download)
- Un editor como [Visual Studio Code](https://code.visualstudio.com/) o [Visual Studio 2022+](https://visualstudio.microsoft.com/)
- (Opcional) [Postman](https://www.postman.com/) o Swagger para probar la API

---

## ⚙️ Configuración inicial

1. Clonar el repositorio:

```bash
git clone https://github.com/tuusuario/TodoApi.git
cd TodoApi
````

2. Crear archivo `appsettings.Development.json` en la raíz:

```json
{
  "JwtSettings": {
    "Secret": "tu-clave-secreta-super-larga-para-jwt"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=todoapi.db"
  }
}
```

📌 **Notas:**

* La clave JWT debe tener mínimo 32 caracteres.
* En este ejemplo se usa SQLite (pero puedes configurar SQL Server, PostgreSQL, etc.).

---

## 🏃 Ejecución del proyecto

1. Restaurar paquetes:

```bash
dotnet restore
```

2. (Si usas Entity Framework Core) Aplicar las migraciones:

```bash
dotnet ef database update
```

3. Ejecutar la aplicación:

```bash
dotnet run
```

Por defecto:

```
https://localhost:5001
http://localhost:5000
```

---

## 📚 Endpoints principales

### 🔐 Autenticación

#### ➕ Registro de usuario

**POST** `/api/auth/register`

```json
{
  "email": "usuario@example.com",
  "password": "P@ssword123",
  "userName": "usuario123"
}
```

---

#### 🔑 Login de usuario

**POST** `/api/auth/login`

```json
{
  "email": "usuario@example.com",
  "password": "P@ssword123"
}
```

**Respuesta:**

```json
{
  "token": "JWT_TOKEN_AQUI",
  "user": {
    "id": "user-id",
    "userName": "usuario123",
    "email": "usuario@example.com"
  }
}
```

---

## ✅ Notas importantes

* Para crear un nuevo comentario sin un parent, no se debe enviar este campo en el body

