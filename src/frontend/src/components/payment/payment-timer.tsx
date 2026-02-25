"use client";

import { useState, useEffect } from "react";
import { AlertCircle } from "lucide-react";

interface PaymentTimerProps {
  expiresAt: Date;
  onExpired: () => void;
}

export function PaymentTimer({ expiresAt, onExpired }: PaymentTimerProps) {
  const [timeLeft, setTimeLeft] = useState<number>(0);

  useEffect(() => {
    const calculateTimeLeft = () => {
      const now = new Date().getTime();
      const expiration = new Date(expiresAt).getTime();
      return Math.max(0, expiration - now);
    };

    setTimeLeft(calculateTimeLeft());

    const timer = setInterval(() => {
      const remaining = calculateTimeLeft();
      setTimeLeft(remaining);

      if (remaining === 0) {
        onExpired();
      }
    }, 1000);

    return () => clearInterval(timer);
  }, [expiresAt, onExpired]);

  const minutes = Math.floor(timeLeft / 60000);
  const seconds = Math.floor((timeLeft % 60000) / 1000);

  const isWarning = minutes < 5;

  return (
    <div
      className={`flex items-center justify-center gap-2 rounded-lg border p-4 ${
        isWarning
          ? "border-yellow-200 bg-yellow-50 text-yellow-800 dark:border-yellow-800 dark:bg-yellow-950 dark:text-yellow-200"
          : "border-muted bg-muted"
      }`}
    >
      <AlertCircle className={`h-5 w-5 ${isWarning ? "animate-pulse" : ""}`} />
      <div className="text-center">
        <p className="text-sm font-medium">
          {isWarning ? "Payment expiring soon!" : "Time remaining to pay"}
        </p>
        <p className="text-2xl font-bold tabular-nums">
          {String(minutes).padStart(2, "0")}:{String(seconds).padStart(2, "0")}
        </p>
      </div>
    </div>
  );
}
