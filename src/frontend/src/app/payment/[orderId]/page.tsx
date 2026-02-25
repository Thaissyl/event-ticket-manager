"use client";

import { useState, useEffect, Suspense } from "react";
import { useRouter, useParams } from "next/navigation";
import { ArrowLeft, Clock, CheckCircle2, XCircle, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { VietQrDisplay } from "@/components/payment/vietqr-display";
import { PaymentTimer } from "@/components/payment/payment-timer";
import { usePaymentStatus } from "@/hooks/use-payment-status";

type PaymentState = "pending" | "completed" | "failed" | "expired";

function PaymentContent() {
  const router = useRouter();
  const params = useParams();
  const orderId = params.orderId as string;

  const [qrCodeUrl, setQrCodeUrl] = useState<string>("");
  const [paymentCode, setPaymentCode] = useState<string>("");
  const [amount, setAmount] = useState<number>(0);
  const [expiresAt, setExpiresAt] = useState<Date>(new Date());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const { status, polling } = usePaymentStatus(orderId);

  useEffect(() => {
    const initializePayment = async () => {
      try {
        setLoading(true);
        const response = await fetch(`/api/payments/${orderId}/create`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          credentials: "include",
        });

        if (!response.ok) {
          const err = await response.json();
          throw new Error(err.message || "Failed to initialize payment");
        }

        const data = await response.json();
        setQrCodeUrl(data.qrCodeUrl);
        setPaymentCode(data.paymentCode);
        setAmount(data.amount);
        setExpiresAt(new Date(data.expiresAt));
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load payment");
      } finally {
        setLoading(false);
      }
    };

    initializePayment();
  }, [orderId]);

  useEffect(() => {
    if (status === "completed") {
      const timer = setTimeout(() => {
        router.push(`/orders/${orderId}`);
      }, 3000);
      return () => clearTimeout(timer);
    }
  }, [status, orderId, router]);

  const getPaymentState = (): PaymentState => {
    if (status === "completed") return "completed";
    if (status === "failed") return "failed";
    if (new Date() > expiresAt) return "expired";
    return "pending";
  };

  const paymentState = getPaymentState();

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="mx-auto max-w-2xl flex items-center justify-center min-h-[400px]">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Button variant="ghost" className="mb-4 gap-2" onClick={() => router.back()}>
          <ArrowLeft className="h-4 w-4" />
          Back
        </Button>
        <Card>
          <CardContent className="flex min-h-[400px] flex-col items-center justify-center">
            <XCircle className="h-16 w-16 text-destructive" />
            <h3 className="mt-4 text-lg font-semibold">Payment Error</h3>
            <p className="text-muted-foreground">{error}</p>
            <Button className="mt-4" onClick={() => router.back()}>
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <Button variant="ghost" className="mb-4 gap-2" onClick={() => router.back()}>
        <ArrowLeft className="h-4 w-4" />
        Back
      </Button>

      <h1 className="text-3xl font-bold tracking-tight mb-8">Complete Payment</h1>

      <div className="grid gap-6 lg:grid-cols-2 max-w-4xl mx-auto">
        {/* VietQR Code */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              Scan to Pay
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <VietQrDisplay qrCodeUrl={qrCodeUrl} paymentCode={paymentCode} amount={amount} />
            <PaymentTimer expiresAt={expiresAt} onExpired={() => {}} />
          </CardContent>
        </Card>

        {/* Payment Status */}
        <Card>
          <CardHeader>
            <CardTitle>Payment Status</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {paymentState === "pending" && (
              <div className="flex flex-col items-center justify-center py-8">
                {polling ? (
                  <>
                    <Loader2 className="h-12 w-12 animate-spin text-primary mb-4" />
                    <p className="text-center text-muted-foreground">
                      Waiting for payment confirmation...
                    </p>
                    <p className="text-sm text-muted-foreground mt-2">
                      We'll automatically update once payment is received
                    </p>
                  </>
                ) : (
                  <>
                    <Clock className="h-12 w-12 text-yellow-500 mb-4" />
                    <p className="text-center text-muted-foreground">
                      Payment status will update automatically
                    </p>
                  </>
                )}
              </div>
            )}

            {paymentState === "completed" && (
              <div className="flex flex-col items-center justify-center py-8">
                <CheckCircle2 className="h-12 w-12 text-green-500 mb-4" />
                <p className="text-center font-semibold">Payment Completed!</p>
                <p className="text-sm text-muted-foreground mt-2">
                  Redirecting to your order...
                </p>
              </div>
            )}

            {paymentState === "failed" && (
              <div className="flex flex-col items-center justify-center py-8">
                <XCircle className="h-12 w-12 text-destructive mb-4" />
                <p className="text-center font-semibold">Payment Failed</p>
                <p className="text-sm text-muted-foreground mt-2">
                  Please try again or contact support
                </p>
                <Button className="mt-4" onClick={() => router.back()}>
                  Try Again
                </Button>
              </div>
            )}

            {paymentState === "expired" && (
              <div className="flex flex-col items-center justify-center py-8">
                <XCircle className="h-12 w-12 text-destructive mb-4" />
                <p className="text-center font-semibold">Payment Expired</p>
                <p className="text-sm text-muted-foreground mt-2">
                  The payment time limit has been reached
                </p>
                <Button className="mt-4" onClick={() => router.back()}>
                  Back to Order
                </Button>
              </div>
            )}

            <div className="border-t pt-4 space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Order ID:</span>
                <span className="font-mono">{orderId.slice(0, 8)}...</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Payment Code:</span>
                <span className="font-mono">{paymentCode}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Amount:</span>
                <span className="font-semibold">${amount.toFixed(2)}</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <div className="max-w-4xl mx-auto mt-6">
        <Card>
          <CardContent className="p-6">
            <h3 className="font-semibold mb-2">Payment Instructions</h3>
            <ol className="list-decimal list-inside space-y-1 text-sm text-muted-foreground">
              <li>Open your banking app (Vietcombank, MB, etc.)</li>
              <li>Select "QR Code Payment" or "Scan to Pay"</li>
              <li>Scan the QR code displayed above</li>
              <li>Confirm the payment amount matches</li>
              <li>Complete the transaction</li>
              <li>Wait for automatic confirmation (usually within 1-2 minutes)</li>
            </ol>
            <p className="mt-4 text-sm text-muted-foreground">
              <strong>Note:</strong> Make sure the payment reference includes the code: <span className="font-mono">{paymentCode}</span>
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function PaymentPage() {
  return (
    <Suspense fallback={
      <div className="container mx-auto px-4 py-8">
        <div className="mx-auto max-w-2xl flex items-center justify-center min-h-[400px]">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      </div>
    }>
      <PaymentContent />
    </Suspense>
  );
}
