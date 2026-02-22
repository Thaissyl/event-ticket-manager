# Event Ticket Manager

> A full-stack event ticketing platform with real-time inventory, QR code check-in, and payment integration.

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Next.js](https://img.shields.io/badge/Next.js-15-black.svg)](https://nextjs.org/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)](https://www.postgresql.org/)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)

## Features

- **Event Management**: Create, publish, and manage events with multiple ticket tiers
- **Ticket Sales**: Real-time inventory tracking with reserved cart system
- **Payment Integration**: SePay QR code payments with webhook processing
- **QR Check-in**: Mobile-friendly ticket validation with QR codes
- **Analytics Dashboard**: Sales data, check-in metrics, and revenue tracking
- **Role-based Access**: Organizers, attendees, and admin interfaces

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0 (Minimal APIs)
- **Architecture**: Clean Architecture (Core, Infrastructure, API layers)
- **Database**: PostgreSQL 16 with Entity Framework Core
- **Authentication**: ASP.NET Identity + JWT
- **API Docs**: Swagger/OpenAPI 3.0
- **Payment**: SePay integration

### Frontend
- **Framework**: Next.js 15 (App Router)
- **UI**: shadcn/ui + Tailwind CSS
- **State**: TanStack Query + Zustand
- **Auth**: NextAuth.js
- **Validation**: Zod

### DevOps
- **Containerization**: Docker & Docker Compose
- **Code Quality**: ESLint, Prettier

## Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/your-username/event-ticket-manager.git
cd event-ticket-manager

# Copy environment file
cp .env.example .env

# Edit .env with your configuration
nano .env

# Start all services
docker-compose up -d

# Run database migrations
docker-compose exec backend dotnet ef database update

# Access the application
# Frontend: http://localhost:3000
# Backend API: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### Local Development

#### Prerequisites
- .NET 8.0 SDK
- Node.js 20+
- PostgreSQL 16
- dotnet-ef tool

#### Backend Setup

```bash
cd src/backend

# Restore dependencies
dotnet restore

# Set up connection string in src/backend/EventTickets.API/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=event_tickets;Username=postgres;Password=your_password"
  }
}

# Run migrations
dotnet ef database update --project EventTickets.API

# Run the API
cd EventTickets.API
dotnet run
```

#### Frontend Setup

```bash
cd src/frontend

# Install dependencies
npm install

# Copy environment file
cp .env.example .env.local

# Run dev server
npm run dev
```

## Project Structure

```
event-ticket-manager/
├── src/
│   ├── backend/
│   │   ├── EventTickets.API/          # Minimal API endpoints
│   │   │   ├── Endpoints/             # Route handlers
│   │   │   │   ├── AuthEndpoints.cs
│   │   │   │   ├── EventEndpoints.cs
│   │   │   │   ├── TicketTierEndpoints.cs
│   │   │   │   ├── CartEndpoints.cs
│   │   │   │   ├── OrderEndpoints.cs
│   │   │   │   ├── PaymentEndpoints.cs
│   │   │   │   ├── CheckinEndpoints.cs
│   │   │   │   ├── AnalyticsEndpoints.cs
│   │   │   │   └── AdminEndpoints.cs
│   │   │   ├── Middleware/            # Custom middleware
│   │   │   └── Program.cs             # Application entry point
│   │   ├── EventTickets.Core/         # Domain layer
│   │   │   ├── Entities/              # Domain models
│   │   │   ├── Interfaces/            # Repository interfaces
│   │   │   ├── DTOs/                  # Data transfer objects
│   │   │   └── Enums/                 # Enumerations
│   │   └── EventTickets.Infrastructure/# Data access layer
│   │       ├── Data/                  # DbContext
│   │       └── Repositories/          # Repository implementations
│   └── frontend/
│       ├── src/
│       │   ├── app/                   # Next.js App Router
│       │   ├── components/            # React components
│       │   │   ├── ui/                # shadcn/ui components
│       │   │   └── ...
│       │   ├── lib/                   # Utilities
│       │   └── api/                   # API client & types
│       │       └── generated/         # Auto-generated types
│       └── public/                    # Static assets
├── scripts/
│   └── generate-api-types.sh          # TypeScript type generator
├── docs/                              # Project documentation
├── plans/                             # Development plans
└── docker-compose.yml                 # Container orchestration
```

## API Documentation

### Interactive Documentation
- **Swagger UI**: http://localhost:5000/swagger
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json

### Main Endpoints

| Resource | Methods | Description |
|----------|---------|-------------|
| `/api/events` | GET, POST | List/create events |
| `/api/events/{id}` | GET, PUT, DELETE | Event details |
| `/api/events/{id}/tiers` | GET, POST | Ticket tier management |
| `/api/cart` | GET, POST, PUT, DELETE | Shopping cart |
| `/api/orders` | GET, POST | Order management |
| `/api/payments/sepay/webhook` | POST | Payment callbacks |
| `/api/checkin` | POST | Ticket validation |
| `/api/analytics` | GET | Sales & metrics |
| `/api/admin` | GET | Admin endpoints |

### Generating TypeScript Types

```bash
# Start the backend API
cd src/backend/EventTickets.API && dotnet run

# In another terminal, generate types
./scripts/generate-api-types.sh
```

## Database Schema

### Core Tables

| Table | Description |
|-------|-------------|
| `Events` | Event details |
| `TicketTiers` | Pricing tiers per event |
| `Orders` | Purchase orders |
| `Tickets` | Individual tickets with QR codes |
| `CartReservations` | Temporary cart holds |
| `Payments` | Payment transactions |
| `AspNetUsers` | User accounts |

### Key Relationships
- Event → TicketTiers (1:N)
- Event → Orders (N:1 via OrganizerId)
- Order → Tickets (1:N)
- TicketTier → Tickets (1:N)
- Order → Payment (1:1)

## Development Guide

### Adding a New Endpoint

1. **Create DTO** in `src/backend/EventTickets.Core/DTOs/`
2. **Add repository method** (if needed) in `src/backend/EventTickets.Core/Interfaces/`
3. **Implement repository** in `src/backend/EventTickets.Infrastructure/Repositories/`
4. **Create endpoint module** in `src/backend/EventTickets.API/Endpoints/`
5. **Register in** `EndpointExtensions.cs`
6. **Generate TypeScript types** using the script

### Running Tests

```bash
# Backend
cd src/backend
dotnet test

# Frontend
cd src/frontend
npm test
```

### Database Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName --project EventTickets.API

# Apply migration
dotnet ef database update --project EventTickets.API

# Rollback migration
dotnet ef database update PreviousMigration --project EventTickets.API
```

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `POSTGRES_HOST` | Database host | localhost |
| `POSTGRES_DB` | Database name | event_tickets |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET environment | Development |
| `JWT_SECRET` | JWT signing key | *(required)* |
| `SEPAY_API_TOKEN` | SePay API token | *(required)* |

See `.env.example` for full list.

## Deployment

### Production Build

```bash
# Build backend
cd src/backend/EventTickets.API
dotnet publish -c Release -o out

# Build frontend
cd src/frontend
npm run build
```

### Docker Deployment

```bash
# Build and start production containers
docker-compose -f docker-compose.yml up -d --build
```

## Roadmap

- [x] Phase 01: Project Setup & Database Schema
- [x] Phase 02: Database Implementation
- [x] Phase 03: Backend API Structure
- [ ] Phase 04: Authentication & Authorization
- [ ] Phase 05: Frontend Core UI
- [ ] Phase 06: Event Management UI
- [ ] Phase 07: Cart & Checkout Flow
- [ ] Phase 08: Payment Integration
- [ ] Phase 09: Check-in Mobile App
- [ ] Phase 10: Analytics Dashboard

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Commit Convention
- `feat:` New feature
- `fix:` Bug fix
- `refactor:` Code refactoring
- `docs:` Documentation changes
- `test:` Test additions/changes
- `chore:` Maintenance tasks

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Next.js](https://nextjs.org/docs)
- [shadcn/ui](https://ui.shadcn.com/)
- [SePay](https://sepay.vn/) for payment gateway

---

**Made with ❤️ for event organizers everywhere**
