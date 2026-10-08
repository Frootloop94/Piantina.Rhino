namespace Piantina.Core.Materials;

/// <summary>
/// Outcome of MaterialService.ImportPriceList(), for the caller to report
/// back to the user.
/// </summary>
public record PriceImportResult(int UpdatedCount, IReadOnlyList<string> AddedNames);
