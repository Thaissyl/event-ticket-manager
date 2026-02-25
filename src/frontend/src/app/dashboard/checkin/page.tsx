"use client";

import { useState, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { CheckCircle2, XCircle, Loader2, Camera } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CheckinResult } from "@/components/checkin/checkin-result";
import { CheckinStats } from "@/components/checkin/checkin-stats";
import { useQrScanner } from "@/hooks/use-qr-scanner";

type CheckinState = "idle" | "scanning" | "processing" | "success" | "error";

function CheckinContent() {
  const searchParams = useSearchParams();
  const eventId = searchParams.get("event");

  const [state, setState] = useState<CheckinState>("idle");
  const [lastResult, setLastResult] = useState<{
    success: boolean;
    message: string;
    attendeeName?: string;
    ticketTierName?: string;
    timestamp?: Date;
  } | null>(null);

  const { startScan, stopScan, isScanning } = useQrScanner(async (qrCode) => {
    setState("processing");

    try {
      const response = await fetch("/api/checkin", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ qrCode }),
      });

      const data = await response.json();

      if (data.success) {
        setState("success");
        setLastResult({
          success: true,
          message: data.message,
          attendeeName: data.attendeeName,
          ticketTierName: data.ticketTierName,
          timestamp: new Date(data.checkedInAt),
        });

        // Auto-reset after 3 seconds
        setTimeout(() => {
          setState("scanning");
        }, 3000);
      } else {
        setState("error");
        setLastResult({
          success: false,
          message: data.message || "Check-in failed",
        });

        setTimeout(() => {
          setState("scanning");
        }, 3000);
      }
    } catch (error) {
      setState("error");
      setLastResult({
        success: false,
        message: "Network error. Please try again.",
      });

      setTimeout(() => {
        setState("scanning");
      }, 3000);
    }
  });

  const handleStartScan = () => {
    setState("scanning");
    startScan();
  };

  const handleStopScan = () => {
    stopScan();
    setState("idle");
  };

  return (
    <div className="container mx-auto px-4 py-8">
      <h1 className="text-3xl font-bold tracking-tight mb-8">Event Check-in</h1>

      {eventId && (
        <CheckinStats eventId={eventId} />
      )}

      <div className="grid gap-6 lg:grid-cols-2 max-w-4xl mx-auto mt-6">
        {/* Scanner */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Camera className="h-5 w-5" />
              QR Scanner
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div id="qr-reader" className="min-h-[300px] bg-muted rounded-lg flex items-center justify-center">
              {state === "idle" && (
                <div className="text-center p-8">
                  <Camera className="h-16 w-16 mx-auto text-muted-foreground mb-4" />
                  <p className="text-muted-foreground">
                    Click "Start Scanning" to begin
                  </p>
                </div>
              )}

              {state === "scanning" && (
                <div className="text-center p-8">
                  <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto mb-4" />
                  <p className="text-muted-foreground">
                    Point camera at QR code...
                  </p>
                </div>
              )}

              {state === "processing" && (
                <div className="text-center p-8">
                  <Loader2 className="h-12 w-12 animate-spin text-primary mx-auto mb-4" />
                  <p className="text-muted-foreground">
                    Processing check-in...
                  </p>
                </div>
              )}
            </div>

            {state === "idle" || state === "scanning" ? (
              <Button
                className="w-full"
                size="lg"
                onClick={state === "scanning" ? handleStopScan : handleStartScan}
              >
                {state === "scanning" ? "Stop Scanning" : "Start Scanning"}
              </Button>
            ) : (
              <Button
                className="w-full"
                size="lg"
                variant="outline"
                onClick={() => setState("scanning")}
              >
                Continue Scanning
              </Button>
            )}
          </CardContent>
        </Card>

        {/* Result */}
        <Card>
          <CardHeader>
            <CardTitle>Last Check-in</CardTitle>
          </CardHeader>
          <CardContent>
            {lastResult ? (
              <CheckinResult {...lastResult} />
            ) : (
              <div className="flex items-center justify-center h-64 text-muted-foreground">
                No check-ins yet
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

export default function CheckinPage() {
  return (
    <Suspense fallback={
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
        </div>
      </div>
    }>
      <CheckinContent />
    </Suspense>
  );
}
