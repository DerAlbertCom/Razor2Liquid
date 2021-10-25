using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RazorLiquid.Tests
{
    public class HelperTest : ReaderTests
    {
        readonly ITestOutputHelper _outputHelper;

        public HelperTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void Helper_removal()
        {
            var template = @"
<body>
    @ShowBoleto(payment)
    <br/>
 @helper ShowBoleto(Payment payment) {
     <hr />
 }
</body>
";
            var expected = @"
<body>
    {% partial 'Boleto', payment %}
    <br/>
</body>
";
            var result = GetLiquidString(template, (t, args) => _outputHelper.WriteLine(t, args));
            result.Should().BeLineEndingNeutral(expected);
        }

        [Fact]
        public void Helper_one_frompage()
        {
            var template = @"
<body>
    @ShowBoleto(payment)
    <br/>
 @helper ShowBoleto(Payment payment) {
     @Translate(Local.Ding)
     <hr />
 }
</body>
";
            var expected = @"@Translate(Local.Ding)
     <hr />

";
            var result = GetHelper(template, (t, args) => _outputHelper.WriteLine(t, args));
            result.Should().ContainKey("Boleto");

            result.TryGetValue("Boleto", out var value);
            value.Should().BeLineEndingNeutral(expected);
        }

        [Fact]
        public void Helper_two_frompage()
        {
            var template = @"
<body>
    @ShowBoleto(payment)
    <br/>
 @helper ShowBoleto(Payment payment) {
     <hr />
 }
   <h1/>
 @helper ShowWireTransfer(Payment payment) {
     <h2 />
 }

</body>
";
            var expected1 = @"     <hr />

";
            var expected2 = @"     <h2 />

";

            var result = GetHelper(template, (t, args) => _outputHelper.WriteLine(t, args));
            result.Should().ContainKey("Boleto");

            result.TryGetValue("Boleto", out var value);
            value.Should().BeLineEndingNeutral(expected1);

            result.Should().ContainKey("WireTransfer");

            result.TryGetValue("WireTransfer", out value);
            value.Should().BeLineEndingNeutral(expected2);
        }
    }
}