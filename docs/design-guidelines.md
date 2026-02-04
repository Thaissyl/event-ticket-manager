# Design Guidelines

**Last Updated:** 2026-02-04
**Version:** 1.0.0

## Overview

This document defines UI/UX design principles, component usage, accessibility standards, and visual guidelines for the Event Ticket Manager frontend.

## Design Principles

### 1. Clarity Over Complexity
- Use clear, descriptive labels and headings
- Avoid jargon in user-facing text
- Provide contextual help where needed
- Show only relevant information per screen

### 2. Consistency
- Maintain uniform spacing, colors, and typography
- Use shadcn/ui components for consistent look
- Apply same interaction patterns throughout
- Standardize button placement (primary actions on right)

### 3. Accessibility First
- Target WCAG 2.1 Level AA compliance
- Ensure keyboard navigation works everywhere
- Provide alternative text for images
- Use sufficient color contrast (4.5:1 for text)

### 4. Mobile-First Responsive
- Design for mobile, scale up to desktop
- Use Tailwind breakpoints (sm, md, lg, xl, 2xl)
- Touch targets minimum 44x44px
- Avoid horizontal scrolling

### 5. Performance
- Minimize layout shifts (CLS)
- Lazy load images and heavy components
- Optimize assets (WebP images, SVG icons)
- Reduce JavaScript bundle size

## Color System

### Brand Colors (TBD - Placeholder)

```typescript
// tailwind.config.ts
const colors = {
  primary: {
    50: '#eff6ff',
    100: '#dbeafe',
    500: '#3b82f6',  // Main brand blue
    600: '#2563eb',
    900: '#1e3a8a',
  },
  accent: {
    500: '#f59e0b',  // Amber for highlights
  },
}
```

### Semantic Colors (shadcn/ui defaults)

| Usage | Variable | Light Mode | Dark Mode |
|-------|----------|------------|-----------|
| Background | `--background` | #ffffff | #0a0a0a |
| Foreground | `--foreground` | #0a0a0a | #fafafa |
| Primary | `--primary` | #18181b | #fafafa |
| Secondary | `--secondary` | #f4f4f5 | #27272a |
| Muted | `--muted` | #f4f4f5 | #27272a |
| Accent | `--accent` | #f4f4f5 | #27272a |
| Destructive | `--destructive` | #ef4444 | #7f1d1d |
| Border | `--border` | #e4e4e7 | #27272a |

**Usage**:
```typescript
<div className="bg-background text-foreground">
  <Button variant="destructive">Delete</Button>
</div>
```

## Typography

### Font Stack

**Primary**: Geist Sans (default)
**Monospace**: Geist Mono (code, IDs)

```typescript
// src/app/layout.tsx
import { Geist, Geist_Mono } from "next/font/google";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});
```

### Type Scale

| Element | Class | Size | Line Height |
|---------|-------|------|-------------|
| H1 | `text-4xl font-bold` | 36px | 40px |
| H2 | `text-3xl font-bold` | 30px | 36px |
| H3 | `text-2xl font-semibold` | 24px | 32px |
| H4 | `text-xl font-semibold` | 20px | 28px |
| Body | `text-base` | 16px | 24px |
| Small | `text-sm` | 14px | 20px |
| Tiny | `text-xs` | 12px | 16px |

### Text Styles

**Headings**:
```typescript
<h1 className="text-4xl font-bold tracking-tight">Page Title</h1>
<h2 className="text-3xl font-bold">Section Title</h2>
<h3 className="text-2xl font-semibold">Subsection</h3>
```

**Paragraphs**:
```typescript
<p className="text-base text-muted-foreground">
  Body text with muted color for hierarchy.
</p>
```

**Labels**:
```typescript
<label className="text-sm font-medium leading-none">
  Email Address
</label>
```

## Spacing System

### Scale (Tailwind defaults)

| Token | Value | Usage |
|-------|-------|-------|
| 1 | 4px | Icon padding, tight spacing |
| 2 | 8px | Component internal padding |
| 4 | 16px | Standard component spacing |
| 6 | 24px | Section spacing |
| 8 | 32px | Large section gaps |
| 12 | 48px | Page section dividers |

**Examples**:
```typescript
<div className="p-4">         {/* 16px padding all sides */}
<div className="mb-6">        {/* 24px margin bottom */}
<div className="space-y-4">   {/* 16px vertical gap between children */}
```

## Component Usage (shadcn/ui)

### Button

**Variants**:
- `default`: Primary action (filled)
- `destructive`: Delete, remove actions (red)
- `outline`: Secondary action (bordered)
- `secondary`: Tertiary action (subtle)
- `ghost`: Minimal emphasis (text-like)
- `link`: Hyperlink style

