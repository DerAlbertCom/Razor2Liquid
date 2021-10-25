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
    }
}