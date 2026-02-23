"use client";

import { useState, useEffect, Suspense } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { CheckCircle, Download, Home, QrCode } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

function OrderConfirmationContent({ orderId }: { orderId: string }) {
  const [order, setOrder] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (orderId) {
      loadOrder();
    }
  }, [orderId]);

  const loadOrder = async () => {
    try {
      setLoading(true);
      const response = await fetch(`/api/orders/${orderId}`, {
        credentials: "include",
      });

      if (!response.ok) {
        throw new Error("Order not found");
      }

      const data = await response.json();
      setOrder(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load order");
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="mx-auto max-w-2xl animate-pulse">
          <div className="h-8 w-48 rounded bg-muted mb-8" />
          <div className="h-64 rounded-xl bg-muted" />
        </div>
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="mx-auto max-w-md">
          <CardContent className="flex min-h-[400px] flex-col items-center justify-center">
            <p className="text-muted-foreground mb-4">
              {error || "Order not found"}
            </p>
            <Link href="/events">
              <Button>Browse Events</Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mx-auto max-w-2xl">
        {/* Success Message */}
        <div className="mb-8 text-center">
          <div className="mb-4 flex justify-center">
            <div className="rounded-full bg-green-100 p-4 dark:bg-green-900">
              <CheckCircle className="h-16 w-16 text-green-600 dark:text-green-400" />
            </div>
          </div>
          <h1 className="text-3xl font-bold tracking-tight mb-2">Order Confirmed!</h1>
          <p className="text-muted-foreground">
            Thank you for your purchase. Your order has been received.
          </p>
        </div>

        {/* Order Details */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Order Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <p className="text-sm text-muted-foreground">Order ID</p>
                <p className="font-mono font-semibold">{order.id.slice(0, 8)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Payment Code</p>
                <p className="font-mono font-semibold text-lg">{order.paymentCode}</p>
              </div>
            </div>

            <div>
              <p className="text-sm text-muted-foreground">Customer</p>
              <p className="font-semibold">{order.guestName}</p>
              <p className="text-sm text-muted-foreground">{order.guestEmail}</p>
            </div>

            <div>
              <p className="text-sm text-muted-foreground">Total Amount</p>
              <p className="text-2xl font-bold">${order.totalAmount.toFixed(2)}</p>
            </div>

            <div className="rounded-lg bg-yellow-50 p-4 dark:bg-yellow-900/20">
              <p className="text-sm font-semibold text-yellow-800 dark:text-yellow-200">
                Payment Pending
              </p>
              <p className="text-sm text-yellow-700 dark:text-yellow-300 mt-1">
                Please complete your payment using the code above. Check your email for detailed instructions.
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Tickets */}
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Your Tickets</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {order.tickets.map((ticket: any) => (
                <div key={ticket.id} className="flex items-center justify-between rounded-lg border p-4">
                  <div>
                    <p className="font-semibold">{ticket.tierName}</p>
                    <p className="text-sm text-muted-foreground">
                      {ticket.attendeeName} • {ticket.attendeeEmail}
                    </p>
                  </div>
                  <Button variant="outline" size="icon">
                    <QrCode className="h-5 w-5" />
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Actions */}
        <div className="flex gap-4">
          <Link href="/events" className="flex-1">
            <Button variant="outline" className="w-full gap-2">
              <Home className="h-4 w-4" />
              Browse Events
            </Button>
          </Link>
          <Button className="flex-1 gap-2">
            <Download className="h-4 w-4" />
            Download Tickets
          </Button>
        </div>
      </div>
    </div>
  );
}

export default function OrderConfirmationPage() {
  return (
    <Suspense fallback={
      <div className="container mx-auto px-4 py-8">
        <div className="mx-auto max-w-2xl animate-pulse">
          <div className="h-8 w-48 rounded bg-muted mb-8" />
          <div className="h-64 rounded-xl bg-muted" />
        </div>
      </div>
    }>
      <OrderDetailsWrapper />
    </Suspense>
  );
}

function OrderDetailsWrapper() {
  const params = useParams();
  const orderId = params.id as string;
  return <OrderConfirmationContent orderId={orderId} />;
}
