using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Razor.Parser.SyntaxTree;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Razor2Liquid
{
    public class ReadingContext
    {
        public LiquidModel Model { get; }
        public ReadingHint Hint { get; set; }

        public StringBuilder Liquid => Model.Liquid;
        public int BarsCounter { get; set; }
        public int CodeCounter { get; set; }
        public string CurrentCulture { get; set; }
        public CSharpSyntaxNode AsComment { get; set; }
        public Stack<string> Inner { get; } = new Stack<string>();
        public SyntaxKind OperatorKind { get; set; } = SyntaxKind.None;
        public int MarkupCounter { get; set; } = 0;
        public SpanKind SpanKind { get; set; }

        public ReadingContext(LiquidModel model)
        {
            Hint = ReadingHint.None;
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}