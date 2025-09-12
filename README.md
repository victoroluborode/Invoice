# Invoice

QuestPDF template for generating invoices.

## Setup

### 1. Install NuGet Package
```bash
dotnet add package QuestPDF
```

### 2. Set License (Required)
```csharp
QuestPDF.Settings.License = LicenseType.Community;
```

## Usage

### Basic PDF Generation
```csharp
var invoice = new InvoiceModel
{
    CustomerName = "John Doe",
    InvoiceNumber = "WEL-INV-2025-001",
    IssueDate = DateTime.Now,
    DueDate = DateTime.Now.AddDays(30),
    DeliveryFee = 2500.00m,
    Discount = 1000.00m,
    TaxRate = 7.5m,
    Items = new List<Items>
    {
        new Items { Description = "Product Name", Quantity = 1, UnitPrice = 5000.00m }
    },
    PaymentInformation = new List<PaymentInformation>
    {
        new PaymentInformation 
        { 
            Bank = "Bank Name", 
            AccountName = "Account Name", 
            AccountNumber = "1234567890" 
        }
    },
    AdditionalInformation = "Thank you for your business!"
};

var document = new InvoiceDocument(invoice);
document.GeneratePdf("invoice.pdf");
```
