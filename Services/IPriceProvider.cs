namespace StockAlert.Services;

internal interface IPriceProvider
{
    Task<decimal> GetPriceAsync(string symbol, CancellationToken cancellationToken);
}
