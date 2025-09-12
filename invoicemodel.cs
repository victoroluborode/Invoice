public class InvoiceModel
{
    public required string InvoiceNumber { get; set; }

    public required string CustomerName { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }

    public List<PaymentInformation>? PaymentInformation { get; set; }

    public List<Items>? Items { get; set; }
    public required string AdditionalInformation { get; set; }

    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
}

public class Items
{
    public required string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public decimal Amount => Quantity * UnitPrice;
}

public class PaymentInformation
{
    public required string Bank { get; set; }
    public required string AccountName { get; set; }
    public required string AccountNumber { get; set; }
}
