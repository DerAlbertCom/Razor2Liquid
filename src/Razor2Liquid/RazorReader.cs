using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;

namespace Razor2Liquid
{
    public class RazorReader
    {
        private readonly Action<string, object[]> _console;

        public RazorReader(Action<string, object[]> console = null)
        {
            _console = console;
        }


        public LiquidModel GetLiquidModel(string fileName)
        {
            var template = File.ReadAllText(fileName);
            return GetLiquidModel(new StringReader(template));
        }

        public LiquidModel GetLiquidModel(TextReader reader)
        {
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());
            var model = new LiquidModel();
            var context = new ReadingContext(model);
            ParserVisitor visitor =
                new CallbackVisitor(span => CallbackParser(span, context), error => ErrorCallback(error, context));

            parser.Parse(reader, visitor);
            return model;
        }


        public IDictionary<string, string> GetHelpers(string fileName)
        {
            var template = File.ReadAllText(fileName);
            return GetHelpers(new StringReader(template));
        }

        public IDictionary<string, string> GetHelpers(TextReader reader)
        {
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());
            var model = new LiquidModel();
            var context = new ReadingContext(model);
            _inHelper = false;
            _bracesCount = 0;
            _helperLine.Clear();
            _helperName = string.Empty;
            _inPrefix = string.Empty;

            ParserVisitor visitor =
                new CallbackVisitor(span => CallbackHelper(span, context), error => ErrorCallback(error, context));

            parser.Parse(reader, visitor);
            return _helpers;
        }

        private void ErrorCallback(RazorError error, ReadingContext context)
        {
            context.Model.AddError(new ParseError(error.Location, error.Message));
        }

        private void CallbackHelper(Span span, ReadingContext context)
        {
            _console?.Invoke("RazorReader= {0}:{1}", new object[] { span.Kind, span.Content });

            var content = span.Content;
            switch (span.Kind)
            {
                case SpanKind.Transition:
                    _inPrefix = span.Content;
                    break;
                case SpanKind.MetaCode:
                {
                    if (content.Contains("helper"))
                    {
                        _inHelper = true;
                    }
                }

                    break;
                case SpanKind.Comment:
                    break;
                case SpanKind.Code:
                {
                    if (_inHelper)
                    {
                        if (_bracesCount > 0)
                        {
                            var code = span.Content.Trim();
                            if (!string.IsNullOrWhiteSpace(code))
                            {
                                _helperLine.Add($"{_inPrefix}{span.Content}");
                                _inPrefix = string.Empty;
                            }
                        }
                        else
                        {
                            if (span.Content.StartsWith("Show"))
                            {
                                var index = span.Content.IndexOf('(');
                                _helperName = span.Content.Substring(4, index - 4);
                            }
                        }

                        LineHelpers(span);
                    }
                }
                    break;
                case SpanKind.Markup:
                {
                    if (_inHelper)
                    {
                        if (_bracesCount > 0)
                        {
                            _helperLine.Add(span.Content);
                        }

                        LineHelpers(span);
                    }
                }

                    break;
                    default:
                    throw new ArgumentOutOfRangeException();
            }

            void LineHelpers(Span s)
            {
                var add = s.Content.Count(c => c == '{');
                var sub = s.Content.Count(c => c == '}');
                _bracesCount += add;
                _bracesCount -= sub;
                if (_bracesCount == 0)
                {
                    _helperLine.RemoveAt(_helperLine.Count - 1); // remove last }
                    _inHelper = false;
                    if (!string.IsNullOrWhiteSpace(_helperName))
                    {
                        var template = string.Join(Environment.NewLine, _helperLine);
                        _helpers[_helperName] = template;
                    }

                    _helperName = null;
                    _helperLine.Clear();
                }
            }
        }

        private bool _inHelper = false;
        private int _bracesCount = 0;
        private string _helperName = "";
        private readonly IList<string> _helperLine = new List<string>();
        private readonly IDictionary<string, string> _helpers = new Dictionary<string, string>();
        private string _inPrefix = string.Empty;

        private void CallbackParser(Span span, ReadingContext context)
        {
            _console?.Invoke("RazorReader= {0}:{1}", new object[] { span.Kind, span.Content });

            var content = span.Content;
            switch (span.Kind)
            {
                case SpanKind.Transition:
                    break;
                case SpanKind.MetaCode:
                {
                    if (content.Contains("helper"))
                    {
                        _inHelper = true;
                    }
                }

                    break;
                case SpanKind.Comment:
                    break;
                case SpanKind.Code:
                {
                    if (!_inHelper)
                    {
                        HandleCode(span, context);
                    }
                    else
                    {
                        CheckHelper(span);
                    }
                }
                    break;
                case SpanKind.Markup:
                    if (!_inHelper)
                    {
                        HandleMarkup(span, context);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            void CheckHelper(Span s)
            {
                var add = s.Content.Count(c => c == '{');
                var sub = s.Content.Count(c => c == '}');
                _bracesCount += add;
                _bracesCount -= sub;
                if (_bracesCount == 0)
                {
                    _inHelper = false;
                }
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
            var parts = markup.Split(separator: new[] { "\r\n", "\n" }, StringSplitOptions.None);
            return string.Join(Environment.NewLine, parts.Skip(1));
        }
    }
}