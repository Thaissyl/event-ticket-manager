# Next.js 14 + ASP.NET Core 8 Fullstack Architecture Research

**Date:** 2026-02-02
**Researcher:** researcher-a94cd1c
**Context:** D:\event-ticket-manager

---

## 1. Monorepo Project Structure

### Recommended Layout
```
event-ticket-manager/
├── src/
│   ├── frontend/               # Next.js 14 App Router
│   │   ├── app/
│   │   ├── components/
│   │   ├── lib/
│   │   ├── types/             # Generated from backend
│   │   └── next.config.js
│   └── backend/               # ASP.NET Core 8
│       ├── API/               # Web API project
│       ├── Core/              # Domain models, interfaces
│       ├── Infrastructure/    # EF Core, repositories
│       └── Contracts/         # DTOs for type generation
├── tools/
│   └── type-generator/        # C# to TypeScript generator
├── .env.local
└── docker-compose.yml
```

### Key Principles
- **Separation of concerns**: Frontend/backend isolated with clear boundaries
- **Shared contracts**: Single source of truth for API types
- **Independent deployability**: Each can be containerized separately
- **Development isolation**: Run independently or together via docker-compose

---

## 2. API Communication Patterns

### REST vs GraphQL Decision Matrix

**Choose REST when:**
- Simple CRUD operations dominate
- Performance critical (less overhead)
- Team familiar with RESTful patterns
- Caching requirements straightforward

**Choose GraphQL when:**
- Complex nested data relationships
- Mobile apps need flexible queries
- Over-fetching is major concern
- Real-time subscriptions needed

### Recommended: REST for Event Ticketing
**Rationale:**
- Event/ticket models are relatively flat
- Standard HTTP caching works well
- ASP.NET Core 8 Minimal APIs provide excellent DX
- TypeScript client generation simpler with OpenAPI/Swagger

### Implementation Pattern
```csharp
// ASP.NET Core 8 Minimal API
app.MapGroup("/api/events")
   .MapGet("/", GetEvents)
   .MapGet("/{id}", GetEvent)
   .MapPost("/", CreateEvent)
   .RequireAuthorization();
```

```typescript
// Next.js 14 Server Action or Route Handler
export async function getEvents() {
  const res = await fetch(`${API_URL}/api/events`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return res.json() as Event[];
}
```

---

## 3. Authentication Architecture

### Hybrid Approach: NextAuth + ASP.NET Core Identity

**Flow:**
1. NextAuth handles OAuth providers (Google, GitHub) in Next.js
2. NextAuth issues JWT on successful login
3. JWT sent to ASP.NET Core for validation
4. ASP.NET Core Identity manages user claims/roles

**Implementation:**

```typescript
// next-auth.config.ts
export const authOptions: NextAuthOptions = {
  providers: [
    GoogleProvider({
      clientId: process.env.GOOGLE_CLIENT_ID!,
      clientSecret: process.env.GOOGLE_CLIENT_SECRET!,
    }),
  ],
  callbacks: {
    async jwt({ token, user }) {
      // Call ASP.NET Core to get roles/claims
      if (user) {
        const backendUser = await fetch(`${API_URL}/auth/user-info`, {
          headers: { Authorization: `Bearer ${token.accessToken}` }
        });
        token.roles = (await backendUser.json()).roles;
      }
      return token;
    },
  },
  jwt: {
    secret: process.env.NEXTAUTH_SECRET,
  },
};
```

```csharp
// ASP.NET Core JWT validation
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
```

**Alternative: Backend-only Auth**
- ASP.NET Core Identity handles everything
- Next.js uses cookies (HttpOnly, Secure)
- Simpler but less flexible for OAuth

---

## 4. Shared Types/Contracts

### Strategy: C# as Source of Truth

**Tools:**
- **NSwag**: Generate TypeScript from OpenAPI spec
- **TypeScript.MSBuild**: Manual type definitions
- **Custom Generator**: Tailored for specific needs

**Recommended: NSwag**

