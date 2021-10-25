using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RazorLiquid.Tests
{
    public class ControlFlowTests : ReaderTests
    {
        readonly ITestOutputHelper _outputHelper;

        public ControlFlowTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void If_should_render_to_if()
        {
            var template = @"
@{ var a = true }
@if (a) {
  <hello>@a</hello>
}
";
            var expected = @"
{% assign a = true %}
{% if a %}
  <hello>{{ a }}</hello>
{% endif %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().Be(expected);
        }

        [Fact]
        public void If_with_more()
        {
            var template = "@if (course.IsBundleItem)";
            var expected = @"{% if course.IsBundleItem %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void If_not_with_more()
        {
            var template = "@if (!course.IsBundleItem)";
            var expected = @"{% unless course.IsBundleItem %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        
        [Fact]
        public void If_with_greater_then()
        {
            var template = "@if (course.IsBundleItem > 0)";
            var expected = @"{% if course.IsBundleItem > 0 %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        [Fact]
        public void If_with_lower_then()
        {
            var template = "@if (course.IsBundleItem < 0)";
            var expected = @"{% if course.IsBundleItem < 0 %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }
        [Fact]
        public void If_assign()
        {
            var template = @"
@{ var priceWidthInPercent = 40; }
@if (course.IsBundleItem) {
  priceWidthInPercent = 30;
  <ding>@priceWidthInPercent</ding>
}";

            var expected = @"
{% assign priceWidthInPercent = 40 %}
{% if course.IsBundleItem %}
{% assign priceWidthInPercent = 30 %}
  <ding>{{ priceWidthInPercent }}</ding>
{% endif %}
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
        }

        [Fact]
        public void IfThenElse()
        {
            var template = @"
                                        @{
                                            string deliveryOptionLabelLocalizationId;
                                            if (course.IsDigital)
                                            {
                                                deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption1Label_Text;
                                            }
                                            else
                                            {
                                                deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption2Label_Text;
                                            }
                                        }";
            var expected = @"
{% assign deliveryOptionLabelLocalizationId = """" %}{% if course.IsDigital %}
{% assign deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption1Label_Text %}
{% else %}
{% assign deliveryOptionLabelLocalizationId = LocalizationKeys.OrderConfirmationEmail.DeliveryOption2Label_Text %}
{% endif %}
";
            
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
            
        }
        
        [Fact]
        public void SimpleForEachWithAnAssign()
        {
            var template = @"
@{
    @foreach (var course in Model.CurrentCart.SortedCourseItems) { 
        var a = course;
    }
}";
            var expected = @"
{% for course in Model.CurrentCart.SortedCourseItems %}
{% assign a = course %}
{% endfor %}
";
            
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));

            result.Should().BeLineEndingNeutral(expected);
            
        }
    }
}