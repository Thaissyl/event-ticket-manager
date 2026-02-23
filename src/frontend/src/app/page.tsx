import Link from "next/link";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function Home() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-zinc-50 to-zinc-100 dark:from-zinc-900 dark:to-black">
      <Card className="w-full max-w-md mx-4">
        <CardHeader className="text-center">
          <CardTitle className="text-3xl font-bold">Event Ticket Manager</CardTitle>
          <CardDescription>
            Create, sell, and manage event tickets with ease
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-2">
            <Link href="/events">
              <Button size="lg" className="w-full" variant="outline">
                Browse Events
              </Button>
            </Link>
            <Link href="/dashboard">
              <Button size="lg" className="w-full">
                Organizer Dashboard
              </Button>
            </Link>
          </div>
          <p className="text-center text-sm text-muted-foreground">
            Powered by Next.js + ASP.NET Core
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
