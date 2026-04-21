using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using PhoneBillManager.Api.Services.Interfaces;

namespace PhoneBillManager.Api.Services;

public class BillParserService : IBillParserService
{
    private static readonly Regex PhoneRegex = new(@"\(?\d{3}\)?[\s\-]?\d{3}[\s\-]?\d{4}", RegexOptions.Compiled);
    private static readonly Regex AmountRegex = new(@"\$?([\d,]+\.\d{2})", RegexOptions.Compiled);

    public Task<ParsedBillData> ParseAsync(Stream pdfStream)
    {
        var result = new ParsedBillData();
        var allLines = ExtractTextLines(pdfStream);

        var section = "NONE";
        var subSection = "";
        string? currentPhone = null;
        decimal totalPlanAmount = 0;
        var rawLineData = new List<(string Phone, decimal Equipment, decimal Services)>();

        foreach (var line in allLines)
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (result.BillingPeriod == null && Regex.IsMatch(trimmed, @"(billing period|bill date|statement date)", RegexOptions.IgnoreCase))
                result.BillingPeriod = trimmed;

            if (result.AccountNumber == null && Regex.IsMatch(trimmed, @"account\s*(number|#|no)", RegexOptions.IgnoreCase))
            {
                var num = Regex.Match(trimmed, @"\d{6,}");
                if (num.Success) result.AccountNumber = num.Value;
            }

            var upper = trimmed.ToUpperInvariant();

            // Detect THIS BILL SUMMARY section
            if (upper.Contains("THIS BILL SUMMARY"))
            {
                section = "BILL_SUMMARY";
                continue;
            }

            if (section == "BILL_SUMMARY")
            {
                // Exit when hitting next major section
                if (upper.Contains("DETAILED CHARGES") || upper == "PLANS" || upper.StartsWith("PLAN CHARGES") ||
                    upper == "EQUIPMENT" || upper.StartsWith("EQUIPMENT CHARGES") ||
                    upper == "SERVICES" || upper.StartsWith("SERVICE CHARGES"))
                {
                    section = "NONE";
                    // Fall through to handle this line in the sections below
                }
                else
                {
                    // Parse Totals row to get total plan amount
                    // e.g. "Totals $324.00 $88.75 $0.00 $412.75"
                    if (upper.StartsWith("TOTALS"))
                    {
                        var totalsAmounts = Regex.Matches(trimmed, @"\$?([\d,]+\.\d{2})");
                        if (totalsAmounts.Count >= 1)
                            decimal.TryParse(totalsAmounts[0].Groups[1].Value.Replace(",", ""), out totalPlanAmount);
                        continue;
                    }

                    // Skip header and account-level rows
                    if (upper.StartsWith("LINE TYPE") || upper.StartsWith("ACCOUNT "))
                        continue;

                    // Parse Voice lines: "(769) 972-0487 Voice Included - - $0.00"
                    var phoneMatch = PhoneRegex.Match(trimmed);
                    if (phoneMatch.Success && trimmed.Contains("Voice", StringComparison.OrdinalIgnoreCase))
                    {
                        var phone = NormalizePhone(phoneMatch.Value);

                        // Get text after the phone number then skip "Voice"
                        var afterPhone = trimmed.Substring(phoneMatch.Index + phoneMatch.Length).Trim();
                        var voiceIdx = afterPhone.IndexOf("Voice", StringComparison.OrdinalIgnoreCase);
                        if (voiceIdx >= 0)
                            afterPhone = afterPhone.Substring(voiceIdx + 5).Trim();

                        // Remaining tokens: [Plans, Equipment, Services, Total]
                        // Plans = "Included" or "$xx.xx"
                        // Equipment = "-" or "$xx.xx"
                        // Services = "-" or "$xx.xx"
                        var tokens = afterPhone.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        decimal equipment = 0, services = 0;

                        // Equipment is at index 1 (after Plans at index 0)
                        if (tokens.Length >= 2 && tokens[1] != "-")
                        {
                            var v = tokens[1].TrimStart('$').Replace(",", "");
                            decimal.TryParse(v, out equipment);
                        }

                        // Services is at index 2
                        if (tokens.Length >= 3 && tokens[2] != "-")
                        {
                            var v = tokens[2].TrimStart('$').Replace(",", "");
                            decimal.TryParse(v, out services);
                        }

                        if (!result.PhoneNumbers.Contains(phone))
                            result.PhoneNumbers.Add(phone);

                        rawLineData.Add((phone, equipment, services));
                    }
                    continue;
                }
            }

            // Section detection
            if (upper == "PLANS" || upper.StartsWith("PLAN CHARGES") || upper == "MONTHLY PLAN")
            {
                section = "PLANS"; subSection = ""; currentPhone = null; continue;
            }
            if (upper == "EQUIPMENT" || upper.StartsWith("EQUIPMENT CHARGES"))
            {
                section = "EQUIPMENT"; subSection = ""; currentPhone = null; continue;
            }
            if (upper == "HANDSETS" || upper.StartsWith("HANDSET"))
            {
                subSection = "HANDSETS"; continue;
            }
            if (upper == "SERVICES" || upper.StartsWith("SERVICE CHARGES") || upper.StartsWith("ADDITIONAL CHARGES"))
            {
                section = "SERVICES"; subSection = ""; currentPhone = null; continue;
            }

            var phoneMatchOther = PhoneRegex.Match(trimmed);
            if (phoneMatchOther.Success && (section == "EQUIPMENT" || section == "SERVICES"))
                currentPhone = NormalizePhone(phoneMatchOther.Value);

            var amountMatch = AmountRegex.Match(trimmed);
            if (!amountMatch.Success) continue;
            if (!decimal.TryParse(amountMatch.Groups[1].Value.Replace(",", ""), out var amount)) continue;

            var chargeName = trimmed[..amountMatch.Index].Trim().TrimEnd('-', ' ');
            if (string.IsNullOrWhiteSpace(chargeName)) chargeName = "Charge";

            switch (section)
            {
                case "PLANS":
                    result.PlanCharges.Add((chargeName, amount, "Plan"));
                    break;
                case "EQUIPMENT":
                    if (subSection == "HANDSETS" && currentPhone != null)
                        result.EquipmentCharges.Add((currentPhone, null, chargeName, amount, "Device Payment"));
                    else if (currentPhone != null)
                        result.EquipmentCharges.Add((currentPhone, null, chargeName, amount, "Equipment"));
                    break;
                case "SERVICES":
                    if (currentPhone != null)
                        result.ServiceCharges.Add((currentPhone, chargeName, amount, "Service"));
                    break;
            }
        }