**Sizes**:
- `default`: Standard (h-10)
- `sm`: Small (h-9)
- `lg`: Large (h-11)
- `icon`: Square icon button

**Examples**:
```typescript
// Primary action
<Button>Create Event</Button>

// Destructive action with confirmation
<Button variant="destructive" onClick={handleDelete}>
  Delete Event
</Button>

// Secondary action
<Button variant="outline">Cancel</Button>

// Icon button
<Button variant="ghost" size="icon">
  <X className="h-4 w-4" />
</Button>
```

**Best Practices**:
- Use `default` for main CTA (Call to Action)
- Limit one primary button per screen section
- Place primary button on right in button groups
- Always provide accessible labels for icon buttons

### Card

**Components**:
- `Card`: Container
- `CardHeader`: Top section (optional)
- `CardTitle`: Heading within header
- `CardDescription`: Subtext within header
- `CardContent`: Main content area
- `CardFooter`: Bottom section (optional)

**Examples**:
```typescript
// Event card
<Card>
  <CardHeader>
    <CardTitle>Concert Name</CardTitle>
    <CardDescription>May 15, 2026 • 7:00 PM</CardDescription>
  </CardHeader>
  <CardContent>
    <p>Event details...</p>
  </CardContent>
  <CardFooter className="flex justify-between">
    <span>$25 - $100</span>
    <Button>View Tickets</Button>
  </CardFooter>
</Card>
```

**Best Practices**:
- Use cards for grouped information (events, orders)
- Ensure cards have clear hierarchy (title > description > content)
- Keep card content scannable (bullet points, icons)
- Add hover effects for clickable cards

### Input & Label

**Examples**:
```typescript
// Form field
<div className="grid gap-2">
  <Label htmlFor="email">Email</Label>
  <Input
    id="email"
    type="email"
    placeholder="you@example.com"
    required
  />
</div>

// With error state
<div className="grid gap-2">
  <Label htmlFor="password">Password</Label>
  <Input
    id="password"
    type="password"
    className={cn(error && "border-destructive")}
  />
  {error && (
    <p className="text-sm text-destructive">{error}</p>
  )}
</div>
```

**Best Practices**:
- Always pair `<Label>` with `<Input>` using `htmlFor` and `id`
- Show validation errors below input
- Use `placeholder` for examples, not labels
- Disable autofill for sensitive fields if needed

## Layout Patterns

### Container

**Max Width Container**:
```typescript
<div className="container mx-auto px-4 max-w-6xl">
  {/* Content */}
</div>
```

**Breakpoints**:
- `max-w-sm`: 384px (mobile forms)
- `max-w-md`: 448px (login, register)
- `max-w-2xl`: 672px (event details)
- `max-w-4xl`: 896px (dashboard)
- `max-w-6xl`: 1152px (main content)
- `max-w-7xl`: 1280px (wide layouts)

### Grid Layouts

**Responsive Grid**:
```typescript
// Event card grid
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
  {events.map(event => (
    <EventCard key={event.id} event={event} />
  ))}
</div>

// Form grid
<div className="grid grid-cols-1 md:grid-cols-2 gap-4">
  <div>
    <Label>First Name</Label>
    <Input />
  </div>
  <div>
    <Label>Last Name</Label>
    <Input />
  </div>
</div>
```

### Flexbox Layouts

**Horizontal Alignment**:
```typescript
// Header with logo and nav
<header className="flex items-center justify-between p-4">
  <Logo />
  <nav className="flex gap-4">
    <a href="/events">Events</a>
    <a href="/dashboard">Dashboard</a>
  </nav>
</header>

// Button group
<div className="flex gap-2 justify-end">
  <Button variant="outline">Cancel</Button>
  <Button>Save</Button>
</div>
```

## Page Templates

### Landing Page

**Structure**:
```typescript
export default function Home() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-zinc-50 to-zinc-100 dark:from-zinc-900 dark:to-black">
      <Card className="w-full max-w-md mx-4">
        <CardHeader className="text-center">
          <CardTitle className="text-3xl font-bold">
            Event Ticket Manager
          </CardTitle>
          <CardDescription>
            Create, sell, and manage event tickets with ease
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* CTA buttons */}
        </CardContent>
      </Card>
    </div>
  );
}
```

### Dashboard Layout

