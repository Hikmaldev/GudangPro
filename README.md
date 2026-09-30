# 📦 GudangPro — Warehouse Inventory Management System

![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Angular 20](https://img.shields.io/badge/Angular-20-DD0031?logo=angular&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-Supabase-3ECF8E?logo=supabase&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-5.9-3178C6?logo=typescript&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)
![Vercel](https://img.shields.io/badge/Frontend-Vercel-000?logo=vercel)
![Render](https://img.shields.io/badge/Backend-Render-46E3B7?logo=render&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green)

**GudangPro** is a full-stack Warehouse Management System (WMS) built for SME manufacturers to replace error-prone Excel-based inventory tracking. Powered by **ASP.NET Core 10 Web API**, **Angular 20 SPA**, and **Supabase PostgreSQL** — deployed for free on **Vercel + Render**.

> *"Track stock, record transactions, and get alerts before items run out."*

---

## ✨ Key Features

### 📊 Executive Dashboard
- **KPI summary cards**: Total items, today's transactions, low-stock alerts count
- **Category distribution chart** and **7-day inbound/outbound trend graph**
- **Critical stock list** sorted by urgency (critical → low → healthy)
- Filter dashboard view per warehouse

### 📋 Master Data Management
- Full CRUD for **Items**, **Categories**, **Units**, and **Warehouses**
- Unique item codes with server-side validation
- Soft-delete protection — items with transaction history are deactivated, never deleted

### 📥 Inbound & 📤 Outbound Transactions
- Record goods-in (supplier receipts) and goods-out (production requests)
- **Real-time stock validation** — rejects negative stock and zero quantities
- **Optimistic Concurrency Control** (`RowVersion` token) prevents race conditions
- Admin can cancel transactions via reversal entries (never physical deletion)

### 🔔 Smart Alert System
- Automatic notifications when stock falls to or below minimum threshold
- Two-tier severity: `critical` (urgent) and `low` (approaching minimum)
- Per-warehouse, per-item granularity

### 📇 Stock Card & Transaction History
- Chronological mutation log per item per warehouse (opening → in → out → closing balance)
- Filter by date range, transaction type, item, and user
- CSV export for reporting

### 🔐 Security & Access Control
- **Role-Based Access Control (RBAC)**: Admin Gudang, Staf Gudang, Pemilik
- Staff members only see warehouses they're assigned to
- JWT authentication with refresh tokens
- BCrypt password hashing
- Auto-lockout after 5 consecutive failed login attempts (15-minute cooldown)

### 📝 Immutable Audit Trail
- Every data change is logged: who, when, what changed, old/new values, IP address
- Audit logs cannot be modified or deleted by any user

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | [ASP.NET Core 10](https://dotnet.microsoft.com/) Web API (C# 13) |
| **Frontend** | [Angular 20](https://angular.dev/) (Standalone Components, Signals, Reactive Forms) |
| **ORM** | [Entity Framework Core](https://learn.microsoft.com/ef/core/) (Dual-engine: SQL Server + PostgreSQL) |
| **Database** | [Supabase](https://supabase.com/) PostgreSQL (cloud) / SQL Server Express (local) |
| **Auth** | JWT Bearer + Refresh Token, BCrypt |
| **Logging** | [Serilog](https://serilog.net/) (structured request logging) |
| **API Docs** | [Swagger / OpenAPI](https://swagger.io/) (interactive) |
| **Testing** | [xUnit](https://xunit.net/) (backend business logic & concurrency tests) |
| **Container** | Docker multi-stage build, Docker Compose |
| **Frontend Hosting** | [Vercel](https://vercel.com/) (Edge Network, reverse proxy to API) |
| **Backend Hosting** | [Render](https://render.com/) (Docker container, free tier) |

---

## 🗂️ Project Structure

```
GudangPro/
├── backend/                          # .NET 10 Solution (Clean Architecture)
│   ├── src/
│   │   ├── GudangPro.Domain/            # Entities, Enums, Business Rules
│   │   ├── GudangPro.Infrastructure/    # EF Core DbContext, Seed Data
│   │   ├── GudangPro.Application/       # Application Services & DTOs
│   │   └── GudangPro.Api/              # REST Controllers, JWT, Swagger, Serilog
│   ├── tests/
│   │   └── GudangPro.Tests/            # xUnit Unit & Integration Tests
│   └── Dockerfile
│
├── frontend/                         # Angular 20 SPA
│   ├── src/
│   │   └── app/
│   │       ├── core/                    # Services, Interceptors, Guards, Models
│   │       ├── features/                # Pages: Dashboard, Master, Transactions,
│   │       │                            #   Reports, Users, Alerts, Audit Log
│   │       └── layout/                  # App shell & sidebar navigation
│   ├── vercel.json                      # Vercel rewrites (API proxy to Render)
│   └── public/                          # Favicon & static assets
│
├── supabase-init.sql                 # PostgreSQL DDL + seed data for Supabase
├── docker-compose.yml                # Multi-container setup (SQL Server + API + SPA)
├── Dockerfile                        # Root Docker build for Render deployment
├── DEPLOY-GUIDE.md                   # Step-by-step cloud deployment guide
├── SETUP-GUIDE.md                    # Local development setup guide
└── package.json                      # Root runner scripts
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) & npm
- Microsoft SQL Server Express *(for local development)*

### 1. Clone & Install

```bash
git clone https://github.com/Hikmaldev/GudangPro.git
cd GudangPro
npm install --prefix frontend
```

### 2. Run Backend Web API

```bash
npm run backend
```

- API runs at: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/` (interactive API documentation)
- Database tables and seed data are created automatically on first startup

### 3. Run Frontend Angular

```bash
npm run dev
```

- Frontend runs at: `http://localhost:4200`
- Automatic reverse proxy to backend API at `/api/*`

### 4. Run Backend Tests

```bash
npm run backend:test
```

---

## 🐳 Docker Compose (Alternative)

If you have Docker Desktop installed:

```bash
# Start database, backend, and frontend simultaneously
npm run docker:up

# Stop all containers
npm run docker:down
```

---

## 🌐 Cloud Deployment (Free Tier)

GudangPro runs entirely for free using the following cloud stack:

| Service | Role | Tier |
|---|---|---|
| [Supabase](https://supabase.com/) | PostgreSQL database | Free (500 MB) |
| [Render](https://render.com/) | Backend .NET API (Docker) | Free (750 hrs/mo) |
| [Vercel](https://vercel.com/) | Frontend Angular SPA | Free (Hobby) |

> 📖 See **[DEPLOY-GUIDE.md](DEPLOY-GUIDE.md)** for detailed step-by-step deployment instructions.

---

## 🔑 Demo Accounts

After running `supabase-init.sql` or on first app startup, the following accounts are available:

| Username | Password | Role | Access Level |
|---|---|---|---|
| `admin` | `admin123` | **Admin Gudang** | Full access — Master data, User management, Transactions, Audit log |
| `sari` | `sari123` | **Staf Gudang** | Operational — Inbound, Outbound, Transaction history, Stock card |
| `hendra` | `hendra123` | **Pemilik** | Executive — Dashboard monitoring, Reports, Transaction overview |

---

## 📡 API Endpoints

| Method | Endpoint | Description | Role |
|---|---|---|---|
| `POST` | `/api/auth/login` | User login (username + password) | All |
| `POST` | `/api/auth/refresh` | Refresh access token | All |
| `GET` | `/api/items` | List items (search, filter, pagination) | All |
| `POST` | `/api/items` | Create new item | Admin |
| `PUT` | `/api/items/{id}` | Update item details | Admin |
| `PATCH` | `/api/items/{id}/deactivate` | Soft-delete (deactivate) item | Admin |
| `GET` | `/api/warehouses` | List warehouses | All |
| `POST` | `/api/warehouses` | Create new warehouse | Admin |
| `POST` | `/api/transactions/in` | Record inbound transaction | Admin, Staff |
| `POST` | `/api/transactions/out` | Record outbound transaction | Admin, Staff |
| `POST` | `/api/transactions/{id}/cancel` | Cancel transaction (reversal entry) | Admin |
| `GET` | `/api/transactions` | Transaction history (filter: warehouse, date, type) | All |
| `GET` | `/api/stock` | Stock levels per item per warehouse | All |
| `GET` | `/api/stock/{itemId}/card` | Stock card (mutation history) | All |
| `GET` | `/api/dashboard/summary` | Dashboard KPI summary | All |
| `GET` | `/api/dashboard/charts` | Dashboard chart data | All |
| `GET` | `/api/alerts` | Alert notifications list | All |
| `PATCH` | `/api/alerts/{id}/read` | Mark alert as read | All |
| `GET` | `/api/users` | List users | Admin |
| `POST` | `/api/users` | Create new user | Admin |
| `GET` | `/api/audit-logs` | Audit trail log | Admin |
| `GET` | `/api/reports/export` | Export report (CSV) | Admin |
| `GET` | `/api/health` | Database connectivity check | Public |

---

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                     Vercel (Edge Network)                        │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │                  Angular 20 SPA                            │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │  │
│  │  │ Dashboard │  │  Master  │  │  Trans-  │  │  Admin   │  │  │
│  │  │   Page   │  │  Data    │  │  actions  │  │  Panel   │  │  │
│  │  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘  │  │
│  │       └──────────────┼──────────────┼─────────────┘        │  │
│  │                      │ /api/* rewrite (vercel.json)        │  │
│  └──────────────────────┼─────────────────────────────────────┘  │
└─────────────────────────┼────────────────────────────────────────┘
                          │ HTTPS (Reverse Proxy)
┌─────────────────────────▼────────────────────────────────────────┐
│                    Render (Docker Container)                     │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │              ASP.NET Core 10 Web API                       │  │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │  │
│  │  │   REST   │  │   JWT    │  │ Business │  │  Audit   │  │  │
│  │  │ Control- │  │   Auth   │  │  Logic   │  │  Logger  │  │  │
│  │  │  lers    │  │ + RBAC   │  │ Services │  │ (Serilog)│  │  │
│  │  └────┬─────┘  └──────────┘  └────┬─────┘  └──────────┘  │  │
│  │       └───────────────────────────┘                        │  │
│  │                      │ Entity Framework Core               │  │
│  └──────────────────────┼─────────────────────────────────────┘  │
└─────────────────────────┼────────────────────────────────────────┘
                          │ SSL/TLS (Port 5432)
              ┌───────────▼───────────┐
              │   Supabase Postgres   │
              │   (Connection Pooler) │
              │   Region: Singapore   │
              └───────────────────────┘
```

**Dual-Database Engine**: The backend automatically detects the connection string format and switches between SQL Server (local development) and PostgreSQL/Supabase (cloud deployment) — no code changes required.

---

## 🔒 Security

- **Authentication**: Stateless JWT Bearer tokens with automatic refresh token rotation
- **Password Hashing**: BCrypt with per-user random salt (work factor 11)
- **Account Protection**: Auto-lockout for 15 minutes after 5 consecutive failed logins
- **Access Control**: Role-based middleware — Staff can only access their assigned warehouses
- **Data Integrity**: Optimistic concurrency tokens prevent race conditions on stock updates
- **SQL Injection**: Prevented by parameterized queries through Entity Framework Core
- **CORS**: Whitelisted origins only (Vercel frontend domain)
- **Audit Trail**: Immutable, append-only log of all user actions with IP tracking

---

## 🧪 Testing

- **Business Logic Tests**: Stock validation, negative stock prevention, concurrency conflict handling
- **Concurrent Transaction Tests**: Two simultaneous outbound transactions on the same item
- **BCrypt Hash Verification**: Seed password integrity checks

Run tests:
```bash
npm run backend:test
```

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).

---

## 🤝 Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.

1. Fork this repository
2. Create a feature branch (`git checkout -b feature/your-feature`)
3. Commit your changes (`git commit -m 'Add new feature'`)
4. Push to branch (`git push origin feature/your-feature`)
5. Open a Pull Request

---

<p align="center">
  Built with ❤️ for SME manufacturers — by <a href="https://github.com/Hikmaldev">Hikmaldev</a>
</p>
