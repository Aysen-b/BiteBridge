using Microsoft.AspNetCore.Mvc;
using BiteBridge.Models;
using System.Text;

namespace BiteBridge.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
public IActionResult Checkout([FromBody] Order order)
{
    if (User.Identity == null || !User.Identity.IsAuthenticated)
    {
        return Unauthorized();
    }

    order.UserEmail = User.Identity.Name ?? "";

    _context.Orders.Add(order);
    _context.SaveChanges();

    _context.EmailNotifications.Add(new EmailNotification
    {
        UserEmail = order.UserEmail,
        Subject = "BiteBridge Order Confirmation",
        Message = $"Your order BB-{order.Id} has been received successfully. Total amount: {order.TotalPrice} TL.",
        OrderId = order.Id,
        IsRead = false
    });

    _context.SaveChanges();

    AddLog("Info", "PaymentSuccess", order.UserEmail, $"Order BB-{order.Id} was created after simulated payment.");
    AddLog("Info", "EmailNotificationCreated", order.UserEmail, $"Order confirmation notification created for order BB-{order.Id}.");

    return Ok();
}

        public IActionResult List()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;

            var orders = _context.Orders
                .Where(o => o.UserEmail == userEmail)
                .OrderByDescending(o => o.Id)
                .ToList();

            var orderIds = orders.Select(o => o.Id).ToList();

            var ratings = _context.Ratings
                .Where(r => orderIds.Contains(r.OrderId) && r.UserEmail == userEmail)
                .ToDictionary(r => r.OrderId, r => r);

            ViewBag.Ratings = ratings;

            return View(orders);
        }

        public IActionResult Delete(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var userEmail = User.Identity.Name;

            var order = _context.Orders
                .FirstOrDefault(o => o.Id == id && o.UserEmail == userEmail);

            if (order != null)
            {
                var relatedRatings = _context.Ratings.Where(r => r.OrderId == order.Id).ToList();

                _context.Ratings.RemoveRange(relatedRatings);
                _context.Orders.Remove(order);
                _context.SaveChanges();

                AddLog("Warning", "OrderDeleted", order.UserEmail, $"Order BB-{order.Id} was deleted.");
            }

            return RedirectToAction("List");
        }

        public IActionResult Rate(int id)
        {
            var order = GetCurrentUserOrder(id);

            if (order == null)
            {
                return NotFound();
            }

            var userEmail = User.Identity?.Name ?? "";

            var existingRating = _context.Ratings
                .FirstOrDefault(r => r.OrderId == id && r.UserEmail == userEmail);

            ViewBag.ExistingRating = existingRating;

            return View(order);
        }

        [HttpPost]
        public IActionResult Rate(int orderId, int score, string comment)
        {
            var order = GetCurrentUserOrder(orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (score < 1 || score > 5)
            {
                score = 5;
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                comment = "No comment provided.";
            }

            var userEmail = User.Identity?.Name ?? "";

            var existingRating = _context.Ratings
                .FirstOrDefault(r => r.OrderId == orderId && r.UserEmail == userEmail);

            if (existingRating == null)
            {
                var rating = new Rating
                {
                    OrderId = orderId,
                    UserEmail = userEmail,
                    Score = score,
                    Comment = comment
                };

                _context.Ratings.Add(rating);

                AddLog("Info", "RatingCreated", userEmail, $"User rated order BB-{orderId} with {score} stars.");
            }
            else
            {
                existingRating.Score = score;
                existingRating.Comment = comment;
                existingRating.CreatedAt = DateTime.Now;

                AddLog("Info", "RatingUpdated", userEmail, $"User updated rating for order BB-{orderId}.");
            }

            _context.SaveChanges();

            return RedirectToAction("List");
        }

        public IActionResult Receipt(int id)
        {
            var order = GetCurrentUserOrder(id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult ReceiptPdf(int id)
        {
            var order = GetCurrentUserOrder(id);

            if (order == null)
            {
                return NotFound();
            }

            AddLog("Info", "ReceiptPdfGenerated", order.UserEmail, $"Receipt PDF generated for order BB-{order.Id}.");

            var lines = new List<string>
            {
                $"Receipt No: BB-{order.Id}",
                $"Customer Email: {order.UserEmail}",
                $"Generated At: {DateTime.Now:dd.MM.yyyy HH:mm}",
                "",
                "Purchased Items:"
            };

            foreach (var item in order.ItemsList)
            {
                lines.Add($"- {item.name} : {item.price} TL");
            }

            lines.Add("");
            lines.Add($"Total Amount: {order.TotalPrice} TL");
            lines.Add("");
            lines.Add("Payment Status: Successful");
            lines.Add("This receipt was generated dynamically by BiteBridge.");

            var pdfBytes = GenerateSimplePdf("BiteBridge Order Receipt", lines);

            return File(pdfBytes, "application/pdf", $"BiteBridge_Receipt_Order_{order.Id}.pdf");
        }

        public IActionResult AgreementPdf(int id)
        {
            var order = GetCurrentUserOrder(id);

            if (order == null)
            {
                return NotFound();
            }

            AddLog("Info", "AgreementPdfGenerated", order.UserEmail, $"Agreement PDF generated for order BB-{order.Id}.");

            var lines = new List<string>
            {
                $"Agreement No: AGR-BB-{order.Id}",
                $"Customer Email: {order.UserEmail}",
                $"Generated At: {DateTime.Now:dd.MM.yyyy HH:mm}",
                "",
                "Agreement Summary:",
                "This agreement confirms that the customer completed a simulated",
                "food order through the BiteBridge catering platform.",
                "",
                "Order Details:"
            };

            foreach (var item in order.ItemsList)
            {
                lines.Add($"- {item.name} : {item.price} TL");
            }

            lines.Add("");
            lines.Add($"Total Amount: {order.TotalPrice} TL");
            lines.Add("");
            lines.Add("Payment Type: Simulation");
            lines.Add("Real card payment was not used.");
            lines.Add("");
            lines.Add("This agreement PDF was generated dynamically with code.");
            lines.Add("BiteBridge");

            var pdfBytes = GenerateSimplePdf("BiteBridge Agreement PDF", lines);

            return File(pdfBytes, "application/pdf", $"BiteBridge_Agreement_Order_{order.Id}.pdf");
        }

        private Order? GetCurrentUserOrder(int id)
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userEmail = User.Identity.Name;

            return _context.Orders
                .FirstOrDefault(o => o.Id == id && o.UserEmail == userEmail);
        }

        private void AddLog(string level, string action, string? userEmail, string? details)
        {
            _context.SystemLogs.Add(new SystemLog
            {
                Level = level,
                Action = action,
                UserEmail = userEmail,
                Details = details
            });

            _context.SaveChanges();
        }

        private byte[] GenerateSimplePdf(string title, List<string> lines)
        {
            var contentBuilder = new StringBuilder();

            contentBuilder.AppendLine("BT");
            contentBuilder.AppendLine("/F1 18 Tf");
            contentBuilder.AppendLine("50 780 Td");
            contentBuilder.AppendLine($"({EscapePdfText(title)}) Tj");
            contentBuilder.AppendLine("0 -35 Td");
            contentBuilder.AppendLine("/F1 11 Tf");

            foreach (var line in lines)
            {
                contentBuilder.AppendLine($"({EscapePdfText(line)}) Tj");
                contentBuilder.AppendLine("0 -18 Td");
            }

            contentBuilder.AppendLine("ET");

            var content = contentBuilder.ToString();

            var objects = new List<string>
            {
                "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n",
                "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n",
                "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n",
                "4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n",
                $"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}endstream\nendobj\n"
            };

            var pdf = new StringBuilder();
            var offsets = new List<int> { 0 };

            pdf.Append("%PDF-1.4\n");

            foreach (var obj in objects)
            {
                offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
                pdf.Append(obj);
            }

            var xrefPosition = Encoding.ASCII.GetByteCount(pdf.ToString());

            pdf.AppendLine("xref");
            pdf.AppendLine($"0 {objects.Count + 1}");
            pdf.AppendLine("0000000000 65535 f ");

            for (int i = 1; i < offsets.Count; i++)
            {
                pdf.AppendLine($"{offsets[i]:D10} 00000 n ");
            }

            pdf.AppendLine("trailer");
            pdf.AppendLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            pdf.AppendLine("startxref");
            pdf.AppendLine(xrefPosition.ToString());
            pdf.AppendLine("%%EOF");

            return Encoding.ASCII.GetBytes(pdf.ToString());
        }

        private string EscapePdfText(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("₺", "TL")
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c")
                .Replace("İ", "I")
                .Replace("Ğ", "G")
                .Replace("Ü", "U")
                .Replace("Ş", "S")
                .Replace("Ö", "O")
                .Replace("Ç", "C");
        }
    }
}