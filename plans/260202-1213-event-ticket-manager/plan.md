---
title: "Event Ticket Manager Platform"
description: "Full-stack ticketing platform with Next.js 14, ASP.NET Core 8, PostgreSQL, and SePay VietQR payments"
status: pending
priority: P1
effort: 80h
branch: main
tags: [fullstack, nextjs, aspnet, postgresql, sepay, ticketing]
created: 2026-02-02
---

# Event Ticket Manager - Implementation Plan

## Overview

Multi-organizer event ticketing platform supporting ticket tiers, shopping cart with reservations, VietQR payments via SePay, QR code check-in, and analytics dashboards.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Next.js 14 (App Router), TypeScript, shadcn/ui, Tailwind CSS |
| Backend | ASP.NET Core 8 Minimal APIs, C# |
| Database | PostgreSQL 16, Entity Framework Core 8 |
| Auth | NextAuth/Auth.js (frontend) + ASP.NET Identity (backend) |
| Payments | SePay VietQR bank transfer |
| Structure | Monorepo (src/frontend, src/backend) |

## Research References

- [Architecture Report](../reports/researcher-260202-1222-nextjs-dotnet-architecture.md)
- [Ticketing Features Report](../reports/researcher-260202-1222-event-ticketing-features.md)
- [SePay Integration Report](../reports/researcher-260202-1222-sepay-integration.md)

## Phases

| # | Phase | Status | Effort | File |
|---|-------|--------|--------|------|
| 01 | Project Setup | pending | 4h | [phase-01-project-setup.md](phase-01-project-setup.md) |
| 02 | Database Schema | pending | 6h | [phase-02-database-schema.md](phase-02-database-schema.md) |
| 03 | Backend API Structure | pending | 8h | [phase-03-backend-api-structure.md](phase-03-backend-api-structure.md) |
| 04 | Authentication | pending | 8h | [phase-04-authentication.md](phase-04-authentication.md) |
| 05 | Event Management | pending | 10h | [phase-05-event-management.md](phase-05-event-management.md) |
| 06 | Ticket Purchasing | pending | 10h | [phase-06-ticket-purchasing.md](phase-06-ticket-purchasing.md) |
| 07 | SePay Payment | pending | 8h | [phase-07-sepay-payment.md](phase-07-sepay-payment.md) |
| 08 | QR Tickets & Check-in | pending | 6h | [phase-08-qr-tickets-checkin.md](phase-08-qr-tickets-checkin.md) |
| 09 | Analytics Dashboard | pending | 8h | [phase-09-analytics-dashboard.md](phase-09-analytics-dashboard.md) |
| 10 | Admin Panel | pending | 6h | [phase-10-admin-panel.md](phase-10-admin-panel.md) |
| 11 | Testing & Deployment | pending | 6h | [phase-11-testing-deployment.md](phase-11-testing-deployment.md) |

## Key Dependencies

- Phase 02 blocks 03-11 (database required for all features)
- Phase 04 blocks 05-10 (auth required for protected routes)
- Phase 05 blocks 06-08 (events required before tickets)
- Phase 06 blocks 07 (cart required before payment)

## Critical Decisions

1. **REST over GraphQL** - Simpler for flat event/ticket models, better caching
2. **Optimistic locking + cart reservations** - Handles race conditions for inventory
3. **NSwag type generation** - C# as source of truth for TypeScript types
4. **Manual refunds** - SePay lacks refund API; handle via bank portal
