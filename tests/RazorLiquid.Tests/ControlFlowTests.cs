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

            result.Should().Be(expected);
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

            result.Should().Be(expected);
        }
    }
}