using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventTickets.API.Endpoints;

public static class TicketEndpoints
{
    public static void MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tickets")
            .WithTags("Tickets")
            .WithOpenApi();

        // GET /api/tickets/{id}/pdf - Download ticket as PDF
        group.MapGet("/{id:guid}/pdf", async (
            Guid id,
            [FromServices] ITicketPdfService ticketPdfService,
            [FromServices] ITicketRepository ticketRepo,
            CancellationToken ct) =>
        {
            var ticket = await ticketRepo.GetByIdAsync(id, ct);
            if (ticket == null)
                return Results.NotFound(new { message = "Ticket not found" });

            var pdfBytes = await ticketPdfService.GenerateTicketPdfAsync(id, ct);

            return Results.File(
                pdfBytes,
                "application/pdf",
                $"ticket-{id:N}.pdf"
            );
        })
        .WithName("DownloadTicketPdf")
        .WithSummary("Download ticket as PDF")
        .WithOpenApi();

        // GET /api/tickets/{id}/qr - Get ticket QR code image
        group.MapGet("/{id:guid}/qr", async (
            Guid id,
            [FromServices] ITicketRepository ticketRepo,
            [FromServices] IQrCodeService qrCodeService,
            CancellationToken ct) =>
        {
            var ticket = await ticketRepo.GetByIdAsync(id, ct);
            if (ticket == null)
                return Results.NotFound(new { message = "Ticket not found" });

            var qrBase64 = await qrCodeService.GenerateQrImageAsync(ticket.QrCode);
            var qrImage = Convert.FromBase64String(qrBase64);

            return Results.File(
                qrImage,
                "image/png",
                $"qr-{id:N}.png"
            );
        })
        .WithName("GetTicketQrCode")
        .WithSummary("Get ticket QR code image")
        .WithOpenApi();
    }
}
