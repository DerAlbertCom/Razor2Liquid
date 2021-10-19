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
= Model.CurrentCart.SortedCourseItems.FirstOrDefault() != null ? !string.IsNullOrEmpty(Model.CurrentCart.SortedCourseItems.FirstOrDefault().CustomerNumberForProductVoucherTransfer) : false
{% endcomment %}
";
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
    }
}