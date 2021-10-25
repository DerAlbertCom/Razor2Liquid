using FluentAssertions;
using Xunit;

namespace RazorLiquid.Tests
{
    public class SimplePrintTest : ReaderTests
    {
        [Fact]
        public void First_element_of_an_array()
        {
            var template = "@Model.Array[0]";
            var expected = "{{ Model.Array | first }}";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }

        [Fact]
        public void Raw_should_be_escaped()
        {
            var template = "<p>@Raw(payment.ChequeAddress)</p>";
            var expected = "<p>{{ payment.ChequeAddress | raw }}</p>";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void Raw_with_function()
        {
            var template = "<b>@Raw(GetFormattedPrice(course.TotalPrice, course.TotalDiscountedPrice, Model.CurrentCart.Currency, course.HideStrikethrough))</b>";
            var expected = "<b>{{ course.TotalPrice | format_price: course.TotalDiscountedPrice, Model.CurrentCart.Currency, course.HideStrikethrough | raw }}</b>";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void Boleto_to_partial()
        {
            var template = "<b>@ShowBoleto(ding.Dong)</b>";
            var expected = "<b>{% partial 'Boleto', ding.Dong %}</b>";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void WireTransfer_to_partial()
        {
            var template = "<b>@ShowWireTransfer(ding.Dong, blub)</b>";
            var expected = "<b>{% partial 'WireTransfer', ding.Dong, blub %}</b>";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }
    }
}