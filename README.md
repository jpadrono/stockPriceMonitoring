# stockPriceMonitoring

C# console tool for real-time B3 stock price monitoring.  
Monitora um ativo da B3 em tempo real e dispara alertas por e-mail quando os preços ultrapassam limites configurados de compra e venda.
O envio de e-mails é feito de forma assíncrona por meio de uma fila interna, evitando bloquear o loop de monitoramento.

---

# Table of Contents

- [Visão Geral](#visão-geral)
- [Getting Started](#getting-started)
- [Arquitetura](#arquitetura)
  - [Fluxo Geral](#fluxo-geral)
  - [Camadas e Responsabilidades](#camadas-e-responsabilidades)
  - [Diagrama Simplificado](#diagrama-simplificado)
- [Instalação](#instalação)
- [Como Executar](#como-executar)
  - [Exemplos de Execução](#exemplos-de-execução)
- [Configuração (`config.json`)](#configuração-configjson)
  - [Exemplo de SMTP Gmail](#exemplo-de-smtp-gmail)
  - [Exemplo de SMTP Outlook](#exemplo-de-smtp-outlook)
  - [Exemplo de SMTP para provedores como MailBox/Locaweb](#exemplo-de-smtp-para-provedores-como-mailboxlocaweb)
- [Extensibilidade](#extensibilidade)
  - [Criando um novo PriceProvider](#criando-um-novo-priceprovider)
  - [Criando um novo SMTP Profile](#criando-um-novo-smtp-profile)
- [Erros Comuns e Soluções](#erros-comuns-e-soluções)

---

# Visão Geral

Este projeto fornece uma ferramenta de linha de comando para monitoramento de preços de ativos da B3.  
O sistema coleta a cotação em tempo real via **Brapi.dev** e envia alertas por e-mail quando:

- o preço sobe **acima do limite de venda**
- o preço cai **abaixo do limite de compra**

Principais características:

- ✔ Monitoramento contínuo
- - ✔ Envio de alertas por e-mail via fila assíncrona (QueuedAlertSender)
- ✔ Suporte a vários provedores SMTP (Gmail, Outlook, MailBox, Custom)
- ✔ Arquitetura modular com Strategy Pattern
- ✔ Fácil extensão com novos providers
- ✔ Cancelamento via **Ctrl+C**
- ✔ Desenvolvido em **.NET 10**

---

# Getting Started

Passo a passo para executar o projeto pela primeira vez:

1. **Instalar o .NET 10**

   Verificar a versão instalada:

   ```
   dotnet --version
   ```

2. **Clonar o repositório e acessar o projeto**

   ```
   git clone https://github.com/seuusuario/stockPriceMonitoring.git
   cd stockPriceMonitoring/StockAlert
   ```

3. **Restaurar dependências**

   ```
   dotnet restore
   ```

4. **Criar o arquivo `config.json` na raiz do projeto**

   - Criar um arquivo `config.json` em `StockAlert/`
   - Utilizar a seção [Configuração (`config.json`)](#configuração-configjson) como referência de conteúdo
   - Ajustar:
     - `recipientEmail` para o e-mail que receberá os alertas
     - bloco `smtp` conforme o provedor (Gmail, Outlook, MailBox, etc.)

5. **Executar o monitor**

   ```
   dotnet run -- <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>
   ```

   Exemplo:

   ```
   dotnet run -- PETR4 22.67 22.50
   ```

---

# Arquitetura

A arquitetura foi projetada para ser modular, escalável e de fácil manutenção.

## Fluxo Geral

1. **Program.cs**
   - Lê argumentos
   - Valida preços
   - Carrega o `config.json`
   - Instancia PriceProvider + AlertSender
   - Inicia o PriceMonitor

2. **PriceMonitor**
   - Loop contínuo
   - Consulta preço
   - Verifica condições de alerta
   - Usa AlertState para evitar spam
   - Envia alertas pelo `IAlertSender`

3. **Providers**
   - `BrapiPriceProvider` obtém preço em tempo real.
   - `SmtpAlertSender` envia e-mails usando SMTP e perfis (Gmail, Outlook, MailBox, Custom).
   - `QueuedAlertSender` cria uma fila assíncrona de envio, desacoplando o tempo de IO de e-mail do caminho crítico do monitoramento.

---

## Camadas e Responsabilidades

### **Config/**
- Carregamento e validação do arquivo `config.json`
- Profiles de SMTP via Strategy Pattern
- Registry para resolução dinâmica dos providers

### **Monitoring/**
- `PriceMonitor` (loop principal)
- `AlertState` (histerese — previne spam)

### **Services/**
- `IPriceProvider` + `BrapiPriceProvider`
- `IAlertSender` + `SmtpAlertSender`
- `QueuedAlertSender`

---

## Diagrama Simplificado

```
Program.cs
   ├── Carrega AppConfig
   ├── Instancia IPriceProvider (BrapiPriceProvider)
   ├── Instancia IAlertSender (SmtpAlertSender)
   └── Chama PriceMonitor.RunAsync()
   
PriceMonitor
   ├── Loop:
   │     → priceProvider.GetPriceAsync()
   │     → alertState.ShouldSendBuy/Sell()
   │     → alertSender.SendAsync()
   └── Usa CancellationToken para Ctrl+C

Services/
   ├── IPriceProvider
   ├── BrapiPriceProvider (API brapi.dev)
   ├── IAlertSender
   ├── SmtpAlertSender (envio de e-mail)
   ├── QueuedAlertSender (fila assíncrona)
   └── SmtpProfileRegistry (Gmail, Outlook, MailBox, Custom)

Config/
   ├── AppConfig, SmtpConfig
   ├── Profiles: Gmail, Outlook, Mailbox, Custom
   └── AppConfigLoader
```

---

# Instalação

Requisitos:

- .NET 10 instalado  

Verificação rápida:

```
dotnet --version
```

Clone o repositório e acesse o projeto:

```
git clone https://github.com/seuusuario/stockPriceMonitoring.git
cd stockPriceMonitoring/StockAlert
```

Restaure dependências:

```
dotnet restore
```

---

# Como Executar

Formato recomendado (consistente com o `Program.cs`):

```
dotnet run --project StockAlert -- <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>
```

Exemplo:

```
dotnet run --project StockAlert -- PETR4 22.67 22.50
```

Pressione **Ctrl+C** para encerrar o monitoramento de forma segura.

---

## Exemplos de Execução

Monitorando PETR4:

```
dotnet run --project StockAlert -- PETR4 30.00 25.00
```

Saída esperada:

```
Monitorando PETR4 - venda ≥ 30.00, compra ≤ 25.00. Pressione Ctrl+C para encerrar.
[12:03:22] PETR4 = 28.15
[12:03:32] PETR4 = 29.98
Alerta de VENDA - PETR4 a 29.98
E-mail de alerta de venda enviado.
[12:03:42] PETR4 = 27.10
Alerta de COMPRA - PETR4 a 27.10
E-mail de alerta de compra enviado.
```

---

# Configuração (`config.json`)

O arquivo `config.json` controla:

- e-mail de destino
- provider SMTP
- credenciais
- símbolo sufixo
- intervalo do monitoramento
- token da brapi (opcional)

### Exemplo mínimo:

```
{
  "recipientEmail": "destino@example.com",
  "pollIntervalSeconds": 30,
  "symbolSuffix": "",
  "smtp": {
    "provider": "gmail",
    "useProviderDefaults": true,
    "username": "seuemail@gmail.com",
    "password": "SENHA_DE_APP"
  }
}
```

---

## Exemplo de SMTP Gmail

```
"smtp": {
  "provider": "gmail",
  "useProviderDefaults": true,
  "username": "seuemail@gmail.com",
  "password": "SENHA_DE_APP",
  "senderName": "Stock Alert Bot"
}
```

---

## Exemplo de SMTP Outlook

```
"smtp": {
  "provider": "outlook",
  "useProviderDefaults": true,
  "username": "seuemail@outlook.com",
  "password": "SENHA_DE_APP"
}
```

---

## Exemplo de SMTP para provedores como MailBox/Locaweb

```
"smtp": {
  "provider": "mailbox",
  "useProviderDefaults": true,
  "username": "email@seudominio.com.br",
  "password": "SUA_SENHA"
}
```

---

# Extensibilidade

A arquitetura foi pensada para facilitar expansão de funcionalidades sem alterar código existente (Open-Closed Principle).

---

## Criando um novo PriceProvider

1. Implementar `IPriceProvider`:

```
public interface IPriceProvider
{
    Task<decimal> GetPriceAsync(string ticker, CancellationToken ct);
}
```

2. Criar nova classe:

```
class MyApiPriceProvider : IPriceProvider
{
    public Task<decimal> GetPriceAsync(string ticker, CancellationToken ct)
    {
        // Lógica para consultar outra API de mercado
    }
}
```

3. Alterar apenas **Program.cs**:

```
IPriceProvider provider = new MyApiPriceProvider(config);
```

Nenhuma mudança é necessária em `PriceMonitor`.

---

## Criando um novo SMTP Profile

1. Implementar `ISmtpProfile`:

```
internal interface ISmtpProfile
{
    string Name { get; }
    void ApplyDefaults(SmtpConfig config);
}
```

2. Criar o perfil:

```
internal sealed class SendGridSmtpProfile : ISmtpProfile
{
    public string Name => "sendgrid";

    public void ApplyDefaults(SmtpConfig config)
    {
        config.Host ??= "smtp.sendgrid.net";
        if (config.Port <= 0)
        {
            config.Port = 587;
        }

        config.UseSsl = true;
    }
}
```

3. Registrar no `SmtpProfileRegistry` adicionando ao array de profiles.

---

# Erros Comuns e Soluções

### ❌ “Preço inválido”
Use ponto como separador decimal (ex.: `22.67`).

### ❌ “Arquivo config.json não encontrado”
Certifique-se de que o arquivo `config.json` está:

- na raiz do projeto `StockAlert/`, ou  
- no diretório atual da execução.

### ❌ SMTP rejeitou login
Causas comuns:

- Gmail sem senha de app configurada
- Outlook sem app password
- Host/porta incorretos no provider `"custom"`

### ❌ Alerta enviado várias vezes
Isso só ocorre se:

- o preço retorna para fora da zona de alerta e depois entra novamente.  

O `AlertState` aplica histerese para impedir reenvio contínuo enquanto o preço permanece acima/abaixo do limite.

