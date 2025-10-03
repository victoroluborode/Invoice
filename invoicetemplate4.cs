using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


public class InvoiceDocument : IDocument
{
    public InvoiceModel Model { get; }

    public InvoiceDocument(InvoiceModel model)
    {
        Model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Plus Jakarta Sans"));
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
    }

    void ComposeHeader(IContainer container)
    {
        container
            .Background(Colors.Black) 
            .PaddingVertical(15) 
            .PaddingHorizontal(30)
            .Row(row =>
            {
                
                row.ConstantItem(200).AlignLeft().Text("INVOICE")
                    .FontSize(28).FontColor("#FFFFFF").Bold();

                
                row.RelativeItem().AlignRight().Background(Colors.Grey.Lighten4).PaddingHorizontal(10).PaddingVertical(10).Column(column =>
                {
                    column.Item().Text($"#{Model.InvoiceNumber}").FontSize(10).FontColor(Colors.Black).Bold();
                    column.Item().PaddingTop(5).Text($"Issued Date: {Model.IssueDate.ToString("dddd, MMMM d, yyyy")}").FontColor(Colors.Black).Bold();
                    column.Item().PaddingTop(5).Text($"Due Date: {Model.DueDate.ToString("dddd, MMMM d, yyyy")}").FontColor(Colors.Black).Bold();
                });
            });
    }

    void ComposeContent(IContainer container)
    {
        container
            .PaddingTop(20)
            .Column(column =>
            {
                column.Spacing(25);
                column.Item().Element(ComposeDetails);
                column.Item().Element(ComposeTable);

                
                column.Item().ShowEntire().Element(ComposeTotals);

                if (!string.IsNullOrWhiteSpace(Model.AdditionalInformation))
                    column.Item().Element(ComposeAdditionalInformation);
            });
    }
    void ComposeDetails(IContainer container)
    {
        container
            .Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Billed To").FontSize(11).FontColor(Colors.Grey.Medium);
                    column.Item().PaddingTop(2).Text(Model.CustomerName).FontSize(11).Bold();
                    column.Item().PaddingTop(20).Text("Payment Information").FontSize(11).FontColor(Colors.Grey.Medium);
                    var paymentInfo = Model.PaymentInformation?.FirstOrDefault();
                    var paymentText = paymentInfo != null ? $"{paymentInfo.Bank}, {paymentInfo.AccountName}, {paymentInfo.AccountNumber}" : string.Empty;
                    if (!string.IsNullOrEmpty(paymentText))
                    {
                        column.Item().PaddingTop(2).Text(paymentText).FontSize(11).Bold();
                    }
                });
            });
    }


    void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(50);
                columns.RelativeColumn(2);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("#");
                header.Cell().Element(HeaderCellStyle).Text("DESCRIPTION");
                header.Cell().Element(HeaderCellStyle).Text("QTY");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("UNIT PRICE");
                header.Cell().Element(HeaderCellStyle).AlignRight().Text("AMOUNT");

                
                static IContainer HeaderCellStyle(IContainer container)
                {
                    return container
                        .DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                        .Background(Colors.Black)
                        .PaddingVertical(5)
                        .BorderBottom(1)
                        .BorderColor(Colors.Black);
                }
            });

            if (Model.Items != null)
            {
                for (int i = 0; i < Model.Items.Count; i++)
                {
                    var item = Model.Items[i];

                    var isEvenRow = i % 2 == 0;
                    var rowBackground = isEvenRow ? Colors.Grey.Lighten2 : Colors.White;

                    table.Cell().Element(c => CellStyle(c, rowBackground)).Text((i + 1).ToString());
                    table.Cell().Element(c => CellStyle(c, rowBackground)).Text(item.Description);
                    table.Cell().Element(c => CellStyle(c, rowBackground)).Text(item.Quantity.ToString());
                    table.Cell().Element(c => CellStyle(c, rowBackground)).AlignRight().Text($"₦{item.UnitPrice:N2}");
                    table.Cell().Element(c => CellStyle(c, rowBackground)).AlignRight().Text($"₦{item.Amount:N2}");
                }
            }
        });
    }

    
    static IContainer CellStyle(IContainer container, Color background)
    {
        return container
            .Background(background)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5);
    }
    void ComposeTotals(IContainer container)
    {
        var subtotal = Model.Items?.Sum(x => x.Amount) ?? 0;
        var delivery = Model.DeliveryFee;
        var discount = Model.Discount;
        var taxRate = Model.TaxRate;
        var taxAmount = subtotal * (taxRate / 100);
        var total = subtotal + delivery - discount + taxAmount;

        container
            .AlignRight()
            .Width(250)
            .Column(column =>
            {
                column.Item().Element(c => ComposeTotalRow(c, "SUBTOTAL", $"₦{subtotal:N2}", false));
                column.Item().Element(c => ComposeTotalRow(c, "DELIVERY", $"₦{delivery:N2}", false));
                column.Item().Element(c => ComposeTotalRow(c, "DISCOUNT", $"-₦{discount:N2}", false));
                column.Item().AlignRight().Element(c => ComposeTotalRow(c, "TAX", $"{taxRate}%, +₦{taxAmount:N2}", false));
                column.Item().PaddingTop(5).AlignRight().Element(c => ComposeTotalRow(c, "TOTAL", $"₦{total:N2}", true));
            });
    }

    void ComposeTotalRow(IContainer container, string label, string amount, bool isTotal)
    {
        var totalContainer = isTotal ? container.Background(Colors.Black).Padding(5) : container.PaddingBottom(8);

        totalContainer
            .Row(row =>
            {
                row.RelativeItem()
                    .AlignBottom() 
                    .Text(label)
                    .FontSize(isTotal ? 12 : 9)
                    .FontColor(isTotal ? Colors.White : Colors.Black)
                    .Bold();

                row.RelativeItem()
                    .AlignBottom() 
                    .AlignRight()
                    .Text(amount)
                    .FontSize(isTotal ? 16 : 10)
                    .FontColor(isTotal ? Colors.White : Colors.Black)
                    .Bold();
            });
    }
    void ComposeAdditionalInformation(IContainer container)
    {
        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text(Model.AdditionalInformation);
        });
    }

    void ComposeDetailRow(IContainer container, string label, string value)
    {
        container
            .PaddingBottom(12)
            .Row(row =>
            {
                row.ConstantItem(150)
                    .Text(label)
                    .FontSize(12)
                    .FontColor(Colors.Grey.Medium);

                row.RelativeItem()
                    .Text(value)
                    .FontSize(12)
                    .FontColor(Colors.Black)
                    .Medium();
            });
    }
}

