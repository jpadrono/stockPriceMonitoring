namespace StockAlert.Monitoring;

internal sealed class AlertState
{
    private bool _sellAlertActive;
    private bool _buyAlertActive;

    // Define se um alerta de venda deve ser enviado.
    // O alerta só é disparado na transição de "preço abaixo do alvo" → "preço acima ou igual ao alvo".
    // Mantém histerese para evitar reenvio contínuo enquanto o preço permanecer acima do limite.
    public bool ShouldSendSell(decimal price, decimal sellTarget)
    {
        if (price >= sellTarget)
        {
            if (_sellAlertActive)
            {
                return false;
            }

            _sellAlertActive = true;
            return true;
        }

        _sellAlertActive = false;
        return false;
    }

    // Define se um alerta de compra deve ser enviado.
    // O alerta só é disparado na transição de "preço acima do alvo" → "preço abaixo ou igual ao alvo".
    // Mantém histerese para evitar reenvio contínuo enquanto o preço permanecer abaixo do limite.
    public bool ShouldSendBuy(decimal price, decimal buyTarget)
    {
        if (price <= buyTarget)
        {
            if (_buyAlertActive)
            {
                return false;
            }

            _buyAlertActive = true;
            return true;
        }

        _buyAlertActive = false;
        return false;
    }
}
