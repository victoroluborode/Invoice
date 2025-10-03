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
        container.Page(page =>
        {
            page.Margin(0);
            page.PageColor("#000000");
            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Plus Jakarta Sans").FontColor("#000000"));

            page.Header().Element(ComposeHeader);

            page.Content().Padding(40).Column(content =>
            {
                content.Item().Row(row =>
                {
                    row.RelativeItem(5).PaddingRight(15).Element(ComposeCustomerCard);
                    row.RelativeItem(5).PaddingLeft(15).Element(ComposePaymentCard);
                });

                content.Item().PaddingTop(30).Element(ComposeItemsTable);

                content.Item().PaddingTop(25).Element(ComposeTotalsPanel);

                if (!string.IsNullOrWhiteSpace(Model.AdditionalInformation))
                {
                    content.Item().PaddingTop(20).Background("#111111").Padding(15).Text(Model.AdditionalInformation)
                        .FontSize(9).FontColor("#ffffff");
                }
            });

            page.Footer().Background("#111111").Padding(15).Row(footerRow =>
            {
                footerRow.RelativeItem().AlignLeft()
                    .DefaultTextStyle(x => x.FontSize(9).FontColor("#bbbbbb"))
                    .Text(t =>
                    {
                        t.CurrentPageNumber();
                        t.Span(" / ");
                        t.TotalPages();
                    });

                footerRow.RelativeItem().AlignRight().Text("THANK YOU")
                    .FontSize(10).FontColor("#ffffff").Bold();
            });
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Background("#111111").Padding(20).Row(row =>
        {
            row.RelativeItem().Text("INVOICE")
                .FontSize(24).FontColor("#ffffff").Bold().LetterSpacing(1f);

            row.RelativeItem().AlignRight().Text($"Invoice No: {Model.InvoiceNumber}")
                .FontSize(11).FontColor("#ffffff");
        });
    }

    void ComposeCustomerCard(IContainer container)
    {
        container.Background("#111111").Padding(20).Column(card =>
        {
            card.Item().Text("BILLED TO")
                .FontSize(10).FontColor("#ffffff").Bold();

            card.Item().PaddingTop(8).Text(Model.CustomerName)
                .FontSize(12).FontColor("#ffffff").Bold();

            card.Item().PaddingTop(15).Text($"Issued: {Model.IssueDate:dd MMM yyyy}")
                .FontSize(10).FontColor("#cccccc");

            card.Item().Text($"Due: {Model.DueDate:dd MMM yyyy}")
                .FontSize(10).FontColor("#cccccc");
        });
    }

    void ComposePaymentCard(IContainer container)
    {
        container.Background("#111111").Padding(20).Column(card =>
        {
            card.Item().Text("PAYMENT DETAILS")
                .FontSize(10).FontColor("#ffffff").Bold();

            if (Model.PaymentInformation != null)
            {
                foreach (var payment in Model.PaymentInformation)
                {
                    card.Item().PaddingTop(10).Column(bank =>
                    {
                        bank.Item().Text(payment.Bank).FontSize(10).FontColor("#ffffff").Bold();
                        bank.Item().Text(payment.AccountName).FontSize(9).FontColor("#bbbbbb");
                        bank.Item().Text(payment.AccountNumber).FontSize(11).FontColor("#ffffff").Bold();
                    });
                }
            }
        });
    }

    void ComposeItemsTable(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Background("#ffffff").Padding(10).Row(headerRow =>
            {
                headerRow.ConstantItem(40).Text("#").FontColor("#111111").Bold();
                headerRow.RelativeItem(3).Text("Description").FontColor("#111111").Bold();
                headerRow.RelativeItem(1).AlignCenter().Text("Qty").FontColor("#111111").Bold();
                headerRow.RelativeItem(2).AlignRight().Text("Unit Price").FontColor("#111111").Bold();
                headerRow.RelativeItem(2).AlignRight().Text("Amount").FontColor("#111111").Bold();
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

    void ComposeTotalsPanel(IContainer container)
    {
        var subtotal = Model.Items?.Sum(x => x.Amount) ?? 0;
        var delivery = Model.DeliveryFee;
        var discount = Model.Discount;
        var taxRate = Model.TaxRate;
        var taxAmount = subtotal * (taxRate / 100);
        var total = subtotal + delivery - discount + taxAmount;

        container.Background("#111111").Padding(20).Column(sum =>
        {
            void AddRow(string label, string value, string color = "#ffffff", int fontSize = 10, bool bold = false)
            {
                sum.Item().Row(r =>
                {
                    r.RelativeItem().Text(label).FontColor("#cccccc").FontSize(9);
                    r.RelativeItem().AlignRight().Text(value)
                        .FontColor(color)
                        .FontSize(fontSize)
                        .Bold();
                });
            }

            AddRow("Subtotal", $"₦{subtotal:N2}");
            AddRow("Delivery", $"₦{delivery:N2}");
            AddRow("Discount", $"-₦{discount:N2}", "#ff6b6b");
            AddRow("Tax", $"+₦{taxAmount:N2}");

            sum.Item().PaddingTop(10).Row(r =>
            {
                r.RelativeItem().Text("TOTAL")
                    .FontColor("#ffffff")
                    .FontSize(11)
                    .Bold();

                r.RelativeItem().AlignRight().Text($"₦{total:N2}")
                    .FontColor("#ffffff")
                    .FontSize(15)
                    .Bold();
            });
        });
    }
}