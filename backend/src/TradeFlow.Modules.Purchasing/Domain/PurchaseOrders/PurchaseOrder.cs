using System.Text.RegularExpressions;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Modules.Purchasing.Domain.PurchaseOrders;

public sealed class PurchaseOrder
{
    private readonly List<PurchaseOrderLine> _lines = [];

    public Guid Id { get; private set; }
    public Guid OrganisationId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public PurchaseOrderStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public Guid? ConfirmedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines;
    public decimal TotalAmount => _lines.Sum(line => line.LineTotal);

    public static PurchaseOrder Create(
        Guid organisationId,
        Guid supplierId,
        string reference,
        IReadOnlyCollection<PurchaseOrderLineDraft> lines,
        DateTimeOffset createdAt,
        Guid createdBy)
    {
        ValidateRequiredIdentifier(
            organisationId,
            PurchaseOrderErrors.OrganisationRequiredCode,
            PurchaseOrderErrors.OrganisationRequiredMessage);
        ValidateRequiredIdentifier(
            supplierId,
            PurchaseOrderErrors.SupplierRequiredCode,
            PurchaseOrderErrors.SupplierRequiredMessage);
        ValidateRequiredIdentifier(
            createdBy,
            PurchaseOrderErrors.CreatedByRequiredCode,
            PurchaseOrderErrors.CreatedByRequiredMessage);

        if (lines is null || lines.Count == 0)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.LinesRequiredCode,
                PurchaseOrderErrors.LinesRequiredMessage);
        }

        if (lines.Select(line => line.ProductId).Distinct().Count() != lines.Count)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.DuplicateProductCode,
                PurchaseOrderErrors.DuplicateProductMessage);
        }

        PurchaseOrder order =
            new()
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisationId,
                SupplierId = supplierId,
                Reference = NormalizeReference(reference),
                Status = PurchaseOrderStatus.Draft,
                CreatedAt = createdAt.ToUniversalTime(),
                CreatedBy = createdBy,
                ConfirmedAt = null,
                ConfirmedBy = null,
                RowVersion = [],
            };

        order._lines.AddRange(
            lines.Select(line => PurchaseOrderLine.Create(order.Id, line)));

        return order;
    }

    public void Confirm(DateTimeOffset confirmedAt, Guid confirmedBy)
    {
        ValidateRequiredIdentifier(
            confirmedBy,
            PurchaseOrderErrors.ConfirmedByRequiredCode,
            PurchaseOrderErrors.ConfirmedByRequiredMessage);

        if (Status == PurchaseOrderStatus.Confirmed)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.AlreadyConfirmedCode,
                PurchaseOrderErrors.AlreadyConfirmedMessage);
        }

        Status = PurchaseOrderStatus.Confirmed;
        ConfirmedAt = confirmedAt.ToUniversalTime();
        ConfirmedBy = confirmedBy;
    }

    private static string NormalizeReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.ReferenceRequiredCode,
                PurchaseOrderErrors.ReferenceRequiredMessage);
        }

        string normalized = Regex.Replace(reference.Trim(), @"\s+", " ").ToUpperInvariant();

        if (normalized.Length > 50)
        {
            throw new BusinessRuleException(
                PurchaseOrderErrors.ReferenceInvalidCode,
                PurchaseOrderErrors.ReferenceInvalidMessage);
        }

        return normalized;
    }

    private static void ValidateRequiredIdentifier(Guid value, string code, string message)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessRuleException(code, message);
        }
    }

    private PurchaseOrder()
    {
    }
}
