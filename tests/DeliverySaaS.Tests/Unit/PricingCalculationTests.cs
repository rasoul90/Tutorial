using DeliverySaaS.Domain.Pricing.Entities;

namespace DeliverySaaS.Tests.Unit;

public class PricingCalculationTests
{
    [Theory]
    [InlineData(1, 3, 30)]
    [InlineData(2, 2, 30)]
    [InlineData(3, 4, 80)]
    [InlineData(4, 1, 25)]
    public void PricingRate_CalculationBySize_ReturnsExpectedTotal(int size, int quantity, decimal expected)
    {
        var rate = new PricingRate { Size1Rate = 10, Size2Rate = 15, Size3Rate = 20, Size4Rate = 25 };
        var total = ResolveRate(rate, size) * quantity;
        Assert.Equal(expected, total);
    }

    private static decimal ResolveRate(PricingRate rate, int size) => size switch
    {
        1 => rate.Size1Rate,
        2 => rate.Size2Rate,
        3 => rate.Size3Rate,
        4 => rate.Size4Rate,
        _ => throw new InvalidOperationException("Unsupported size")
    };
}
