"use client";

import { useEffect, useRef } from "react";

type QrScanCallback = (qrCode: string) => void;

export function useQrScanner(onScan: QrScanCallback) {
  const scannerRef = useRef<any>(null);
  const isScanningRef = useRef(false);

  const startScan = async () => {
    if (isScanningRef.current || !window) return;

    try {
      const { Html5Qrcode } = await import("html5-qrcode");

      scannerRef.current = new Html5Qrcode("qr-reader");

      await scannerRef.current.start(
        { facingMode: "environment" },
        {
          fps: 10,
          qrbox: { width: 250, height: 250 },
        },
        (decodedText: string) => {
          onScan(decodedText);
          // Pause scanning briefly to prevent duplicate scans
          scannerRef.current?.pause();
          setTimeout(() => {
            scannerRef.current?.resume();
          }, 2000);
        },
        () => {
          // Ignore scan errors (no QR code in frame)
        }
      );

      isScanningRef.current = true;
    } catch (error) {
      console.error("Failed to start QR scanner:", error);
    }
  };

  const stopScan = async () => {
    if (scannerRef.current && isScanningRef.current) {
      try {
        await scannerRef.current.stop();
        isScanningRef.current = false;
      } catch (error) {
        console.error("Failed to stop QR scanner:", error);
      }
    }
  };

  useEffect(() => {
    return () => {
      stopScan();
    };
  }, []);

  return {
    startScan,
    stopScan,
    isScanning: isScanningRef.current,
  };
}
