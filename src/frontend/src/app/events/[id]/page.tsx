"use client";

import { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { Calendar, MapPin, Users, ArrowLeft, Share2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiClient } from "@/api/generated/client";
import type { EventResponse, TicketTierResponse } from "@/api/generated/api-schema";

export default function EventDetailsPage() {
  const params = useParams();
  const eventId = params.id as string;

  const [event, setEvent] = useState<EventResponse | null>(null);
  const [tiers, setTiers] = useState<TicketTierResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (eventId) {
      loadEventDetails();
    }
  }, [eventId]);

  const loadEventDetails = async () => {
    try {
      setLoading(true);
      setError(null);

      const [eventData, tiersData] = await Promise.all([
        apiClient.getEvent(eventId),
        apiClient.getTicketTiers(eventId),
      ]);

      setEvent(eventData);
      setTiers(tiersData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load event details");
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat("en-US", {
      weekday: "long",
      month: "long",
      day: "numeric",
      year: "numeric",
      hour: "numeric",
      minute: "2-digit",
    }).format(date);
  };

  const formatPrice = (price: number) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(price);
  };

  const isSaleActive = (saleStart: string, saleEnd: string) => {
    const now = new Date();
    const start = new Date(saleStart);
    const end = new Date(saleEnd);
    return now >= start && now <= end;
  };

  const handleShare = async () => {
    if (navigator.share && event) {
      try {
        await navigator.share({
          title: event.title,
          text: event.description || `Check out ${event.title}`,
          url: window.location.href,
        });
      } catch (err) {
        // User canceled or sharing failed
        console.log("Share failed:", err);
      }
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-b from-zinc-50 to-zinc-100 dark:from-zinc-900 dark:to-black">
        <div className="container mx-auto px-4 py-8">
          <div className="mx-auto max-w-3xl animate-pulse space-y-6">
            <div className="h-8 w-48 rounded bg-muted" />
            <div className="aspect-video w-full rounded-xl bg-muted" />
            <div className="space-y-2">
              <div className="h-4 w-full rounded bg-muted" />
              <div className="h-4 w-3/4 rounded bg-muted" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error || !event) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-zinc-50 to-zinc-100 dark:from-zinc-900 dark:to-black">
        <Card className="mx-4 max-w-md">
          <CardHeader>
            <CardTitle>Event Not Found</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-muted-foreground mb-4">
              {error || "The event you're looking for doesn't exist."}
            </p>
            <Link href="/events">
              <Button>Browse Events</Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  const isPublished = event.status === "Published";
  const hasAvailableTiers = tiers.some((t) => t.quantityAvailable > 0);

  return (
    <div className="min-h-screen bg-gradient-to-b from-zinc-50 to-zinc-100 dark:from-zinc-900 dark:to-black">
      <div className="container mx-auto px-4 py-8">
        <Link href="/events">
          <Button variant="ghost" className="mb-4 gap-2">
            <ArrowLeft className="h-4 w-4" />
            Back to Events
          </Button>
        </Link>

        <div className="mx-auto max-w-3xl">
          {event.imageUrl && (
            <div className="mb-6 overflow-hidden rounded-xl">
              <img
                src={event.imageUrl}
                alt={event.title}
                className="h-auto w-full object-cover"
              />
            </div>
          )}

          <div className="mb-6">
            <div className="mb-4 flex items-start justify-between gap-4">
              <h1 className="text-3xl font-bold tracking-tight">{event.title}</h1>
              <Button variant="outline" size="icon" onClick={handleShare}>
                <Share2 className="h-4 w-4" />
              </Button>
            </div>

            <div className="flex flex-wrap gap-4 text-muted-foreground">
              <div className="flex items-center gap-2">
                <Calendar className="h-5 w-5" />
                <time dateTime={event.startDateTime}>
                  {formatDate(event.startDateTime)}
                </time>
              </div>
              <div className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                <span>
                  {event.venueName}, {event.venueCity}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <Users className="h-5 w-5" />
                <span>{event.totalCapacity} capacity</span>
              </div>
            </div>
          </div>

          {event.description && (
            <Card className="mb-6">
              <CardHeader>
                <CardTitle>About this event</CardTitle>
              </CardHeader>
              <CardContent>
                <p className="whitespace-pre-wrap">{event.description}</p>
              </CardContent>
            </Card>
          )}

          <Card>
            <CardHeader>
              <CardTitle>Ticket Tiers</CardTitle>
            </CardHeader>
            <CardContent>
              {tiers.length === 0 ? (
                <p className="text-muted-foreground">
                  No ticket tiers available yet.
                </p>
              ) : (
                <div className="space-y-4">
                  {tiers.map((tier) => {
                    const saleActive = isSaleActive(
                      tier.saleStartDateTime,
                      tier.saleEndDateTime
                    );
                    const soldOut = tier.quantityAvailable === 0;

                    return (
                      <div
                        key={tier.id}
                        className="flex items-center justify-between rounded-lg border p-4"
                      >
                        <div className="flex-1">
                          <div className="flex items-center gap-2">
                            <h3 className="font-semibold">{tier.name}</h3>
                            {!saleActive && (
                              <span className="rounded-full bg-muted px-2 py-0.5 text-xs">
                                Coming Soon
                              </span>
                            )}
                            {soldOut && saleActive && (
                              <span className="rounded-full bg-destructive/10 px-2 py-0.5 text-xs text-destructive">
                                Sold Out
                              </span>
                            )}
                          </div>
                          {tier.description && (
                            <p className="text-sm text-muted-foreground">
                              {tier.description}
                            </p>
                          )}
                          <p className="mt-1 text-sm text-muted-foreground">
                            {tier.quantityAvailable} / {tier.quantityTotal} available
                          </p>
                        </div>
                        <div className="text-right">
                          <p className="text-xl font-bold">
                            {formatPrice(tier.price)}
                          </p>
                          <Link href={`/checkout?event=${eventId}&tier=${tier.id}`}>
                            <Button
                              disabled={!isPublished || !saleActive || soldOut}
                              className="mt-2"
                            >
                              {soldOut ? "Sold Out" : "Buy Ticket"}
                            </Button>
                          </Link>
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </CardContent>
          </Card>

          <div className="mt-6 rounded-lg border p-4">
            <h3 className="font-semibold">Venue Information</h3>
            <p className="text-muted-foreground">{event.venueName}</p>
            <p className="text-sm text-muted-foreground">{event.venueAddress}</p>
            <p className="text-sm text-muted-foreground">{event.venueCity}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
