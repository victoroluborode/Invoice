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
                page.PageColor(Colors.Teal.Lighten5);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Plus Jakarta Sans"));
                page.Header().AlignCenter().Element(ComposeHeader);
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
            .Padding(15)
            .Column(column =>
            {
                column
                    .Item()
                    .PaddingTop(5)
                    .Text("INVOICE")
                    .FontSize(30)
                    .FontColor("#000000")
                    .Bold()
                    .LetterSpacing(0.5f);
            });
    }


    void ComposeContent(IContainer container)
    {
        container
        .Padding(30)
        .Column(column =>
        {

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(column =>
            {
                column.Item().Text("Billed To:").FontSize(11).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(5).Text(Model.CustomerName).FontSize(10).Bold();

                column.Item().PaddingTop(15).Text("Payment Information").FontSize(10).FontColor(Colors.Grey.Darken1);
                if (Model.PaymentInformation != null)
                {
                    foreach (var payment in Model.PaymentInformation)
                    {
                        column.Item()
                            .PaddingTop(5)
                            .Text($"{payment.Bank}, {payment.AccountName}, {payment.AccountNumber}")
                            .FontSize(10)
                            .Bold();
                    }
                }
            });


                row.RelativeItem().AlignRight().Column(column =>
            {
                column.Item().Text("Invoice No.").FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(2).Text(Model.InvoiceNumber).FontSize(10).Bold();
                column.Item().PaddingTop(8).Text("Issued Date").FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(2).Text(Model.IssueDate.ToString("dd MMMM yyyy")).FontSize(10).Bold();
                column.Item().PaddingTop(8).Text("Due Date").FontSize(10).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(2).Text(Model.DueDate.ToString("dd MMMM yyyy")).FontSize(10).Bold();
            });
            });
            column.Item().PaddingTop(30).Element(ComposeTable);
            column.Item().PaddingTop(20).Element(ComposeTotals);
            if (!string.IsNullOrWhiteSpace(Model.AdditionalInformation))
                column.Item().PaddingTop(25).Element(ComposeAdditionalInformation);
        });
    }

    void ComposeTable(IContainer container)
    {
        container
        .Table(table =>
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
                header.Cell().Element(CellStyle).Text("#");
                header.Cell().Element(CellStyle).Text("DESCRIPTION");
                header.Cell().Element(CellStyle).Text("QTY");
                header.Cell().Element(CellStyle).AlignRight().Text("UNIT PRICE");
                header.Cell().Element(CellStyle).AlignRight().Text("AMOUNT");

                IContainer CellStyle(IContainer container)
                {
                    return container
                        .Background(Colors.Black)
                        .PaddingVertical(5)
                        .DefaultTextStyle(x => x.Bold().FontColor(Colors.White));
                }
            });

            if (Model.Items != null)
            {
                foreach (var item in Model.Items)
                {
                    table.Cell().Element(CellStyle).Text((Model.Items.IndexOf(item) + 1).ToString());
                    table.Cell().Element(CellStyle).Text(item.Description);
                    table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text($"₦{item.UnitPrice:N2}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"₦{item.Amount:N2}");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            }
        });
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
        container
            .PaddingBottom(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(label)
                    .FontSize(isTotal ? 12 : 9)
                    .FontColor(isTotal ? Colors.Black : Colors.Black)
                    .Bold();

                row.RelativeItem()
                    .AlignRight()
                    .Text(amount)
                    .FontSize(isTotal ? 16 : 10)
                    .FontColor(isTotal ? Colors.Black : Colors.Black)
                    .Bold();
            });
    }
    void ComposeAdditionalInformation(IContainer container)
    {
        container.Background(Colors.Black).Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text(Model.AdditionalInformation).FontColor(Colors.White);
        });
    }