```xml
<!-- Backend.API.csproj -->
<ItemGroup>
  <PackageReference Include="NSwag.AspNetCore" Version="14.0.3" />
</ItemGroup>
```

```csharp
// Program.cs
app.UseOpenApi();
app.UseSwaggerUi();

// Generate TypeScript on build
services.AddOpenApiDocument(config =>
{
    config.PostProcess = document =>
    {
        document.Info.Title = "Event Ticket API";
    };
});
```

```json
// package.json
{
  "scripts": {
    "generate-types": "nswag run nswag.json",
    "prebuild": "npm run generate-types"
  }
}
```

**nswag.json:**
```json
{
  "runtime": "Net80",
  "defaultVariables": null,
  "documentGenerator": {
    "aspNetCoreToOpenApi": {
      "project": "../src/backend/API/API.csproj",
      "output": "swagger.json"
    }
  },
  "codeGenerators": {
    "openApiToTypeScriptClient": {
      "output": "../src/frontend/types/api-client.ts",
      "template": "Fetch",
      "generateClientInterfaces": true,
      "generateOptionalParameters": true
    }
  }
}
```

**Manual Types Alternative:**
```csharp
// Contracts/EventDto.cs
public record EventDto(
    Guid Id,
    string Name,
    DateTime StartDate,
    decimal Price
);
```

```typescript
// types/event.ts
export interface EventDto {
  id: string;
  name: string;
  startDate: string;
  price: number;
}
```

---

## 5. Development Workflow

### Local Development Setup

**Option A: Separate Processes**
```bash
# Terminal 1 - Backend
cd src/backend
dotnet watch run --project API

# Terminal 2 - Frontend
cd src/frontend
npm run dev
```

**Option B: Docker Compose (Recommended)**
```yaml
# docker-compose.dev.yml
version: '3.8'
services:
  frontend:
    build:
      context: ./src/frontend
      dockerfile: Dockerfile.dev
    ports:
      - "3000:3000"
    volumes:
      - ./src/frontend:/app
      - /app/node_modules
      - /app/.next
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:5000
    depends_on:
      - backend

  backend:
    build:
      context: ./src/backend
      dockerfile: Dockerfile.dev
    ports:
      - "5000:5000"
    volumes:
      - ./src/backend:/app
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=...
    depends_on:
      - postgres

  postgres:
    image: postgres:16
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=eventtickets
      - POSTGRES_USER=dev
      - POSTGRES_PASSWORD=dev123
```

### Hot Reloading
- **Next.js**: Built-in Fast Refresh (works automatically)
- **ASP.NET Core**: `dotnet watch` monitors file changes
- **Docker**: Use volumes to sync local changes

### Type Generation Workflow
1. Modify C# DTOs/contracts
2. Build backend (triggers OpenAPI generation)
3. Run `npm run generate-types`
4. TypeScript types auto-update in frontend
5. TypeScript compiler catches breaking changes

**Automation:**
```json
// package.json
{
  "scripts": {
    "dev": "concurrently \"npm:dev:*\"",
    "dev:types": "nodemon --watch ../backend/Contracts --exec 'npm run generate-types'",
    "dev:next": "next dev"
  }
}
```

---

## Key Recommendations

1. **Use REST with Minimal APIs** - Simpler, faster, adequate for ticket management
2. **NextAuth for frontend OAuth** - Integrate with backend Identity for roles
3. **NSwag for type generation** - Automated, reliable, reduces drift
4. **Docker Compose for dev** - Consistent environment, easy onboarding
5. **Separate deployment** - Frontend (Vercel/AWS) + Backend (Azure/AWS) for scalability

---

## Unresolved Questions

1. **Session Management**: Should sessions be stored in Redis or database?
2. **File Uploads**: Where to host ticket PDFs/QR codes (S3, Azure Blob, local)?
3. **Real-time Updates**: Need SignalR for live seat availability?
4. **Rate Limiting**: Implement in Next.js middleware or ASP.NET Core?
5. **Deployment Strategy**: Monorepo deploy together or separate CI/CD pipelines?
