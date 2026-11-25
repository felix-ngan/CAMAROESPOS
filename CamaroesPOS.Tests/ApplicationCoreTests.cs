using System;
using System.Collections.Generic;
using System.Linq;
using CamaroesPOS.Application.Security;
using CamaroesPOS.Application.Services;
using CamaroesPOS.Domain.Entities;
using Xunit;

namespace CamaroesPOS.Tests;

public class ApplicationCoreTests
{
    [Fact]
    public void PasswordHasher_HashesWithSaltedPbkdf2_AndVerifiesCorrectly()
    {
        string hash1 = PasswordHasher.Hash("S3cr3t!");
        string hash2 = PasswordHasher.Hash("S3cr3t!");

        Assert.True(PasswordHasher.IsHashed(hash1));
        Assert.NotEqual(hash1, hash2); // Unique random salt per hash
        Assert.DoesNotContain("S3cr3t!", hash1);
        Assert.True(PasswordHasher.Verify("S3cr3t!", hash1));
        Assert.False(PasswordHasher.Verify("wrong", hash1));
        Assert.False(PasswordHasher.Verify("", hash1));
    }

    [Fact]
    public void SessionTokenService_ValidatesSignedTokens_AndRejectsTamperedOrExpired()
    {
        var svc = new SessionTokenService("test-secret", TimeSpan.FromHours(1));
        var now = new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero);

        string token = svc.CreateToken("caissier1", "Cashier", now);

        Assert.True(svc.TryValidate(token, out var user, out var role, now.AddMinutes(30)));
        Assert.Equal("caissier1", user);
        Assert.Equal("Cashier", role);

        // Raw username forgery (the v1 exploit) MUST fail
        Assert.False(svc.TryValidate("admin", out _, out _, now));

        // Tampered payload MUST fail
        var parts = token.Split('.');
        string forgedPayload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"admin|Admin|{now.AddHours(1).ToUnixTimeSeconds()}")).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        Assert.False(svc.TryValidate($"{forgedPayload}.{parts[1]}", out _, out _, now));

        // Expired token MUST fail
        Assert.False(svc.TryValidate(token, out _, out _, now.AddHours(2)));
    }

    [Fact]
    public void PosCalculator_ComputesCameroonVatAndDecimalTotalsAccurately()
    {
        var items = new List<CartItem>
        {
            new() { ProductCode = "P1", Price = 11925m, Qty = 1, Total = 11925m, IsTaxable = true },
            new() { ProductCode = "P2", Price = 5000m, Qty = 2, Total = 10000m, IsTaxable = false }
        };

        var totals = PosCalculator.CalculateTotals(items, discountPercent: 0m, taxRatePercent: 19.25m);

        Assert.Equal(21925m, totals.TotalTTC);
        Assert.Equal(1925m, totals.TaxAmount); // 11925 / 1.1925 = 10000 HT -> 1925 TVA
        Assert.Equal(20000m, totals.TotalHT);  // 10000 + 10000 non-taxable
    }

    [Fact]
    public void TransactionNumberGenerator_Generates1000UniqueIdsWithoutCollision()
    {
        var ids = Enumerable.Range(0, 1000)
            .Select(_ => TransactionNumberGenerator.NextTransactionNo())
            .ToHashSet();

        Assert.Equal(1000, ids.Count);
        Assert.All(ids, id => Assert.StartsWith("TX-", id));
    }
}