**Structure**:
```typescript
export default function DashboardLayout({ children }) {
  return (
    <div className="flex min-h-screen">
      {/* Sidebar */}
      <aside className="w-64 border-r bg-muted/40">
        <nav className="p-4 space-y-2">
          {/* Navigation links */}
        </nav>
      </aside>

      {/* Main content */}
      <main className="flex-1">
        <header className="border-b p-4">
          <h1 className="text-2xl font-bold">Dashboard</h1>
        </header>
        <div className="p-6">
          {children}
        </div>
      </main>
    </div>
  );
}
```

### Form Page

**Structure**:
```typescript
export default function CreateEventPage() {
  return (
    <div className="container mx-auto max-w-2xl py-8">
      <h1 className="text-3xl font-bold mb-6">Create Event</h1>

      <Card>
        <CardHeader>
          <CardTitle>Event Details</CardTitle>
          <CardDescription>
            Provide information about your event
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form className="space-y-4">
            {/* Form fields */}
          </form>
        </CardContent>
        <CardFooter className="flex justify-between">
          <Button variant="outline">Cancel</Button>
          <Button type="submit">Create Event</Button>
        </CardFooter>
      </Card>
    </div>
  );
}
```

## Icons

### Library

**Lucide React**: `lucide-react` (v0.563.0)

**Usage**:
```typescript
import { Calendar, MapPin, Users, Ticket } from 'lucide-react';

<Button>
  <Calendar className="mr-2 h-4 w-4" />
  Schedule Event
</Button>
```

**Common Icons**:
- `Calendar`: Dates, scheduling
- `MapPin`: Location, venue
- `Users`: Attendees, organizers
- `Ticket`: Tickets
- `CreditCard`: Payments
- `QrCode`: QR codes, check-in
- `TrendingUp`: Analytics, growth
- `Settings`: Configuration
- `Search`: Search functionality
- `X`: Close, remove

**Best Practices**:
- Use consistent icon size within sections (h-4 w-4 for buttons, h-6 w-6 for larger contexts)
- Add `aria-label` for icon-only buttons
- Avoid using icons without text labels unless widely recognized

## Responsive Breakpoints

### Tailwind Breakpoints

| Breakpoint | Min Width | Typical Device |
|------------|-----------|----------------|
| `sm` | 640px | Large phones |
| `md` | 768px | Tablets |
| `lg` | 1024px | Laptops |
| `xl` | 1280px | Desktops |
| `2xl` | 1536px | Large desktops |

**Mobile-First Approach**:
```typescript
// Base styles apply to mobile, override for larger screens
<div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
  {/* 1 column on mobile, 2 on tablet, 3 on desktop */}
</div>

<Button className="w-full md:w-auto">
  {/* Full width on mobile, auto on tablet+ */}
</Button>
```

## Accessibility (a11y)

### Keyboard Navigation

**Focus Styles**:
```typescript
// Ensure visible focus indicators
<Button className="focus:ring-2 focus:ring-primary focus:ring-offset-2">
  Click Me
</Button>
```

**Tab Order**:
- Use semantic HTML to maintain logical tab order
- Avoid `tabIndex > 0` (disrupts natural flow)
- Use `tabIndex={-1}` for programmatic focus only

### Screen Readers

**ARIA Labels**:
```typescript
// Icon button
<Button variant="ghost" size="icon" aria-label="Close modal">
  <X className="h-4 w-4" />
</Button>

// Search input
<Input
  type="search"
  placeholder="Search events"
  aria-label="Search events"
/>
```

**Semantic HTML**:
```typescript
// Use semantic elements over divs
<nav> {/* Not <div> */}
<main> {/* Not <div id="main"> */}
<article>
<section>
```

### Color Contrast

**Minimum Ratios** (WCAG 2.1 Level AA):
- Normal text (< 18pt): 4.5:1
- Large text (≥ 18pt or ≥ 14pt bold): 3:1
- UI components: 3:1

**Check Tools**:
- Chrome DevTools (Lighthouse)
- WebAIM Contrast Checker
- axe DevTools browser extension

### Forms

**Labels**:
```typescript
// Always associate labels with inputs
<Label htmlFor="email">Email Address</Label>
<Input id="email" type="email" />
```

**Error Messages**:
```typescript
<div>
  <Label htmlFor="password">Password</Label>
  <Input
    id="password"
    type="password"
    aria-invalid={!!error}
    aria-describedby={error ? "password-error" : undefined}
  />
  {error && (
    <p id="password-error" className="text-sm text-destructive">
      {error}
    </p>
  )}
</div>
```

## Animation and Transitions

### Subtle Animations

**Hover Effects**:
```typescript
<Button className="transition-colors hover:bg-primary/90">
  Hover Me
</Button>

<Card className="transition-shadow hover:shadow-lg">
  {/* Card content */}
</Card>
```

