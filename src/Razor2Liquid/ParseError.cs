using System;
using System.Web.Razor.Text;

namespace Razor2Liquid
{
    public class ParseError
    {
        public ParseError(SourceLocation location, string message)
        {
            Location = location;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public SourceLocation Location { get; }
        public string Message { get; }
    }
}