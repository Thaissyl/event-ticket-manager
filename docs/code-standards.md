# Code Standards

**Last Updated:** 2026-02-04
**Version:** 1.0.0

## Overview

This document defines coding conventions, naming standards, file organization, and best practices for the Event Ticket Manager codebase. All contributors must adhere to these standards to maintain consistency and code quality.

## General Principles

1. **YAGNI** (You Aren't Gonna Need It): Don't implement features until required
2. **KISS** (Keep It Simple, Stupid): Favor simple solutions over complex ones
3. **DRY** (Don't Repeat Yourself): Extract reusable code into functions/components
4. **Self-Documenting Code**: Prefer clear naming over excessive comments
5. **Type Safety**: Leverage TypeScript and C# type systems
6. **Error Handling**: Always handle errors gracefully with meaningful messages

## File Organization

### Monorepo Structure

```
event-ticket-manager/
├── src/
│   ├── backend/              # ASP.NET Core backend
│   └── frontend/             # Next.js frontend
├── tools/                    # Build tools (NSwag, scripts)
├── docs/                     # Documentation
├── plans/                    # Implementation plans and reports
├── docker-compose.yml        # Production compose
├── docker-compose.dev.yml    # Development compose
├── .env.example              # Environment template
└── .gitignore                # Git exclusions
```

### Backend Structure (Clean Architecture)

```
src/backend/
├── EventTickets.API/         # HTTP presentation layer
│   ├── Endpoints/            # Minimal API endpoint definitions
│   │   ├── EventEndpoints.cs
│   │   ├── TicketEndpoints.cs
│   │   └── OrderEndpoints.cs
│   ├── Middleware/           # Custom middleware
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── DTOs/                 # Request/response data transfer objects
│   │   ├── Requests/
│   │   │   ├── CreateEventRequest.cs
│   │   │   └── CreateOrderRequest.cs
│   │   └── Responses/
│   │       ├── EventResponse.cs
│   │       └── OrderResponse.cs
│   ├── Validators/           # FluentValidation validators (future)
│   ├── Program.cs            # Application entry point
│   └── appsettings.json      # Configuration
│
├── EventTickets.Core/        # Domain logic (framework-agnostic)
│   ├── Entities/             # Domain models (POCOs)
│   │   ├── ApplicationUser.cs
│   │   ├── Event.cs
│   │   ├── TicketTier.cs
│   │   ├── Order.cs
│   │   └── Ticket.cs
│   ├── Interfaces/           # Repository and service contracts
│   │   ├── IEventRepository.cs
│   │   ├── IOrderRepository.cs
│   │   └── IPaymentService.cs
│   ├── Services/             # Business logic implementations
│   │   └── InventoryService.cs
│   ├── Enums/                # Enumerations
│   │   ├── UserRole.cs
│   │   ├── OrderStatus.cs
│   │   └── PaymentStatus.cs
│   └── Exceptions/           # Custom domain exceptions
│       └── InsufficientInventoryException.cs
│
└── EventTickets.Infrastructure/  # Data access and external services
    ├── Data/                 # EF Core context and configurations
    │   ├── ApplicationDbContext.cs
    │   ├── Configurations/   # EF fluent API configurations
    │   │   ├── EventConfiguration.cs
    │   │   └── OrderConfiguration.cs
    │   └── Migrations/       # EF Core migrations (auto-generated)
    ├── Repositories/         # Repository implementations
    │   ├── EventRepository.cs
    │   └── OrderRepository.cs
    ├── Services/             # External service integrations
    │   ├── SePayService.cs
    │   ├── EmailService.cs
    │   └── QrCodeGenerator.cs
    └── Extensions/           # Dependency injection extensions
        └── ServiceCollectionExtensions.cs
```

### Frontend Structure (Next.js App Router)

```
src/frontend/
├── src/
│   ├── app/                  # App Router pages
│   │   ├── (auth)/           # Auth route group
│   │   │   ├── login/
│   │   │   │   └── page.tsx
│   │   │   └── register/
│   │   │       └── page.tsx
│   │   ├── (dashboard)/      # Organizer dashboard group
│   │   │   ├── events/
│   │   │   │   ├── page.tsx
│   │   │   │   ├── [id]/
│   │   │   │   │   └── page.tsx
│   │   │   │   └── new/
│   │   │   │       └── page.tsx
│   │   │   └── analytics/
│   │   │       └── page.tsx
│   │   ├── (public)/         # Public pages group
│   │   │   ├── events/
│   │   │   │   ├── page.tsx
│   │   │   │   └── [id]/
│   │   │   │       └── page.tsx
│   │   │   └── checkout/
│   │   │       └── page.tsx
│   │   ├── layout.tsx        # Root layout
│   │   ├── page.tsx          # Home page
│   │   └── globals.css       # Global styles
│   ├── components/
│   │   ├── ui/               # shadcn/ui components (DO NOT MODIFY)
│   │   │   ├── button.tsx
│   │   │   ├── card.tsx
│   │   │   └── input.tsx
│   │   ├── shared/           # Custom shared components
│   │   │   ├── header.tsx
│   │   │   ├── footer.tsx
│   │   │   └── event-card.tsx
│   │   ├── event/            # Event-specific components
│   │   │   ├── event-form.tsx
│   │   │   └── ticket-tier-list.tsx
│   │   └── cart/             # Cart components
│   │       ├── cart-item.tsx
│   │       └── cart-summary.tsx
│   ├── lib/
│   │   ├── api-client.ts     # Generated API client (NSwag)
│   │   ├── auth.ts           # NextAuth configuration (future)
│   │   ├── utils.ts          # Utility functions (cn, formatDate, etc.)
│   │   └── constants.ts      # App-wide constants
│   ├── hooks/                # Custom React hooks
│   │   ├── use-cart.ts
│   │   └── use-events.ts
│   ├── types/                # Generated TypeScript types (NSwag)
│   │   └── api.ts
│   └── contexts/             # React Context providers
│       ├── auth-context.tsx
│       └── cart-context.tsx
├── public/                   # Static assets
│   └── images/
├── next.config.ts            # Next.js configuration
├── tailwind.config.ts        # Tailwind CSS configuration
├── tsconfig.json             # TypeScript configuration
└── package.json              # Dependencies
```

## Naming Conventions

### C# (Backend)

| Item | Convention | Example |
|------|------------|---------|
| Namespaces | PascalCase | `EventTickets.Core.Entities` |
| Classes | PascalCase | `ApplicationUser`, `EventRepository` |
| Interfaces | I + PascalCase | `IEventRepository`, `IPaymentService` |
| Methods | PascalCase | `CreateEvent()`, `GetOrderById()` |
| Private fields | _camelCase | `_dbContext`, `_logger` |
| Public properties | PascalCase | `EventName`, `UserId` |
| Parameters | camelCase | `userId`, `eventId` |
| Local variables | camelCase | `order`, `ticketTier` |
| Constants | PascalCase | `MaxTicketsPerOrder` |
| Enums | PascalCase (singular) | `UserRole`, `OrderStatus` |
| Enum values | PascalCase | `UserRole.Organizer`, `OrderStatus.Confirmed` |

### TypeScript/JavaScript (Frontend)

| Item | Convention | Example |
|------|------------|---------|
| Files | kebab-case | `event-card.tsx`, `use-cart.ts` |
| Components | PascalCase | `EventCard`, `TicketTierList` |
| Functions | camelCase | `fetchEvents()`, `formatCurrency()` |
| Variables | camelCase | `eventList`, `userId` |
| Constants | UPPER_SNAKE_CASE | `MAX_TICKETS_PER_ORDER`, `API_BASE_URL` |
| Types/Interfaces | PascalCase | `Event`, `CreateOrderRequest` |
| Hooks | use + camelCase | `useCart`, `useEventList` |
| Context | PascalCase + Context | `AuthContext`, `CartContext` |

### Database (PostgreSQL)

| Item | Convention | Example |
|------|------------|---------|
| Tables | PascalCase (plural) | `Events`, `TicketTiers`, `Orders` |
| Columns | PascalCase | `EventName`, `UserId` |
| Indexes | IX_ + table + columns | `IX_Events_OrganizerId_StartDate` |
| Foreign keys | FK_ + tables | `FK_Orders_Users_UserId` |

## Code Style Guidelines

### C# Backend

**File Header** (optional):
```csharp
// EventTickets.Core/Services/InventoryService.cs
```

**Usings**:
```csharp
// System namespaces first, then third-party, then local
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
```

**Class Structure**:
```csharp
public class InventoryService
{
    // Private fields
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<InventoryService> _logger;

    // Constructor
    public InventoryService(
        IEventRepository eventRepository,
        ILogger<InventoryService> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    // Public methods
    public async Task<bool> CheckAvailability(Guid tierId, int quantity)
    {
        // Implementation
    }

    // Private methods
    private void ValidateInput(int quantity)
    {
        // Implementation
    }
}
```

**Error Handling**:
```csharp
// DO: Throw specific exceptions with messages
if (quantity <= 0)
{
    throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
}

// DO: Use try-catch for expected failures
try
{
    await _dbContext.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Optimistic lock conflict for tier {TierId}", tierId);
    throw new InsufficientInventoryException("Tickets no longer available");
}

// DON'T: Catch generic exceptions without rethrowing
catch (Exception) { } // NEVER DO THIS
```

**Async/Await**:
```csharp
// DO: Use async/await for I/O operations
public async Task<Event> GetEventByIdAsync(Guid id)
{
    return await _dbContext.Events
        .Include(e => e.TicketTiers)
        .FirstOrDefaultAsync(e => e.Id == id);
}

// DON'T: Block on async code
var event = GetEventByIdAsync(id).Result; // Deadlock risk
```

**Nullability**:
```csharp
// Enable nullable reference types in .csproj
<Nullable>enable</Nullable>

// Use nullable annotations
public class Event
{
    public string Title { get; set; } = string.Empty; // Non-nullable
    public string? Description { get; set; }          // Nullable
}
```

### TypeScript Frontend

**File Header** (optional):
```typescript
// src/components/event/event-card.tsx
```

**Imports**:
```typescript
// React imports first, then third-party, then local
import React from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Event } from '@/types/api';
import { formatDate } from '@/lib/utils';
```

**Component Structure**:
```typescript
// DO: Use named exports for components
export function EventCard({ event }: { event: Event }) {
  // Hooks at top
  const [isExpanded, setIsExpanded] = React.useState(false);

  // Event handlers
  const handleToggle = () => setIsExpanded(!isExpanded);

  // Render
  return (
    <Card>
      {/* JSX */}
    </Card>
  );
}

// DON'T: Use default exports (harder to refactor)
export default EventCard;
```

**Type Safety**:
```typescript
// DO: Define types for props
interface EventCardProps {
  event: Event;
  onSelect?: (id: string) => void;
}

export function EventCard({ event, onSelect }: EventCardProps) {
  // Implementation
}

// DO: Use type guards
function isValidEvent(data: unknown): data is Event {
  return typeof data === 'object' && data !== null && 'id' in data;
}
```

**Async Operations**:
```typescript
// DO: Use async/await with error handling
async function fetchEvents() {
  try {
    const response = await fetch('/api/v1/events');
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Failed to fetch events:', error);
    throw error;
  }
}

// DO: Use React Query or SWR for data fetching (future)
const { data, error, isLoading } = useQuery('events', fetchEvents);
```

**Styling**:
```typescript
// DO: Use Tailwind utility classes
<div className="flex items-center justify-between p-4 bg-white rounded-lg shadow">

// DO: Use cn() for conditional classes
import { cn } from '@/lib/utils';
<button className={cn(
  "px-4 py-2 rounded",
  isPrimary ? "bg-blue-600 text-white" : "bg-gray-200"
)}>

// DON'T: Use inline styles (except for dynamic values)
<div style={{ color: 'red' }}> // Avoid
```

## File Size Guidelines

### Modularization Threshold

**Rule**: If a code file exceeds **200 lines**, consider splitting it.

**Exceptions** (do NOT modularize):
- Markdown files
- Plain text files
- Configuration files (JSON, YAML)
- Environment variable files (.env.example)
- Auto-generated files (migrations, types)

**How to Split**:

1. **Backend Classes**:
```csharp
// Before: EventService.cs (300 lines)
public class EventService
{
    public Task Create() { }
    public Task Update() { }
    public Task Delete() { }
    public Task Publish() { }
    public Task ValidateTickets() { }
}

// After: Split into focused classes
EventCreationService.cs
EventUpdateService.cs
EventPublishService.cs
TicketValidationService.cs
```

2. **Frontend Components**:
```typescript
// Before: EventPage.tsx (250 lines)
export function EventPage() {
  // Form logic + API calls + rendering
}

// After: Extract sub-components
EventPage.tsx           // 50 lines (layout)
EventForm.tsx           // 80 lines (form)
EventDetails.tsx        // 60 lines (display)
useEventMutations.ts    // 40 lines (API logic)
```

## Comments and Documentation

### When to Comment

**DO Comment**:
- Complex business logic
- Non-obvious workarounds
- Performance optimizations
- Security considerations
- Regex patterns

```csharp
// SePay webhook requires HMAC-SHA256 signature validation
// to prevent replay attacks. See: https://sepay.vn/docs/webhooks
var isValid = ValidateSignature(payload, signature);
```

**DON'T Comment**:
- Obvious code (self-documenting)
```csharp
// BAD: Get user by ID
var user = await _userRepository.GetByIdAsync(userId);

// GOOD: No comment needed (clear method name)
```

### XML Documentation (C#)

**Required for public APIs**:
```csharp
/// <summary>
/// Validates inventory availability for a ticket tier with optimistic locking.
/// </summary>
/// <param name="tierId">The ticket tier ID.</param>
/// <param name="quantity">Number of tickets requested.</param>
/// <returns>True if inventory is available, otherwise false.</returns>
/// <exception cref="ArgumentException">Thrown when quantity is zero or negative.</exception>
public async Task<bool> CheckAvailability(Guid tierId, int quantity)
{
    // Implementation
}
```

### JSDoc (TypeScript)

**Optional but recommended for utilities**:
```typescript
/**
 * Formats a date in the user's timezone.
 * @param date - ISO date string or Date object
 * @param format - Format string (default: 'MMM DD, YYYY')
 * @returns Formatted date string
 */
export function formatDate(date: string | Date, format = 'MMM DD, YYYY'): string {
  // Implementation
}
```

## Testing Standards

### Backend Tests

**File Naming**: `{ClassName}Tests.cs`
**Location**: `tests/EventTickets.Tests/Services/InventoryServiceTests.cs`

```csharp
using Xunit;

public class InventoryServiceTests
{
    [Fact]
    public async Task CheckAvailability_WithSufficientInventory_ReturnsTrue()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.CheckAvailability(tierId, 5);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckAvailability_WithInvalidQuantity_ThrowsException(int quantity)
    {
        // Arrange, Act, Assert
    }
}
```

### Frontend Tests

**File Naming**: `{ComponentName}.test.tsx`
**Location**: `src/components/event/__tests__/event-card.test.tsx` (future)

```typescript
import { render, screen } from '@testing-library/react';
import { EventCard } from '../event-card';

describe('EventCard', () => {
  it('renders event title', () => {
    const event = { id: '1', title: 'Test Event' };
    render(<EventCard event={event} />);
    expect(screen.getByText('Test Event')).toBeInTheDocument();
  });
});
```

## Git Commit Standards

### Conventional Commits

**Format**: `<type>(<scope>): <subject>`

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only
- `style`: Formatting, no code change
- `refactor`: Code restructuring
- `test`: Adding tests
- `chore`: Build scripts, dependencies

**Examples**:
```
feat(events): add ticket tier creation endpoint
fix(cart): resolve race condition in reservation expiry
docs(readme): update setup instructions
refactor(auth): extract JWT validation to middleware
test(orders): add integration tests for checkout flow
chore(deps): upgrade EF Core to 8.0.1
```

### Pre-Commit Checklist

- [ ] Code compiles without errors
- [ ] Linter passes (ESLint for TS, built-in for C#)
- [ ] Tests pass (if applicable)
- [ ] No sensitive data (API keys, passwords) in commit
- [ ] Commit message follows conventional format

## Security Best Practices

### Input Validation

```csharp
// DO: Validate all user inputs
public async Task<Event> CreateEvent(CreateEventRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        throw new ArgumentException("Title is required");
    }

    if (request.StartDate < DateTime.UtcNow)
    {
        throw new ArgumentException("Start date must be in the future");
    }
}
```

### SQL Injection Prevention

```csharp
// DO: Use parameterized queries (EF Core does this automatically)
var events = await _dbContext.Events
    .Where(e => e.OrganizerId == userId)
    .ToListAsync();

// DON'T: String concatenation in raw SQL
_dbContext.Database.ExecuteSqlRaw($"SELECT * FROM Events WHERE UserId = '{userId}'"); // DANGEROUS
```

### XSS Prevention

```typescript
// DO: React escapes by default
<p>{event.description}</p>

// DON'T: Use dangerouslySetInnerHTML without sanitization
<div dangerouslySetInnerHTML={{ __html: userInput }} /> // DANGEROUS
```

### Environment Variables

```env
# DO: Use .env.example as template, never commit .env
# .env.example
POSTGRES_PASSWORD=your_password_here
JWT_SECRET=your_jwt_secret_here_min_32_chars

# DON'T: Hardcode secrets in code
const jwtSecret = "my-secret-key"; // NEVER
```

## Performance Best Practices

### Backend

```csharp
// DO: Use async/await for I/O
await _dbContext.SaveChangesAsync();

// DO: Include related entities explicitly to avoid N+1 queries
var events = await _dbContext.Events
    .Include(e => e.TicketTiers)
    .ToListAsync();

// DON'T: Load all data then filter in memory
var allEvents = await _dbContext.Events.ToListAsync(); // Loads everything
var filtered = allEvents.Where(e => e.OrganizerId == userId).ToList();
```

### Frontend

```typescript
// DO: Use React.memo for expensive components
export const EventList = React.memo(({ events }) => {
  // Rendering logic
});

// DO: Debounce search inputs
const debouncedSearch = useMemo(
  () => debounce((query) => fetchEvents(query), 300),
  []
);

// DON'T: Create functions inside render
<button onClick={() => handleClick(id)}> // Creates new function every render
```

## Accessibility (a11y)

### Semantic HTML

```typescript
// DO: Use semantic elements
<nav>
  <ul>
    <li><a href="/events">Events</a></li>
  </ul>
</nav>

// DON'T: Use divs for everything
<div onClick={handleClick}>Click me</div> // Should be <button>
```

### ARIA Attributes

```typescript
// DO: Add ARIA labels for screen readers
<button aria-label="Close modal" onClick={handleClose}>
  <X size={16} />
</button>

// DO: Manage focus for modals
<dialog role="dialog" aria-modal="true">
```

## Code Review Checklist

Before marking a PR as ready:

- [ ] Code follows naming conventions (C#: PascalCase, TS: camelCase)
- [ ] No hardcoded secrets or credentials
- [ ] Error handling implemented with meaningful messages
- [ ] Input validation on all user inputs
- [ ] Database queries optimized (includes, indexes)
- [ ] Comments added for complex logic
- [ ] Tests added/updated (if applicable)
- [ ] Linter passes without warnings
- [ ] No console.log or Debug.WriteLine in production code
- [ ] Files under 200 lines (or split appropriately)
- [ ] Accessibility considered (semantic HTML, ARIA)

## Tools and Linters

### Backend

- **Compiler**: .NET 8 SDK
- **Linter**: Built-in (enable warnings as errors in .csproj)
- **Formatter**: EditorConfig or Rider/VS settings
- **Test Framework**: xUnit (future)

### Frontend

- **Compiler**: TypeScript 5.x
- **Linter**: ESLint (configured in `eslint.config.mjs`)
- **Formatter**: Prettier (future)
- **Test Framework**: Jest + React Testing Library (future)

## References

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Next.js Documentation](https://nextjs.org/docs)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [TypeScript Style Guide](https://google.github.io/styleguide/tsguide.html)
