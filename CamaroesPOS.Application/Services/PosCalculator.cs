using System.Globalization;
using CamaroesPOS.Domain.Entities;

namespace CamaroesPOS.Application.Services;

public record CartTotals(
    decimal SubTotalTTC,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TotalTTC,
    decimal TotalHT,
    decimal TaxAmount,
    decimal TaxRatePercent);

public static class PosCalculator
{
    public const decimal DefaultCameroonVatRate = 19.25m;

    public static decimal ClampDiscount(decimal discountPercent)
    {
        if (discountPercent < 0m) return 0m;
        if (discountPercent > 100m) return 100m;
        return decimal.Round(discountPercent, 2, MidpointRounding.AwayFromZero);
    }

    public static bool TryParseDecimalInput(string? raw, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var normalized = raw.Trim().Replace(" ", "").Replace("\u00A0", "").Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    public static decimal RoundMoney(decimal amount)
        => decimal.Round(amount, 0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Computes Cameroon POS totals from TTC catalog prices (standard retail pricing in FCFA).
    /// Extracts HT and TVA (e.g. 19.25%) cleanly using decimal precision.
    /// </summary>
    public static CartTotals CalculateTotals(IEnumerable<CartItem> items, decimal discountPercent, decimal taxRatePercent = DefaultCameroonVatRate)
    {
        decimal discount = ClampDiscount(discountPercent);
        decimal taxRate = taxRatePercent < 0m ? 0m : taxRatePercent;

        decimal subTotalTtc = 0m;
        decimal taxableNetTtc = 0m;
        decimal nonTaxableNetTtc = 0m;

        foreach (var item in items)
        {
            decimal lineGross = item.Price * item.Qty;
            subTotalTtc += lineGross;
            decimal lineDiscount = RoundMoney(lineGross * (discount / 100m));
            decimal lineNet = lineGross - lineDiscount;

            if (item.IsTaxable)
                taxableNetTtc += lineNet;
            else
                nonTaxableNetTtc += lineNet;
        }

        decimal totalTtc = taxableNetTtc + nonTaxableNetTtc;
        decimal totalDiscount = subTotalTtc - totalTtc;

        decimal taxableHt = taxRate > 0m
            ? RoundMoney(taxableNetTtc / (1m + (taxRate / 100m)))
            : taxableNetTtc;
        decimal taxAmount = taxableNetTtc - taxableHt;
        decimal totalHt = taxableHt + nonTaxableNetTtc;

        return new CartTotals(
            SubTotalTTC: subTotalTtc,
            DiscountPercent: discount,
            DiscountAmount: totalDiscount,
            TotalTTC: totalTtc,
            TotalHT: totalHt,
            TaxAmount: taxAmount,
            TaxRatePercent: taxRate);
    }

    public static (decimal LineDiscount, decimal LineTotalTTC, decimal LineHT, decimal LineTax) CalculateLineBreakdown(
        CartItem item,
        decimal discountPercent,
        decimal taxRatePercent = DefaultCameroonVatRate)
    {
        decimal discount = ClampDiscount(discountPercent);
        decimal taxRate = taxRatePercent < 0m ? 0m : taxRatePercent;
        decimal gross = item.Price * item.Qty;
        decimal lineDiscount = RoundMoney(gross * (discount / 100m));
        decimal netTtc = gross - lineDiscount;

        if (!item.IsTaxable || taxRate <= 0m)
            return (lineDiscount, netTtc, netTtc, 0m);

        decimal ht = RoundMoney(netTtc / (1m + (taxRate / 100m)));
        decimal tax = netTtc - ht;
        return (lineDiscount, netTtc, ht, tax);
    }

    public static int CalculateLoyaltyPoints(decimal netAmount)
        => netAmount <= 0m ? 0 : (int)(netAmount / 1000m);
}
