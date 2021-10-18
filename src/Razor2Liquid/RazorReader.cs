using System;
using System.IO;
using System.Linq;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace Razor2Liquid
{
    public class RazorReader
    {
        public LiquidModel GetLiquidModel(string razorPage)
        {
            var template = File.ReadAllText(razorPage);
            return GetLiquidModel(new StringReader(template));
        }

        public LiquidModel GetLiquidModel(TextReader reader)
        {
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());
            var model = new LiquidModel();
            var context = new ReadingContext(model);
            ParserVisitor visitor = new CallbackVisitor(span => Callback(span, context), error => ErrorCallback(error, context));

            parser.Parse(reader, visitor);
            return model;
        }

        private void ErrorCallback(RazorError error, ReadingContext context)
        {
            context.Model.AddError(new ParseError(error.Location, error.Message));
        }

        private void Callback(Span span, ReadingContext context)
        {
            Console.WriteLine("{0}:{1}", span.Kind, span.Content);

            switch (span.Kind)
            {
                case SpanKind.Transition:
                    break;
                case SpanKind.MetaCode:
                    break;
                // case SpanKind.Comment:
                //     break;
                case SpanKind.Code:
                    HandleCode(span, context);
                    break;
                case SpanKind.Markup:
                    HandleMarkup(span, context);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleCode(Span span, ReadingContext context)
        {
            var codeReader = new CodeReader();
            codeReader.Handle(span, context);
        }

        private void HandleMarkup(Span span, ReadingContext context)
        {
            string markup = span.Content;
            if (context.Hint == ReadingHint.SkipNextMarkupLine)
            {
                markup = RemoveFirstLine(markup);
                context.Hint = ReadingHint.None;
            }

            context.Model.Liquid.Append(markup);
        }

        private string RemoveFirstLine(string markup)
        {
            var parts = markup.Split(separator: new[] {"\r\n", "\n"}, StringSplitOptions.None);
            return string.Join(Environment.NewLine, parts.Skip(1));
        }
    }
}