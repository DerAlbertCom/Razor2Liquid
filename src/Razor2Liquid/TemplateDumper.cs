using System;
using System.IO;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Razor2Liquid
{
    class TemplateDumper
    {
        public void Dump(string template)
        {
            var parser = new RazorParser(new CSharpCodeParser(), new HtmlMarkupParser());
            ParserVisitor visitor = new CallbackVisitor(Callback);
            parser.Parse(new StringReader(template), visitor);
        }

        private void Callback(Span obj)
        {
            if (obj.Kind == SpanKind.MetaCode)
            {
                var a = obj.Content;
            }
            Console.WriteLine("{0}:{1}", obj.Kind, obj.Content);
            if (obj.Kind == SpanKind.Code)
            {
                Code(obj.Content);
            }
        }

        private void Code(string objContent)
        {
            var tree = CSharpSyntaxTree.ParseText(objContent);
            WriteNode(tree.GetRoot(), "+");
        }

        private void WriteNode(SyntaxNode node, string prefix)
        {
            Console.WriteLine("{2}{0}:{1}", node.Kind(), node, prefix);
            prefix += "--";
            foreach (var childNode in node.ChildNodes())
            {
                WriteNode(childNode, prefix);
            }
        }
    }
}