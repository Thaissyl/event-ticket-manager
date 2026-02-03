# Phase 11: Testing & Deployment

## Context Links
- [Main Plan](plan.md)
- [Architecture Report](../reports/researcher-260202-1222-nextjs-dotnet-architecture.md)

## Overview
- **Priority:** P1 (Quality assurance)
- **Status:** pending
- **Effort:** 6h
- **Description:** Comprehensive testing, CI/CD pipeline, production deployment

## Key Insights
- Unit tests for business logic
- Integration tests for API endpoints
- E2E tests for critical user flows
- Docker-based deployment
- CI/CD with GitHub Actions

## Requirements

### Functional
- F1: Unit tests for all services
- F2: Integration tests for API
- F3: E2E tests for critical paths
- F4: Automated CI/CD pipeline
- F5: Production Docker deployment

### Non-Functional
- NF1: >80% code coverage
- NF2: All tests pass before merge
- NF3: Deploy in <5 minutes

## Architecture

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

### CI/CD Pipeline

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

### Test Coverage Targets

| Component | Target | Priority |
|-----------|--------|----------|
| Core Services | 90% | High |
| API Endpoints | 85% | High |
| Frontend Utils | 80% | Medium |
| React Components | 70% | Medium |
| E2E Flows | 5 critical paths | High |

## Related Code Files

### Create (Backend Tests)
- `src/backend/EventTickets.Tests/EventTickets.Tests.csproj`
- `src/backend/EventTickets.Tests/Unit/Services/EventServiceTests.cs`
- `src/backend/EventTickets.Tests/Unit/Services/CartServiceTests.cs`
- `src/backend/EventTickets.Tests/Unit/Services/PaymentServiceTests.cs`
- `src/backend/EventTickets.Tests/Unit/Services/CheckinServiceTests.cs`
- `src/backend/EventTickets.Tests/Integration/EventEndpointsTests.cs`
- `src/backend/EventTickets.Tests/Integration/OrderEndpointsTests.cs`
- `src/backend/EventTickets.Tests/Integration/WebhookTests.cs`
- `src/backend/EventTickets.Tests/Fixtures/TestDatabaseFixture.cs`
- `src/backend/EventTickets.Tests/Helpers/TestAuthHandler.cs`

### Create (Frontend Tests)
- `src/frontend/__tests__/components/event-card.test.tsx`
- `src/frontend/__tests__/components/cart-drawer.test.tsx`
- `src/frontend/__tests__/hooks/use-cart.test.ts`
- `src/frontend/__tests__/lib/api-client.test.ts`
- `src/frontend/e2e/purchase-flow.spec.ts`
- `src/frontend/e2e/organizer-flow.spec.ts`
- `src/frontend/e2e/checkin-flow.spec.ts`
- `src/frontend/playwright.config.ts`

### Create (CI/CD)
- `.github/workflows/ci.yml`
- `.github/workflows/deploy-staging.yml`
- `.github/workflows/deploy-production.yml`
- `src/frontend/Dockerfile`
- `src/backend/EventTickets.API/Dockerfile`
- `docker-compose.prod.yml`
- `scripts/deploy.sh`

## Implementation Steps

### 1. Backend Unit Tests (1.5h)
- [ ] Create test project with xUnit
- [ ] Add Moq for mocking
- [ ] Write EventService tests
- [ ] Write CartService tests (reservation logic)
- [ ] Write PaymentService tests (webhook handling)
- [ ] Write CheckinService tests (validation)
- [ ] Aim for >90% coverage on services

### 2. Backend Integration Tests (1h)
- [ ] Setup test database with Testcontainers
- [ ] Create test auth handler
- [ ] Write API endpoint tests
- [ ] Test authorization rules
- [ ] Test webhook endpoint

### 3. Frontend Tests (1h)
- [ ] Configure Vitest for unit tests
- [ ] Write component tests with Testing Library
- [ ] Test custom hooks
- [ ] Test API client error handling

### 4. E2E Tests (1h)
- [ ] Configure Playwright
- [ ] Write purchase flow test
- [ ] Write organizer event creation test
- [ ] Write check-in flow test
- [ ] Configure test data seeding

### 5. CI/CD Pipeline (1h)
- [ ] Create GitHub Actions workflow
- [ ] Add build step for both projects
- [ ] Add test step with coverage
- [ ] Add Docker build step
- [ ] Configure deployment triggers
- [ ] Add environment secrets

### 6. Production Deployment (0.5h)
- [ ] Create production Docker Compose
- [ ] Configure environment variables
- [ ] Setup database migration on deploy
- [ ] Configure health checks
- [ ] Document deployment process

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
            .ReturnsAsync(new TicketTier { QuantityTotal = 100, QuantitySold = 50, QuantityReserved = 10 });

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
            .ReturnsAsync(new TicketTier { QuantityTotal = 100, QuantitySold = 100 });

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

## Todo List
- [ ] Create backend test project
- [ ] Write service unit tests
- [ ] Write API integration tests
- [ ] Configure frontend testing
- [ ] Write component tests
- [ ] Configure Playwright
- [ ] Write E2E tests
- [ ] Create CI workflow
- [ ] Create deployment workflow
- [ ] Create production Docker setup
- [ ] Document deployment

## Success Criteria
- [ ] All tests pass
- [ ] >80% code coverage
- [ ] CI runs on every PR
- [ ] Deployment works to staging
- [ ] Production deployment documented

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Flaky tests | Medium | Medium | Retry logic, stable selectors |
| Slow CI | Medium | Low | Parallel jobs, caching |
| Deployment failures | Low | High | Rollback strategy |

## Security Considerations
- Store secrets in GitHub Secrets
- Use OIDC for cloud deployments
- Scan Docker images for vulnerabilities
- Run SAST/DAST in CI
- Rotate secrets regularly

## Deployment Checklist
- [ ] Database migrations applied
- [ ] Environment variables configured
- [ ] SSL certificates valid
- [ ] Health checks passing
- [ ] Monitoring/alerting configured
- [ ] Backup strategy in place
- [ ] Rollback tested

## Post-Deployment Verification
1. Verify home page loads
2. Test login/registration
3. Test event listing
4. Test add to cart
5. Verify webhook endpoint responds
6. Check admin panel access
7. Monitor error rates

## Project Complete!
After this phase, the Event Ticket Manager platform is ready for launch.
