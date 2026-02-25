"use client";

import { useState, useEffect, useCallback } from "react";
import type { PaymentStatus } from "@/api/generated/api-schema";

export function usePaymentStatus(orderId: string) {
  const [status, setStatus] = useState<PaymentStatus>("pending");
  const [polling, setPolling] = useState(true);

  const checkStatus = useCallback(async () => {
    try {
      const response = await fetch(`/api/payments/status/${orderId}`, {
        credentials: "include",
      });

      if (response.ok) {
        const data = await response.json();
        setStatus(data.status);
        return data.status;
      }
    } catch (error) {
      console.error("Failed to check payment status:", error);
    }
    return null;
  }, [orderId]);

  useEffect(() => {
    let interval: NodeJS.Timeout;

    const poll = async () => {
      const currentStatus = await checkStatus();

      if (currentStatus === "completed" || currentStatus === "failed") {
        setPolling(false);
        if (interval) clearInterval(interval);
      }
    };

    poll();

    interval = setInterval(poll, 5000);

    return () => {
      if (interval) clearInterval(interval);
    };
  }, [checkStatus]);

  return { status, polling };
}
