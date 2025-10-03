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
                page.PageColor("#111111");
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Plus Jakarta Sans").FontColor(Colors.White));
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
            .Background("#0a0a0a")
            .Padding(20)
            .Column(column =>
            {
                column
                    .Item()
                    .PaddingTop(5)
                    .Text("INVOICE")
                    .FontSize(30)
                    .AlignCenter()
                    .FontColor(Colors.White)
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
                    column.Item().Text("Billed To:").FontSize(11).FontColor(Colors.White);
                    column.Item().PaddingTop(5).Text(Model.CustomerName).FontSize(10).Bold();

                    column.Item().PaddingTop(15).Text("Payment Information").FontSize(10).FontColor(Colors.White);
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
                    column.Item().Text("Invoice No.").FontSize(10).FontColor(Colors.White);
                    column.Item().PaddingTop(2).Text(Model.InvoiceNumber).FontSize(10).Bold();
                    column.Item().PaddingTop(8).Text("Issued Date").FontSize(10).FontColor(Colors.White);
                    column.Item().PaddingTop(2).Text(Model.IssueDate.ToString("dd MMMM yyyy")).FontSize(10).Bold();
                    column.Item().PaddingTop(8).Text("Due Date").FontSize(10).FontColor(Colors.White);
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
        container.Column(column =>
        {
            
            column.Item().Background(Colors.White).Padding(10).Row(headerRow =>
            {
                headerRow.ConstantItem(40).Text("#").FontColor(Colors.Black).Bold();
                headerRow.RelativeItem(3).Text("Description").FontColor(Colors.Black).Bold();
                headerRow.RelativeItem(1).AlignCenter().Text("Qty").FontColor(Colors.Black).Bold();
                headerRow.RelativeItem(2).AlignRight().Text("Unit Price").FontColor(Colors.Black).Bold();
                headerRow.RelativeItem(2).AlignRight().Text("Amount").FontColor(Colors.Black).Bold();
            });

            
            if (Model.Items != null)
            {
                for (int i = 0; i < Model.Items.Count; i++)
                {
                    var item = Model.Items[i];
                    var bgColor = i % 2 == 0 ? "#111111" : "#0a0a0a";

                    column.Item().Background(bgColor).PaddingVertical(8).PaddingHorizontal(10).Row(itemRow =>
                    {
                        itemRow.ConstantItem(40).Text((i + 1).ToString("00")).FontColor("#ffffff").Bold();
                        itemRow.RelativeItem(3).Text(item.Description).FontSize(10).FontColor("#ffffff");
                        itemRow.RelativeItem(1).AlignCenter().Text(item.Quantity.ToString()).FontColor("#ffffff");
                        itemRow.RelativeItem(2).AlignRight().Text($"₦{item.UnitPrice:N2}").FontColor("#ffffff");
                        itemRow.RelativeItem(2).AlignRight().Text($"₦{item.Amount:N2}").FontColor("#ffffff").Bold();
                    });
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
            column.Item().Element(c => ComposeTotalRow(c, "DISCOUNT", $"-₦{discount:N2}", false, isDiscount: true));
            column.Item().Element(c => ComposeTotalRow(c, "TAX", $"{taxRate}%, +₦{taxAmount:N2}", false));
            column.Item().PaddingTop(5).Element(c => ComposeTotalRow(c, "TOTAL", $"₦{total:N2}", true));
        });
    }

    void ComposeTotalRow(IContainer container, string label, string amount, bool isTotal, bool isDiscount = false)
    {
        container
            .PaddingBottom(8)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(label)
                    .FontSize(isTotal ? 12 : 9)
                    .FontColor(Colors.White)
                    .Bold();

                row.RelativeItem()
                    .AlignRight()
                    .Text(amount)
                    .FontSize(isTotal ? 16 : 10)
                    .FontColor(isTotal ? Colors.White : isDiscount ? Colors.Red.Medium : Colors.White)
                    .Bold();
            });
    }

    void ComposeAdditionalInformation(IContainer container)
    {
        container.Background("#0a0a0a").Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text(Model.AdditionalInformation).FontColor(Colors.White);
        });
    }

    