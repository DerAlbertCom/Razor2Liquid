using FluentAssertions;
using Xunit;

namespace RazorLiquid.Tests
{
    public class VariableAssignmentTests : ReaderTests
    {
        [Fact]
        public void Complex_Assignment_is_a_comment()
        {
            var template = @"@{
                var isFirstItemTransferedProductVoucher = Model.CurrentCart.SortedCourseItems.FirstOrDefault() != null ? !string.IsNullOrEmpty(Model.CurrentCart.SortedCourseItems.FirstOrDefault().CustomerNumberForProductVoucherTransfer) : false;
            }";

            var result = GetLiquidString(template);

            var expected = @"{% assign isFirstItemTransferedProductVoucher = TODO_COMMENT %}
{% comment %}
---Expression: ConditionalExpressionSyntax ---- From: TransformExpression
Model.CurrentCart.SortedCourseItems.FirstOrDefault() != null ? !string.IsNullOrEmpty(Model.CurrentCart.SortedCourseItems.FirstOrDefault().CustomerNumberForProductVoucherTransfer) : false
{% endcomment %}
 %}";
            result.Should().BeLineEndingNeutral(expected);
        }

        [Fact]
        public void Simple_Assignment_is_direct()
        {
            var template = "@{ var priceWidthInPercent = 40; }";
            var expected = "{% assign priceWidthInPercent = 40 %}";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void No_Assignment_is_string()
        {
            var template = "@{ var priceWidthInPercent;}";
            var expected = "{% assign priceWidthInPercent = \"\" %}";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void Assignment_with_Cast()
        {
            var template = "@{ var payment = (Cws.Shop.Model.Order.BankPayment)Model.CurrentCart.Payment; }";
            var expected = "{% assign payment = Model.CurrentCart.Payment %}";
            var result = GetLiquidString(template);
            result.Should().BeLineEndingNeutral(expected);
            
        }

    }
}