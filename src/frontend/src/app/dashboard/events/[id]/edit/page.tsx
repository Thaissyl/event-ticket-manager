"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { EventForm } from "@/components/dashboard/event-form";
import { apiClient } from "@/api/generated/client";

export default function EditEventPage() {
  const params = useParams();
  const router = useRouter();
  const eventId = params.id as string;

  const [eventData, setEventData] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadEvent();
  }, [eventId]);

  const loadEvent = async () => {
    try {
      setLoading(true);
      const data = await apiClient.getEvent(eventId);
      setEventData({
        ...data,
        startDateTime: new Date(data.startDateTime).toISOString().slice(0, 16),
        endDateTime: new Date(data.endDateTime).toISOString().slice(0, 16),
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load event");
      if (err instanceof Error && err.message.includes("404")) {
        router.push("/dashboard/events");
      }
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl font-bold tracking-tight">Edit Event</h1>
        <div className="h-64 animate-pulse rounded-xl bg-muted" />
      </div>
    );
  }

  if (error || !eventData) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl font-bold tracking-tight">Edit Event</h1>
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-800 dark:bg-red-950 dark:text-red-200">
          {error || "Event not found"}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Edit Event</h1>
        <p className="text-muted-foreground">
          Update your event details
        </p>
      </div>

      <EventForm initialData={eventData} eventId={eventId} />
    </div>
  );
}
