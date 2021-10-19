using FluentAssertions;
using Xunit;

namespace RazorLiquid.Tests
{
    public class VariableAssignmentTests : ReaderTests
    {
        [Fact]
        public void Markup_Assignment()
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
    }
}