        // Calculate per-line charges: plan per line = total plan / number of voice lines
        if (rawLineData.Count > 0)
        {
            var planPerLine = Math.Round(totalPlanAmount / rawLineData.Count, 2);
            result.TotalPlanAmount = totalPlanAmount;

            foreach (var (phone, equipment, services) in rawLineData)
            {
                var total = Math.Round(planPerLine + equipment + services, 2);
                result.LineCharges.Add((phone, planPerLine, equipment, services, total));
            }
        }

        return Task.FromResult(result);
    }

    public Task<List<string>> GetDebugLinesAsync(Stream pdfStream)
    {
        var allLines = ExtractTextLines(pdfStream);
        return Task.FromResult(allLines);
    }

    private static List<string> ExtractTextLines(Stream pdfStream)
    {
        var lines = new List<string>();
        using var pdf = PdfDocument.Open(pdfStream);
        foreach (var page in pdf.GetPages())
        {
            var words = page.GetWords();
            var byY = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
                .OrderByDescending(g => g.Key);

            foreach (var row in byY)
            {
                var lineText = string.Join(" ", row.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text));
                lines.Add(lineText);
            }
        }
        return lines;
    }

    private static string NormalizePhone(string raw)
    {
        var digits = Regex.Replace(raw, @"\D", "");
        if (digits.Length == 10)
            return $"({digits[..3]}) {digits[3..6]}-{digits[6..]}";
        return raw;
    }
}
