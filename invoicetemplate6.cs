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
            page.PageColor("#0a0a0a");
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Plus Jakarta Sans").FontColor("#ffffff"));

            page.Header().Element(ComposeHeader);

            page.Footer().AlignRight().PaddingRight(40).PaddingBottom(10)
                .DefaultTextStyle(t => t.FontSize(8).FontColor("#666666"))
                .Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });

            page.Content().Column(column =>
            {
                column.Item().PaddingHorizontal(40).PaddingVertical(30).Row(row =>
                {
                    row.RelativeItem(5).PaddingRight(40).Column(leftColumn =>
                    {
                        leftColumn.Item().Background("#1a1a1a").Padding(15).Column(customerColumn =>
                        {
                            customerColumn.Item().Text("Billed To")
                                .FontSize(11)
                                .FontColor("#cccccc")
                                .Bold();

                            customerColumn.Item().PaddingTop(8).Text(Model.CustomerName)
                                .FontSize(13)
                                .FontColor("#ffffff")
                                .Bold();
                        });

                        leftColumn.Item().PaddingTop(30).Element(ComposeTable);
                    });

                    row.RelativeItem(2).Element(ComposeRightSidebar);
                });

                if (!string.IsNullOrWhiteSpace(Model.AdditionalInformation))
                {
                    column.Item().Background("#1a1a1a").PaddingHorizontal(40).PaddingVertical(15).Row(footerRow =>
                    {
                        footerRow.RelativeItem().Column(footerColumn =>
                        {
                            footerColumn.Item().Text(Model.AdditionalInformation)
                                .FontSize(10)
                                .FontColor("#ffffff");
                        });

                        footerRow.ConstantItem(120).AlignRight().AlignMiddle().Text("Thank You")
                            .FontSize(12)
                            .FontColor("#ffffff")
                            .Bold();
                    });
                }
            });
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem(3).Background("#ffffff").Padding(30).Column(headerColumn =>
            {
                headerColumn.Item().Text("INVOICE")
                    .FontSize(36)
                    .FontColor("#0a0a0a")
                    .Bold();

                headerColumn.Item().PaddingTop(6).Text($"Invoice No: {Model.InvoiceNumber}")
                    .FontSize(11)
                    .FontColor("#666666");
            });

            row.ConstantItem(140).Background("#000435").Padding(20).Column(accentColumn =>
            {
                accentColumn.Item().AlignCenter().Text("Issued Date")
                    .FontSize(10)
                    .FontColor("#ffffff")
                    .Bold();

                accentColumn.Item().AlignCenter().PaddingTop(5).Text(Model.IssueDate.ToString(" dd MMM yyyy"))
                    .FontSize(13)
                    .FontColor("#ffffff");
            });
        });
    }

    void ComposeRightSidebar(IContainer container)
    {
        container.Column(rightColumn =>
        {
            rightColumn.Item().Background("#000435").Padding(20).Column(dueDateColumn =>
            {
                dueDateColumn.Item().Text("Due Date")
                    .FontSize(10)
                    .FontColor("#ffffff")
                    .Bold();

                dueDateColumn.Item().PaddingTop(5).Text(Model.DueDate.ToString("dd MMM yyyy"))
                    .FontSize(13)
                    .FontColor("#ffffff")
                    .Bold();
            });

            rightColumn.Item().PaddingTop(20).Background("#111111").Padding(20).Column(paymentColumn =>
            {
                paymentColumn.Item().Text("Payment Details")
                    .FontSize(9)
                    .FontColor("#cccccc")
                    .Bold();

                if (Model.PaymentInformation != null)
                {
                    foreach (var payment in Model.PaymentInformation)
                    {
                        paymentColumn.Item().PaddingTop(10).Column(bankColumn =>
                        {
                            bankColumn.Item().Text(payment.Bank)
                                .FontSize(10)
                                .FontColor("#ffffff")
                                .Bold();
                            bankColumn.Item().Text(payment.AccountName)
                                .FontSize(9)
                                .FontColor("#999999");
                            bankColumn.Item().Text(payment.AccountNumber)
                                .FontSize(11)
                                .FontColor("#ffffff")
                                .Bold();
                        });
                    }
                }
            });

            rightColumn.Item().PaddingTop(20).Element(ComposeTotals);
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Background("#ffffff").Padding(10).Row(headerRow =>
            {
                headerRow.ConstantItem(40).Text("#").FontColor("#0a0a0a").Bold();
                headerRow.RelativeItem(2).Text("Description").FontColor("#0a0a0a").Bold();
                headerRow.RelativeItem(1).AlignCenter().Text("Qty").FontColor("#0a0a0a").Bold();
                headerRow.RelativeItem(3).AlignRight().Text("Unit Price").FontColor("#0a0a0a").Bold();
                headerRow.RelativeItem(3).AlignRight().Text("Amount").FontColor("#0a0a0a").Bold();
            });

            if (Model.Items != null)
            {
                for (int i = 0; i < Model.Items.Count; i++)
                {
                    var item = Model.Items[i];
                    var bgColor = i % 2 == 0 ? "#111111" : "#0a0a0a";

                    column.Item().Background(bgColor).PaddingVertical(10).PaddingHorizontal(15).Row(itemRow =>
                    {
                        itemRow.ConstantItem(40).Text((i + 1).ToString("00")).FontColor("#ffffff");
                        itemRow.RelativeItem(2).Text(item.Description).FontSize(9).FontColor("#ffffff");
                        itemRow.RelativeItem(1).AlignCenter().Text(item.Quantity.ToString()).FontColor("#ffffff");
                        itemRow.RelativeItem(3).AlignRight().Text($"₦{item.UnitPrice:N2}").FontColor("#ffffff");
                        itemRow.RelativeItem(3).AlignRight().Text($"₦{item.Amount:N2}").FontColor("#ffffff").Bold();
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

        container.Column(column =>
        {
            column.Item().Background("#111111").Padding(20).Column(summaryColumn =>
            {
                summaryColumn.Item().Text("Summary")
                    .FontSize(10)
                    .FontColor("#cccccc")
                    .Bold();

                void AddRow(string label, decimal amount, string color = "#ffffff", bool isDiscount = false, bool isTax = false)
                {
                    string formatted;
                    if (isDiscount)
                        formatted = $"-₦{amount:N2}";
                    else if (isTax)
                        formatted = $"+₦{amount:N2}";
                    else
                        formatted = $"₦{amount:N2}";

                    summaryColumn.Item().PaddingTop(8).Column(rowColumn =>
                    {
                        rowColumn.Item().Text(label).FontColor("#cccccc").FontSize(9);
                        rowColumn.Item().PaddingTop(2).Text(formatted)
                            .FontColor(color)
                            .Bold()
                            .FontSize(11);
                    });
                }

                AddRow("Subtotal", subtotal);
                AddRow("Delivery", delivery);
                AddRow("Discount", discount, "#ff6b6b", isDiscount: true);
                AddRow("Tax", taxAmount, "#ffffff", isTax: true);
            });

            column.Item().Background("#000435").Padding(20).Column(totalColumn =>
            {
                totalColumn.Item().Text("TOTAL")
                    .FontSize(11)
                    .FontColor("#ffffff")
                    .Bold();

                totalColumn.Item().PaddingTop(5).Text($"₦{total:N2}")
                    .FontSize(12)
                    .FontColor("#ffffff")
                    .Bold();
            });
        });
    }
}