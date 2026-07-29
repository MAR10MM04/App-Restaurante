# App-Restaurante
 # 🍔 DiDi Food - Prototipo de Aplicación de Entrega de Comida

## 📖 Descripción

DiDi Food es un prototipo de una aplicación de entrega de comida que permite a los usuarios explorar restaurantes, realizar pedidos y dar seguimiento a sus órdenes en tiempo real.

El proyecto se desarrolla bajo una arquitectura Cliente-Servidor utilizando **React** para el Frontend y **ASP.NET Core Web API** para el Backend.

---

# 🏗 Arquitectura General

```text
                     Cliente
                (Aplicación React)

                       │
                  HTTP / HTTPS
                       │
                       ▼

          ASP.NET Core Web API (.NET 8)

                       │

              Entity Framework Core

                       │

                   Base de Datos
                      MySQL
```

---

# 🛠 Tecnologías Utilizadas

## Frontend

| Tecnología        | Uso                       |
| ----------------- | ------------------------- |
| React 19          | Desarrollo de la interfaz |
| Vite              | Entorno de desarrollo     |
| React Router DOM  | Navegación                |
| Tailwind CSS      | Diseño de la interfaz     |
| Axios             | Consumo de API            |
| React Context API | Manejo del estado         |
| Lucide React      | Iconografía               |

---

## Backend

| Tecnología            | Uso                           |
| --------------------- | ----------------------------- |
| ASP.NET Core 8        | API REST                      |
| Entity Framework Core | ORM                           |
| MySQL                 | Base de datos                 |
| JWT                   | Autenticación                 |
| Swagger               | Documentación de la API       |
| AutoMapper            | Conversión de DTOs (Opcional) |

---

## Herramientas de Desarrollo

* Visual Studio 2022
* Visual Studio Code
* Git
* GitHub
* Postman
* MySQL Workbench
* Figma / Stitch

---

# 📂 Estructura del Proyecto

```text
DidiFood/

│
├── frontend/
│
│   ├── public/
│   ├── src/
│   │
│   ├── assets/
│   ├── components/
│   ├── context/
│   ├── hooks/
│   ├── layouts/
│   ├── pages/
│   ├── routes/
│   ├── services/
│   ├── utils/
│   ├── App.jsx
│   └── main.jsx
│
│
├── backend/
│
│   ├── Controllers/
│   ├── DTOs/
│   ├── Models/
│   ├── Data/
│   ├── Repositories/
│   ├── Services/
│   ├── Interfaces/
│   ├── Middleware/
│   ├── Helpers/
│   ├── Migrations/
│   ├── Program.cs
│   └── appsettings.json
│
│
└── README.md
```

---

# 🎨 Estructura del Frontend

```text
src/

components/
│
├── Navbar
├── Footer
├── RestaurantCard
├── ProductCard
├── SearchBar
├── CategoryCard
├── OrderStatus
├── CartItem
└── Buttons

pages/

Splash

Login

Register

Home

Search

Restaurant

Product

Cart

Checkout

Tracking

History

Profile
```

---

# ⚙ Estructura del Backend

```text
Controllers

AuthController

RestaurantController

ProductController

OrderController

UserController

CategoryController

DeliveryController

PaymentController
```

---

# 🗃 Base de Datos

```text
Usuarios

Restaurantes

Categorias

Productos

Pedidos

DetallePedido

Carrito

Repartidores

Direcciones

Pagos
```

---

# 🔄 Flujo de Funcionamiento del Sistema

```text
Usuario

        │

        ▼

Iniciar Sesión

        │

        ▼

Página Principal

        │

        ▼

Buscar Restaurante

        │

        ▼

Seleccionar Restaurante

        │

        ▼

Seleccionar Producto

        │

        ▼

Agregar al Carrito

        │

        ▼

Confirmar Pedido

        │

        ▼

Procesar Pedido

        │

        ▼

Seguimiento

        │

        ▼

Pedido Entregado

        │

        ▼

Calificación
```

---

# 🔐 Flujo del Backend

```text
Frontend

      │

Axios

      │

Controllers

      │

Services

      │

Repositories

      │

Entity Framework

      │

MySQL
```

---

# 🌿 Flujo de Trabajo con Git y GitHub

El proyecto utiliza una estrategia basada en **Git Flow** para organizar el desarrollo y mantener un historial limpio de cambios.

## Estructura de ramas

```text
main
│
├── develop
│
├── feature/login
├── feature/register
├── feature/home
├── feature/cart
├── feature/orders
├── feature/profile
│
├── release/v1.0
│
└── hotfix/login-error
```

### Descripción de las ramas

| Rama        | Propósito                                                      |
| ----------- | -------------------------------------------------------------- |
| `main`      | Código estable en producción.                                  |
| `develop`   | Integración de todas las funcionalidades antes de una versión. |
| `feature/*` | Desarrollo de nuevas funcionalidades.                          |
| `release/*` | Preparación de una nueva versión para pruebas finales.         |
| `hotfix/*`  | Corrección de errores críticos detectados en producción.       |

---

# 🔄 Flujo de Trabajo con GitHub

```text
Nueva tarea

      │

Crear rama

feature/nombre-funcionalidad

      │

Desarrollo

      │

Commits

      │

Push

      │

Pull Request

      │

Code Review

      │

Merge a develop

      │

Pruebas QA

      │

release/v1.0

      │

Merge a main

      │

Producción
```

---

# 📋 Convención de Commits

```bash
feat: Agregar módulo de pedidos

fix: Corregir error de autenticación

docs: Actualizar README

style: Mejorar diseño del Home

refactor: Optimizar controlador

test: Agregar pruebas unitarias

chore: Actualizar dependencias
```

---

# 🚀 Instalación

## Backend

```bash
cd backend

dotnet restore

dotnet ef database update

dotnet run
```

---

## Frontend

```bash
cd frontend

npm install

npm run dev
```

---

# 👥 Roles del Proyecto

* **Administrador:** Gestiona usuarios, restaurantes y categorías.
* **Cliente:** Explora restaurantes, realiza pedidos y consulta su historial.
* **Restaurante:** Administra su menú y gestiona pedidos.
* **Repartidor (Prototipo):** Consulta y actualiza el estado de las entregas.

---

# 📌 Objetivo

Desarrollar un prototipo funcional de una plataforma de entrega de comida que demuestre la integración entre un Frontend moderno, un Backend basado en APIs REST y una base de datos relacional, siguiendo buenas prácticas de desarrollo de software y control de versiones mediante Git y GitHub.