**Loading States**:
```typescript
// Spinner (using lucide-react)
import { Loader2 } from 'lucide-react';

<Button disabled={isLoading}>
  {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
  {isLoading ? 'Saving...' : 'Save'}
</Button>
```

**Page Transitions** (future with Framer Motion):
```typescript
// Fade in on mount
<motion.div
  initial={{ opacity: 0 }}
  animate={{ opacity: 1 }}
  transition={{ duration: 0.3 }}
>
  {/* Content */}
</motion.div>
```

**Best Practices**:
- Keep animations under 300ms for snappiness
- Respect `prefers-reduced-motion` media query
- Avoid animations for critical content (e.g., error messages)

## Dark Mode

### Automatic Dark Mode

shadcn/ui supports dark mode via `next-themes`.

**Implementation** (future):
```typescript
// app/layout.tsx
import { ThemeProvider } from 'next-themes';

export default function RootLayout({ children }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body>
        <ThemeProvider attribute="class" defaultTheme="system">
          {children}
        </ThemeProvider>
      </body>
    </html>
  );
}
```

**Usage**:
```typescript
import { useTheme } from 'next-themes';

export function ThemeToggle() {
  const { theme, setTheme } = useTheme();

  return (
    <Button
      variant="ghost"
      onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
    >
      Toggle Theme
    </Button>
  );
}
```

**Styling**:
```typescript
// Automatic dark mode classes
<div className="bg-white dark:bg-zinc-900 text-black dark:text-white">
  {/* Content */}
</div>
```

## Loading States

### Skeleton Loaders

```typescript
// Placeholder while data loads
export function EventCardSkeleton() {
  return (
    <Card>
      <CardHeader>
        <div className="h-6 w-3/4 bg-muted rounded animate-pulse" />
        <div className="h-4 w-1/2 bg-muted rounded animate-pulse mt-2" />
      </CardHeader>
      <CardContent>
        <div className="h-4 w-full bg-muted rounded animate-pulse" />
        <div className="h-4 w-5/6 bg-muted rounded animate-pulse mt-2" />
      </CardContent>
    </Card>
  );
}
```

### Spinner

```typescript
import { Loader2 } from 'lucide-react';

export function Spinner({ className }: { className?: string }) {
  return <Loader2 className={cn("animate-spin", className)} />;
}
```

## Error States

### Error Messages

```typescript
// Inline error
<div className="rounded-lg bg-destructive/10 border border-destructive p-4">
  <p className="text-sm text-destructive font-medium">
    Failed to load events. Please try again.
  </p>
</div>

// Alert component (future)
<Alert variant="destructive">
  <AlertCircle className="h-4 w-4" />
  <AlertTitle>Error</AlertTitle>
  <AlertDescription>
    Your session has expired. Please log in again.
  </AlertDescription>
</Alert>
```

## Forms

### Validation

**Client-Side**:
```typescript
const [errors, setErrors] = useState<Record<string, string>>({});

const validate = (data: FormData) => {
  const errors: Record<string, string> = {};

  if (!data.email) {
    errors.email = 'Email is required';
  } else if (!/\S+@\S+\.\S+/.test(data.email)) {
    errors.email = 'Email is invalid';
  }

  return errors;
};

const handleSubmit = (e: FormEvent) => {
  e.preventDefault();
  const validationErrors = validate(formData);

  if (Object.keys(validationErrors).length > 0) {
    setErrors(validationErrors);
    return;
  }

  // Submit form
};
```

**Server-Side**:
Display server errors returned from API.

```typescript
{apiError && (
  <div className="rounded-lg bg-destructive/10 border border-destructive p-4 mb-4">
    <p className="text-sm text-destructive">{apiError}</p>
  </div>
)}
```

## Modals/Dialogs (Future)

**Usage**:
```typescript
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';

<Dialog open={isOpen} onOpenChange={setIsOpen}>
  <DialogContent>
    <DialogHeader>
      <DialogTitle>Confirm Deletion</DialogTitle>
    </DialogHeader>
    <p>Are you sure you want to delete this event?</p>
    <div className="flex justify-end gap-2 mt-4">
      <Button variant="outline" onClick={() => setIsOpen(false)}>
        Cancel
      </Button>
      <Button variant="destructive" onClick={handleDelete}>
        Delete
      </Button>
    </div>
  </DialogContent>
</Dialog>
```

## References

- [shadcn/ui Documentation](https://ui.shadcn.com/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Radix UI Primitives](https://www.radix-ui.com/primitives)
- [Lucide Icons](https://lucide.dev/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Code Standards](./code-standards.md)
