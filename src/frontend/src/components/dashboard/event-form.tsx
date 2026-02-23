"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Plus, Trash2, Calendar } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { apiClient } from "@/api/generated/client";
import type { CreateEventRequest } from "@/api/generated/api-schema";

interface TicketTierForm {
  name: string;
  description: string;
  price: string;
  quantityTotal: string;
  saleStartDateTime: string;
  saleEndDateTime: string;
}

interface EventFormProps {
  initialData?: Partial<CreateEventRequest>;
  eventId?: string;
}

export function EventForm({ initialData, eventId }: EventFormProps) {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [formData, setFormData] = useState<CreateEventRequest>({
    title: initialData?.title || "",
    description: initialData?.description || "",
    venueName: initialData?.venueName || "",
    venueAddress: initialData?.venueAddress || "",
    venueCity: initialData?.venueCity || "",
    startDateTime: initialData?.startDateTime || "",
    endDateTime: initialData?.endDateTime || "",
    imageUrl: initialData?.imageUrl || "",
    totalCapacity: initialData?.totalCapacity || 0,
  });

  const [ticketTiers, setTicketTiers] = useState<TicketTierForm[]>([]);
  const [showTierForm, setShowTierForm] = useState(false);
  const [currentTier, setCurrentTier] = useState<TicketTierForm>({
    name: "",
    description: "",
    price: "",
    quantityTotal: "",
    saleStartDateTime: "",
    saleEndDateTime: "",
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    // Validation
    if (!formData.title || !formData.venueName || !formData.venueCity) {
      setError("Please fill in all required fields");
      return;
    }

    if (new Date(formData.endDateTime) <= new Date(formData.startDateTime)) {
      setError("End date must be after start date");
      return;
    }

    if (formData.totalCapacity <= 0) {
      setError("Total capacity must be greater than 0");
      return;
    }

    try {
      setLoading(true);

      if (eventId) {
        await apiClient.updateEvent(eventId, formData);
      } else {
        const createdEvent = await apiClient.createEvent(formData);

        // Create ticket tiers if any
        if (ticketTiers.length > 0) {
          for (const tier of ticketTiers) {
            await apiClient.createTicketTier(createdEvent.id, {
              name: tier.name,
              description: tier.description || undefined,
              price: parseFloat(tier.price),
              quantityTotal: parseInt(tier.quantityTotal),
              saleStartDateTime: tier.saleStartDateTime,
              saleEndDateTime: tier.saleEndDateTime,
            });
          }
        }

        router.push(`/dashboard/events/${createdEvent.id}/tiers`);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to save event");
    } finally {
      setLoading(false);
    }
  };

  const addTicketTier = () => {
    if (!currentTier.name || !currentTier.price || !currentTier.quantityTotal) {
      setError("Please fill in all required tier fields");
      return;
    }

    if (new Date(currentTier.saleEndDateTime) <= new Date(currentTier.saleStartDateTime)) {
      setError("Tier sale end date must be after start date");
      return;
    }

    setTicketTiers([...ticketTiers, currentTier]);
    setCurrentTier({
      name: "",
      description: "",
      price: "",
      quantityTotal: "",
      saleStartDateTime: "",
      saleEndDateTime: "",
    });
    setShowTierForm(false);
  };

  const removeTicketTier = (index: number) => {
    setTicketTiers(ticketTiers.filter((_, i) => i !== index));
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-red-800 dark:border-red-800 dark:bg-red-950 dark:text-red-200">
          {error}
        </div>
      )}

      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle>Basic Information</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <Label htmlFor="title">Event Title *</Label>
            <Input
              id="title"
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              placeholder="Enter event title"
              required
            />
          </div>

          <div>
            <Label htmlFor="description">Description</Label>
            <textarea
              id="description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              placeholder="Describe your event"
              rows={4}
              className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
            />
          </div>
        </CardContent>
      </Card>

      {/* Venue Information */}
      <Card>
        <CardHeader>
          <CardTitle>Venue Information</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <Label htmlFor="venueName">Venue Name *</Label>
            <Input
              id="venueName"
              value={formData.venueName}
              onChange={(e) => setFormData({ ...formData, venueName: e.target.value })}
              placeholder="e.g., Grand Convention Center"
              required
            />
          </div>

          <div>
            <Label htmlFor="venueAddress">Address *</Label>
            <Input
              id="venueAddress"
              value={formData.venueAddress}
              onChange={(e) => setFormData({ ...formData, venueAddress: e.target.value })}
              placeholder="Street address"
              required
            />
          </div>

          <div>
            <Label htmlFor="venueCity">City *</Label>
            <Input
              id="venueCity"
              value={formData.venueCity}
              onChange={(e) => setFormData({ ...formData, venueCity: e.target.value })}
              placeholder="e.g., Ho Chi Minh City"
              required
            />
          </div>
        </CardContent>
      </Card>

      {/* Date and Time */}
      <Card>
        <CardHeader>
          <CardTitle>Date and Time</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <Label htmlFor="startDateTime">Start Date & Time *</Label>
              <Input
                id="startDateTime"
                type="datetime-local"
                value={formData.startDateTime}
                onChange={(e) => setFormData({ ...formData, startDateTime: e.target.value })}
                required
              />
            </div>

            <div>
              <Label htmlFor="endDateTime">End Date & Time *</Label>
              <Input
                id="endDateTime"
                type="datetime-local"
                value={formData.endDateTime}
                onChange={(e) => setFormData({ ...formData, endDateTime: e.target.value })}
                required
              />
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Capacity */}
      <Card>
        <CardHeader>
          <CardTitle>Capacity</CardTitle>
        </CardHeader>
        <CardContent>
          <div>
            <Label htmlFor="totalCapacity">Total Capacity *</Label>
            <Input
              id="totalCapacity"
              type="number"
              min="1"
              value={formData.totalCapacity || ""}
              onChange={(e) => setFormData({ ...formData, totalCapacity: parseInt(e.target.value) || 0 })}
              placeholder="Total number of attendees"
              required
            />
          </div>
        </CardContent>
      </Card>

      {/* Image URL */}
      <Card>
        <CardHeader>
          <CardTitle>Event Image</CardTitle>
        </CardHeader>
        <CardContent>
          <div>
            <Label htmlFor="imageUrl">Image URL</Label>
            <Input
              id="imageUrl"
              type="url"
              value={formData.imageUrl || ""}
              onChange={(e) => setFormData({ ...formData, imageUrl: e.target.value })}
              placeholder="https://example.com/image.jpg"
            />
            {formData.imageUrl && (
              <div className="mt-4 aspect-video w-full overflow-hidden rounded-lg bg-muted">
                <img
                  src={formData.imageUrl}
                  alt="Preview"
                  className="h-full w-full object-cover"
                />
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Ticket Tiers */}
      {!eventId && (
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>Ticket Tiers</CardTitle>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => setShowTierForm(!showTierForm)}
                className="gap-2"
              >
                <Plus className="h-4 w-4" />
                Add Tier
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-4">
            {ticketTiers.map((tier, index) => (
              <div key={index} className="flex items-center justify-between rounded-lg border p-4">
                <div>
                  <p className="font-semibold">{tier.name}</p>
                  <p className="text-sm text-muted-foreground">{tier.description}</p>
                  <p className="text-sm">
                    ${tier.price} × {tier.quantityTotal} tickets
                  </p>
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={() => removeTicketTier(index)}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ))}

            {showTierForm && (
              <div className="space-y-4 rounded-lg border p-4">
                <div>
                  <Label htmlFor="tierName">Tier Name *</Label>
                  <Input
                    id="tierName"
                    value={currentTier.name}
                    onChange={(e) => setCurrentTier({ ...currentTier, name: e.target.value })}
                    placeholder="e.g., General Admission, VIP"
                  />
                </div>

                <div>
                  <Label htmlFor="tierDescription">Description</Label>
                  <Input
                    id="tierDescription"
                    value={currentTier.description}
                    onChange={(e) => setCurrentTier({ ...currentTier, description: e.target.value })}
                    placeholder="What's included in this tier?"
                  />
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <Label htmlFor="tierPrice">Price (USD) *</Label>
                    <Input
                      id="tierPrice"
                      type="number"
                      min="0"
                      step="0.01"
                      value={currentTier.price}
                      onChange={(e) => setCurrentTier({ ...currentTier, price: e.target.value })}
                      placeholder="0.00"
                    />
                  </div>

                  <div>
                    <Label htmlFor="tierQuantity">Quantity *</Label>
                    <Input
                      id="tierQuantity"
                      type="number"
                      min="1"
                      value={currentTier.quantityTotal}
                      onChange={(e) => setCurrentTier({ ...currentTier, quantityTotal: e.target.value })}
                      placeholder="Number of tickets"
                    />
                  </div>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <Label htmlFor="saleStart">Sale Start *</Label>
                    <Input
                      id="saleStart"
                      type="datetime-local"
                      value={currentTier.saleStartDateTime}
                      onChange={(e) => setCurrentTier({ ...currentTier, saleStartDateTime: e.target.value })}
                    />
                  </div>

                  <div>
                    <Label htmlFor="saleEnd">Sale End *</Label>
                    <Input
                      id="saleEnd"
                      type="datetime-local"
                      value={currentTier.saleEndDateTime}
                      onChange={(e) => setCurrentTier({ ...currentTier, saleEndDateTime: e.target.value })}
                    />
                  </div>
                </div>

                <div className="flex gap-2">
                  <Button type="button" onClick={addTicketTier}>
                    Add Tier
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setShowTierForm(false)}
                  >
                    Cancel
                  </Button>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex justify-end gap-4">
        <Button
          type="button"
          variant="outline"
          onClick={() => router.back()}
          disabled={loading}
        >
          Cancel
        </Button>
        <Button type="submit" disabled={loading}>
          {loading ? "Saving..." : eventId ? "Update Event" : "Create Event"}
        </Button>
      </div>
    </form>
  );
}
