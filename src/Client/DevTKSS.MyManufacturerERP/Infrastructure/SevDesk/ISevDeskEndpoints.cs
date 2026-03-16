namespace DevTKSS.MyManufacturerERP.Infrastructure.SevDesk;

/// <summary>
/// Refit interface for SevDesk API v1.
/// Authentication is handled by <see cref="SevDeskApiKeyHandler"/>.
/// See https://api.sevdesk.de/ for the full API reference.
/// </summary>
[Headers("Content-Type: application/json")]
public interface ISevDeskEndpoints
{
    [Get("/Contact")]
    Task<ApiResponse<SevDeskListResponse<SevDeskContact>>> GetContactsAsync(CancellationToken cancellationToken = default);

    [Get("/Contact/{id}")]
    Task<ApiResponse<SevDeskObjectResponse<SevDeskContact>>> GetContactAsync(long id, CancellationToken cancellationToken = default);

    [Get("/Invoice")]
    Task<ApiResponse<SevDeskListResponse<SevDeskInvoice>>> GetInvoicesAsync(CancellationToken cancellationToken = default);

    [Get("/Invoice/{id}")]
    Task<ApiResponse<SevDeskObjectResponse<SevDeskInvoice>>> GetInvoiceAsync(long id, CancellationToken cancellationToken = default);

    [Get("/Voucher")]
    Task<ApiResponse<SevDeskListResponse<SevDeskVoucher>>> GetVouchersAsync(CancellationToken cancellationToken = default);

    [Get("/Voucher/{id}")]
    Task<ApiResponse<SevDeskObjectResponse<SevDeskVoucher>>> GetVoucherAsync(long id, CancellationToken cancellationToken = default);
}

#region SevDesk response wrapper types

public record SevDeskListResponse<T>
{
    [JsonPropertyName("objects")]
    public List<T> Objects { get; init; } = [];
}

public record SevDeskObjectResponse<T>
{
    [JsonPropertyName("objects")]
    public T? Objects { get; init; }
}

#endregion

#region SevDesk domain models (minimal subset)

public record SevDeskContact
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("surename")]
    public string? Surname { get; init; }

    [JsonPropertyName("familyname")]
    public string? FamilyName { get; init; }

    [JsonPropertyName("category")]
    public SevDeskCategory? Category { get; init; }
}

public record SevDeskInvoice
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("invoiceNumber")]
    public string? InvoiceNumber { get; init; }

    [JsonPropertyName("invoiceDate")]
    public string? InvoiceDate { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("header")]
    public string? Header { get; init; }

    [JsonPropertyName("totalNet")]
    public string? TotalNet { get; init; }

    [JsonPropertyName("totalGross")]
    public string? TotalGross { get; init; }
}

public record SevDeskVoucher
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("voucherDate")]
    public string? VoucherDate { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("sumNet")]
    public string? SumNet { get; init; }

    [JsonPropertyName("sumGross")]
    public string? SumGross { get; init; }
}

public record SevDeskCategory
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("objectName")]
    public string? ObjectName { get; init; }
}

#endregion
