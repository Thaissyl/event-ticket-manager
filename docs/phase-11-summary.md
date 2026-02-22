# Phase 11: Testing & Deployment - Summary

**Status:** ⏳ PENDING
**Effort:** 6h
**Priority:** P1 (Quality assurance)

---

## What Is Planned

### Testing Strategy
- [ ] Unit tests for backend services
- [ ] Integration tests for API endpoints
- [ ] Component tests for frontend
- [ ] E2E tests for critical flows
- [ ] CI/CD pipeline with GitHub Actions
- [ ] Production Docker deployment
- [ ] >80% code coverage target

### Testing Pyramid

```
        /\
       /  \
      / E2E \        Few, slow, expensive
     /--------\      (Critical flows only)
    /Integration\    More, medium speed
   /--------------\  (API, DB tests)
  /     Unit       \ Many, fast, cheap
 /------------------\ (Services, logic)
```

### Test Coverage Targets

| Component | Target | Priority |
|-----------|--------|----------|
| Core Services | 90% | High |
| API Endpoints | 85% | High |
| Frontend Utils | 80% | Medium |
| React Components | 70% | Medium |
| E2E Flows | 5 critical paths | High |

---

## CI/CD Pipeline

```
┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐
│  Push   │───>│  Build  │───>│  Test   │───>│ Deploy  │
│  to PR  │    │  + Lint │    │  Suite  │    │ Staging │
└─────────┘    └─────────┘    └─────────┘    └─────────┘
                                                  │
                                                  ▼
┌─────────┐    ┌─────────┐    ┌─────────────────────────┐
│  Merge  │───>│  Build  │───>│  Deploy to Production   │
│ to main │    │  Docker │    │  (with approval)        │
└─────────┘    └─────────┘    └─────────────────────────┘
```

---

## Test Examples

### Unit Test (C#)

```csharp
public class CartServiceTests
{
    [Fact]
    public async Task ReserveTickets_WhenAvailable_ReturnsTrue()
    {
        // Arrange
        var mockRepo = new Mock<ITicketTierRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new TicketTier {
                QuantityTotal = 100,
                QuantitySold = 50,
                QuantityReserved = 10
            });

        var service = new CartService(mockRepo.Object);

        // Act
        var result = await service.ReserveTickets(Guid.NewGuid(), 5);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ReserveTickets_WhenNotAvailable_ReturnsFalse()
    {
        // Arrange - sold out
        var mockRepo = new Mock<ITicketTierRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new TicketTier {
                QuantityTotal = 100,
                QuantitySold = 100
            });

        var service = new CartService(mockRepo.Object);

        // Act
        var result = await service.ReserveTickets(Guid.NewGuid(), 5);

        // Assert
        Assert.False(result);
    }
}
```

### E2E Test (Playwright)

```typescript
test('complete purchase flow', async ({ page }) => {
  // Browse to event
  await page.goto('/events');
  await page.click('[data-testid="event-card"]:first-child');

  // Add to cart
  await page.click('[data-testid="add-to-cart"]');
  await page.fill('[data-testid="quantity-input"]', '2');
  await page.click('[data-testid="confirm-add"]');

  // Checkout
  await page.click('[data-testid="cart-button"]');
  await page.click('[data-testid="checkout-button"]');

  // Fill guest details
  await page.fill('[data-testid="email"]', 'test@example.com');
  await page.fill('[data-testid="name"]', 'Test User');
  await page.click('[data-testid="place-order"]');

  // Verify QR code page
  await expect(page).toHaveURL(/\/payment\//);
  await expect(page.locator('[data-testid="vietqr-code"]')).toBeVisible();
});
```

---

## GitHub Actions Workflow

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  backend:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_DB: testdb
          POSTGRES_PASSWORD: test
        ports:
          - 5432:5432
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore src/backend
      - run: dotnet build src/backend --no-restore
      - run: dotnet test src/backend --no-build --collect:"XPlat Code Coverage"

  frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - run: npm ci
        working-directory: src/frontend
      - run: npm run lint
        working-directory: src/frontend
      - run: npm run test:coverage
        working-directory: src/frontend
      - run: npm run build
        working-directory: src/frontend
```

---

## Implementation Steps

### 1. Backend Unit Tests (1.5h)
- [ ] Create test project with xUnit
- [ ] Add Moq for mocking
- [ ] EventService tests
- [ ] CartService tests (reservation logic)
- [ ] PaymentService tests (webhook handling)
- [ ] CheckinService tests (validation)
- [ ] Aim for >90% coverage on services

### 2. Backend Integration Tests (1h)
- [ ] Setup test database with Testcontainers
- [ ] Create test auth handler
- [ ] API endpoint tests
- [ ] Authorization rule tests
- [ ] Webhook endpoint test

### 3. Frontend Tests (1h)
- [ ] Configure Vitest for unit tests
- [ ] Component tests with Testing Library
- [ ] Custom hooks tests
- [ ] API client error handling tests

### 4. E2E Tests (1h)
- [ ] Configure Playwright
- [ ] Purchase flow test
- [ ] Organizer event creation test
- [ ] Check-in flow test
- [ ] Test data seeding

### 5. CI/CD Pipeline (1h)
- [ ] GitHub Actions workflow
- [ ] Build step for both projects
- [ ] Test step with coverage
- [ ] Docker build step
- [ ] Deployment triggers
- [ ] Environment secrets

### 6. Production Deployment (0.5h)
- [ ] Production Docker Compose
- [ ] Environment variables
- [ ] Database migration on deploy
- [ ] Health checks
- [ ] Deployment documentation

---

## Success Criteria

- [ ] All tests pass
- [ ] >80% code coverage
- [ ] CI runs on every PR
- [ ] Deployment works to staging
- [ ] Production deployment documented

---

## Deployment Checklist

- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] SSL certificates valid
- [ ] Health checks passing
- [ ] Monitoring/alerting configured
- [ ] Backup strategy in place
- [ ] Rollback tested

---

## Post-Deployment Verification

1. Verify home page loads
2. Test login/registration
3. Test event listing
4. Test add to cart
5. Verify webhook endpoint responds
6. Check admin panel access
7. Monitor error rates

---

## Project Complete!

After this phase, the Event Ticket Manager platform is ready for launch.
