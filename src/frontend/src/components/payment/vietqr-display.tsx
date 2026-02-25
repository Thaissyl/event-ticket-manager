"use client";

import { useState } from "react";
import { CreditCard, Copy, Check } from "lucide-react";
import { Button } from "@/components/ui/button";

interface VietQrDisplayProps {
  qrCodeUrl: string;
  paymentCode: string;
  amount: number;
}

export function VietQrDisplay({ qrCodeUrl, paymentCode, amount }: VietQrDisplayProps) {
  const [copied, setCopied] = useState(false);
  const [imageError, setImageError] = useState(false);

  const copyPaymentCode = async () => {
    await navigator.clipboard.writeText(paymentCode);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="space-y-4">
      <div className="flex aspect-square w-full max-w-xs mx-auto items-center justify-center rounded-lg border bg-white p-4">
        {imageError ? (
          <div className="flex flex-col items-center text-center">
            <CreditCard className="h-16 w-16 text-muted-foreground mb-2" />
            <p className="text-sm text-muted-foreground">QR Code unavailable</p>
            <p className="text-xs text-muted-foreground mt-1">Please refresh the page</p>
          </div>
        ) : (
          <img
            src={qrCodeUrl}
            alt="VietQR Payment Code"
            className="h-full w-full object-contain"
            onError={() => setImageError(true)}
          />
        )}
      </div>

      <div className="space-y-2 text-center">
        <p className="text-sm text-muted-foreground">
          Scan with your banking app to pay
        </p>
        <p className="text-2xl font-bold">
          ${amount.toFixed(2)}
        </p>
      </div>

      <div className="flex items-center justify-center gap-2">
        <div className="flex items-center gap-2 rounded-md border bg-muted px-3 py-2">
          <code className="font-mono text-sm">{paymentCode}</code>
          <Button
            variant="ghost"
            size="icon"
            className="h-6 w-6"
            onClick={copyPaymentCode}
          >
            {copied ? (
              <Check className="h-3 w-3 text-green-500" />
            ) : (
              <Copy className="h-3 w-3" />
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
