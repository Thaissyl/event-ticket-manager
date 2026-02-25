import { CheckCircle2, XCircle, Clock, User, Ticket } from "lucide-react";

interface CheckinResultProps {
  success: boolean;
  message: string;
  attendeeName?: string;
  ticketTierName?: string;
  timestamp?: Date;
}

export function CheckinResult({
  success,
  message,
  attendeeName,
  ticketTierName,
  timestamp,
}: CheckinResultProps) {
  return (
    <div className="space-y-4">
      {/* Status Icon */}
      <div className="flex justify-center">
        {success ? (
          <div className="rounded-full bg-green-100 p-4 dark:bg-green-900">
            <CheckCircle2 className="h-16 w-16 text-green-600 dark:text-green-400" />
          </div>
        ) : (
          <div className="rounded-full bg-red-100 p-4 dark:bg-red-900">
            <XCircle className="h-16 w-16 text-red-600 dark:text-red-400" />
          </div>
        )}
      </div>

      {/* Message */}
      <div className="text-center">
        <p className={`text-lg font-semibold ${success ? "text-green-600 dark:text-green-400" : "text-red-600 dark:text-red-400"}`}>
          {message}
        </p>
      </div>

      {/* Details */}
      {success && (attendeeName || ticketTierName || timestamp) && (
        <div className="space-y-3 pt-4 border-t">
          {attendeeName && (
            <div className="flex items-center gap-3">
              <User className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">Attendee</p>
                <p className="font-medium">{attendeeName}</p>
              </div>
            </div>
          )}

          {ticketTierName && (
            <div className="flex items-center gap-3">
              <Ticket className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">Ticket Type</p>
                <p className="font-medium">{ticketTierName}</p>
              </div>
            </div>
          )}

          {timestamp && (
            <div className="flex items-center gap-3">
              <Clock className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">Checked In At</p>
                <p className="font-medium">{timestamp.toLocaleTimeString()}</p>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
