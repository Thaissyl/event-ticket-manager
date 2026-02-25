using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using EventTickets.Core.Entities;
using EventTickets.Core.Interfaces;
using EventTickets.Core.Services;

namespace EventTickets.Infrastructure.Services;

public class TicketPdfService : ITicketPdfService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IQrCodeService _qrCodeService;

    public TicketPdfService(
        ITicketRepository ticketRepository,
        IQrCodeService qrCodeService)
    {
        _ticketRepository = ticketRepository;
        _qrCodeService = qrCodeService;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateTicketPdfAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetWithTicketTierAndOrderAsync(ticketId, cancellationToken);
        if (ticket == null)
            throw new InvalidOperationException($"Ticket {ticketId} not found");

        var ticketTier = ticket.TicketTier;
        var eventEntity = ticketTier?.Event;
        var order = ticket.Order;

        var qrCode = ticket.QrCode;
        var qrImage = await GenerateQrImageAsync(qrCode);

        var document = new TicketDocument(ticket, eventEntity, ticketTier, order, qrImage);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateQrImageAsync(string qrCode)
    {
        var base64 = await _qrCodeService.GenerateQrImageAsync(qrCode);
        return Convert.FromBase64String(base64);
    }
}

public class TicketDocument : IDocument
{
    private readonly Ticket _ticket;
    private readonly Event? _event;
    private readonly TicketTier? _tier;
    private readonly Order? _order;
    private readonly byte[] _qrImage;

    public TicketDocument(Ticket ticket, Event? eventEntity, TicketTier? tier, Order? order, byte[] qrImage)
    {
        _ticket = ticket;
        _event = eventEntity;
        _tier = tier;
        _order = order;
        _qrImage = qrImage;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Calibri));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("This is your valid ticket. Present the QR code at the entrance.");
                    x.Span(" ");
                    x.Span("Ticket ID: ").FontColor(Colors.Grey.Darken2);
                    x.Span(_ticket.Id.ToString("N").Substring(0, 8).ToUpper()).FontColor(Colors.Grey.Darken2);
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("EVENT TICKET").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                column.Item().Text(_event?.Title ?? "Event").SemiBold().FontSize(12);
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            // Event Details
            column.Item().Element(ComposeEventDetails);
            column.Item().PaddingTop(10);

            // Ticket Details
            column.Item().Element(ComposeTicketDetails);
            column.Item().PaddingTop(10);

            // QR Code Section
            column.Item().Element(ComposeQrSection);
        });
    }

    void ComposeEventDetails(IContainer container)
    {
        container.ShowEntire().Column(column =>
        {
            column.Item().Background(Colors.Grey.Lighten3).Padding(10).Column(content =>
            {
                content.Item().Text("Event Details").SemiBold().FontColor(Colors.Grey.Darken2);
                content.Item().PaddingTop(5);

                if (_event != null)
                {
                    content.Item().Row(row =>
                    {
                        row.ConstantItem(80).Text("Date:").FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().Text(_event.StartDateTime.ToString("ddd, MMM dd, yyyy")).SemiBold();
                    });
                    content.Item().Row(row =>
                    {
                        row.ConstantItem(80).Text("Time:").FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().Text($"{_event.StartDateTime:HH:mm} - {_event.EndDateTime:HH:mm}").SemiBold();
                    });
                    content.Item().Row(row =>
                    {
                        row.ConstantItem(80).Text("Venue:").FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().Text(_event.VenueName).SemiBold();
                    });
                    content.Item().Row(row =>
                    {
                        row.ConstantItem(80).Text("Address:").FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().Text(_event.VenueAddress);
                    });
                }
            });
        });
    }

    void ComposeTicketDetails(IContainer container)
    {
        container.ShowEntire().Column(column =>
        {
            column.Item().Background(Colors.Blue.Lighten4).Padding(10).Column(content =>
            {
                content.Item().Text("Ticket Details").SemiBold().FontColor(Colors.Blue.Darken2);
                content.Item().PaddingTop(5);

                content.Item().Row(row =>
                {
                    row.ConstantItem(80).Text("Ticket:").FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().Text(_tier?.Name ?? "General Admission").SemiBold().FontSize(12);
                });
                content.Item().Row(row =>
                {
                    row.ConstantItem(80).Text("Attendee:").FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().Text(_ticket.AttendeeName).SemiBold();
                });
                content.Item().Row(row =>
                {
                    row.ConstantItem(80).Text("Email:").FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().Text(_ticket.AttendeeEmail);
                });
                if (_order != null)
                {
                    content.Item().Row(row =>
                    {
                        row.ConstantItem(80).Text("Order:").FontColor(Colors.Grey.Darken1);
                        row.RelativeItem().Text(_order.PaymentCode).FontColor(Colors.Grey.Darken2).FontSize(8);
                    });
                }
            });
        });
    }

    void ComposeQrSection(IContainer container)
    {
        container.AlignCenter().Column(column =>
        {
            column.Item().Width(150).Height(150).Image(_qrImage);
            column.Item().PaddingTop(10).Text("Scan this QR code at the entrance").FontSize(9).FontColor(Colors.Grey.Darken1);
        });
    }
}
