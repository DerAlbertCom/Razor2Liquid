using FluentAssertions;
using FluentAssertions.Primitives;

namespace RazorLiquid.Tests
{
    public static class StringAssertionsExtensions
    {
        public static AndConstraint<StringAssertions> BeLineEndingNeutral(this StringAssertions assertions,
            string expected)
        {
            var subject = assertions.Subject.Replace("\r\n", "\n");

            var newAssertion = new StringAssertions(subject);

            expected = expected.Replace("\r\n", "\n");
            

            return newAssertion.Be(expected);
        }
    }
}