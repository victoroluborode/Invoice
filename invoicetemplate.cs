// using QuestPDF.Drawing;
// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;


// public class InvoiceDocument : IDocument
// {
//     public InvoiceModel Model { get; }

//     public InvoiceDocument(InvoiceModel model)
//     {
//         Model = model;
//     }

//     public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
//     public DocumentSettings GetSettings() => DocumentSettings.Default;

//     public void Compose(IDocumentContainer container)
//     {
//         container
//             .Page(page =>
//             {
//                 page.Margin(30);
//                 page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Plus Jakarta Sans"));
//                 page.Header().Element(ComposeHeader);
//                 page.Content().Element(ComposeContent);
//                 page.Footer().AlignCenter().Text(x =>
//                 {
//                     x.CurrentPageNumber();
//                     x.Span(" / ");
//                     x.TotalPages();
//                 });
//             });
//     }

//     void ComposeHeader(IContainer container)
//     {
//         container
//         .Background("#000000")
//         .Padding(15)
//         .Column(column =>
//         {
//             column.Item().PaddingTop(20).Text("Invoice").FontSize(24).FontColor("#FFFFFF").Bold();
//             column.Item().PaddingTop(10).Text($"#{Model.InvoiceNumber}").FontSize(10).FontColor(Colors.Grey.Lighten2);
//         });
//     }

//     void ComposeContent(IContainer container)
//     {
//         container
//         .Padding(30)
//         .Column(column =>
//         {
//             column.Item().Element(ComposeDetails);
//             column.Item().PaddingTop(30).Element(ComposeTable);
//             column.Item().PaddingTop(20).Element(ComposeTotals);
//             if (!string.IsNullOrWhiteSpace(Model.AdditionalInformation))
//                 column.Item().PaddingTop(25).Element(ComposeAdditionalInformation);
//         });
//     }

//     void ComposeDetails(IContainer container)
//     {
//         container
//         .Column(column =>
//         {
//             column.Item().Element(c => ComposeDetailRow(c, "Billed To", Model.CustomerName));
//             column.Item().Element(c => ComposeDetailRow(c, "Issued Date", Model.IssueDate.ToString("dddd, MMMM d, yyyy")));
//             column.Item().Element(c => ComposeDetailRow(c, "Due Date", Model.DueDate.ToString("dddd, MMMM d, yyyy")));
//             var paymentInfo = Model.PaymentInformation?.FirstOrDefault();
//             var paymentText = paymentInfo != null ? $"{paymentInfo.Bank}, {paymentInfo.AccountName}, {paymentInfo.AccountNumber}" : string.Empty;
//             if (!string.IsNullOrEmpty(paymentText))
//             {
//                 column.Item().Element(c => ComposeDetailRow(c, "Payment information", paymentText));
//             }
//         });
//     }

//     void ComposeTable(IContainer container)
//     {
//         container.Table(table =>
//         {
//             table.ColumnsDefinition(columns =>
//             {
//                 columns.ConstantColumn(50);
//                 columns.RelativeColumn(2);
//                 columns.RelativeColumn();
//                 columns.RelativeColumn();
//                 columns.RelativeColumn(2);
//             });

//             table.Header(header =>
//             {
//                 header.Cell().Element(CellStyle).Text("#");
//                 header.Cell().Element(CellStyle).Text("DESCRIPTION");
//                 header.Cell().Element(CellStyle).Text("QTY");
//                 header.Cell().Element(CellStyle).AlignRight().Text("UNIT PRICE");
//                 header.Cell().Element(CellStyle).AlignRight().Text("AMOUNT");

//                 static IContainer CellStyle(IContainer container)
//                 {
//                     return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
//                 }
//             });

//             if (Model.Items != null)
//             {
//                 foreach (var item in Model.Items)
//                 {
//                     table.Cell().Element(CellStyle).Text((Model.Items.IndexOf(item) + 1).ToString());
//                     table.Cell().Element(CellStyle).Text(item.Description);
//                     table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
//                     table.Cell().Element(CellStyle).AlignRight().Text($"₦{item.UnitPrice:N2}");
//                     table.Cell().Element(CellStyle).AlignRight().Text($"₦{item.Amount:N2}");

//                     static IContainer CellStyle(IContainer container)
//                     {
//                         return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
//                     }
//                 }
//             }
//         });
//     }

//     void ComposeTotals(IContainer container)
//     {
//         var subtotal = Model.Items?.Sum(x => x.Amount) ?? 0;
//         var delivery = Model.DeliveryFee;
//         var discount = Model.Discount;
//         var taxRate = Model.TaxRate;
//         var taxAmount = subtotal * (taxRate / 100);
//         var total = subtotal + delivery - discount + taxAmount;

//         container
//         .AlignRight()
//         .Width(250)
//         .Column(column =>
//         {
//             column.Item().Element(c => ComposeTotalRow(c, "SUBTOTAL", $"₦{subtotal:N2}", false));
//             column.Item().Element(c => ComposeTotalRow(c, "DELIVERY", $"₦{delivery:N2}", false));
//             column.Item().Element(c => ComposeTotalRow(c, "DISCOUNT", $"-₦{discount:N2}", false));
//             column.Item().AlignRight().Element(c => ComposeTotalRow(c, "TAX", $"{taxRate}%, +₦{taxAmount:N2}", false));
//             column.Item().PaddingTop(5).AlignRight().Element(c => ComposeTotalRow(c, "TOTAL", $"₦{total:N2}", true));
//         });
//     }

//     void ComposeTotalRow(IContainer container, string label, string amount, bool isTotal)
//     {
//         container
//             .PaddingBottom(8)
//             .Row(row =>
//             {
//                 row.RelativeItem()
//                     .Text(label)
//                     .FontSize(isTotal ? 12 : 9)
//                     .FontColor(isTotal ? Colors.Black : Colors.Black)
//                     .Bold();

//                 row.RelativeItem()
//                     .AlignRight()
//                     .Text(amount)
//                     .FontSize(isTotal ? 16 : 10)
//                     .FontColor(isTotal ? Colors.Blue.Medium : Colors.Black)
//                     .Bold();
//             });
//     }
//     void ComposeAdditionalInformation(IContainer container)
//     {
//         container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
//         {
//             column.Spacing(5);
//             column.Item().Text(Model.AdditionalInformation);
//         });
//     }

//     void ComposeDetailRow(IContainer container, string label, string value)
//     {
//         container
//             .PaddingBottom(12)
//             .Row(row =>
//             {
//                 row.ConstantItem(150)
//                     .Text(label)
//                     .FontSize(12)
//                     .FontColor(Colors.Grey.Medium);

//                 row.RelativeItem()
//                     .Text(value)
//                     .FontSize(12)
//                     .FontColor(Colors.Black)
//                     .Medium();
//             });
//     }
// }
