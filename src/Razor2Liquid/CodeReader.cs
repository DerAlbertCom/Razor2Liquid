using System;
using System.Web.Razor.Parser.SyntaxTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Razor2Liquid
{
    public class CodeReader
    {
        public void Handle(Span span, ReadingContext context)
        {
            var tree = CSharpSyntaxTree.ParseText(span.Content);
            HandleNode(tree.GetRoot(), context);
        }

        private void HandleNode(SyntaxNode node, ReadingContext context)
        {
            if (node is CompilationUnitSyntax compilationUnitSyntax)
            {
                Handle(compilationUnitSyntax, context);
            }
            else
            {
                throw new NotImplementedException($"No Handler for Node Kind {node.Kind()}");
            }
        }

        private void Handle(SyntaxNode node, ReadingContext context)
        {
            HandleKind(node, context);
            foreach (var childNode in node.ChildNodes())
            {
                Handle(childNode, context);
            }
        }

        private void HandleKind(SyntaxNode node, ReadingContext context)
        {
            if (node is CompilationUnitSyntax)
            {
                return;
            }

            if (node is IncompleteMemberSyntax incompleteMemberSyntax)
            {
                HandleIncompleteMember(incompleteMemberSyntax, context);
            }
            else if (node is GlobalStatementSyntax globalStatementSyntax)
            {
                HandleGlobalStatement(globalStatementSyntax, context);
            }
        }

        private void HandleGlobalStatement(GlobalStatementSyntax node, ReadingContext context)
        {
            foreach (var childNode in node.ChildNodes())
            {
                if (FindLayout(childNode, context))
                {
                    continue;
                }
                HandleCode(childNode, context);
            }
        }

        private void HandleCode(SyntaxNode node, ReadingContext context)
        {
            var kind = node.Kind();
            

            var transformer = new CodeTransformer(context);
            transformer.TransformStatement(node);
        }

        private bool FindLayout(SyntaxNode node, ReadingContext context)
        {
            if (node.Kind() != SyntaxKind.ExpressionStatement)
            {
                return false;
            }

            var assignment = node.FindFirstChildNode<AssignmentExpressionSyntax>();

            if (assignment == null)
            {
                return false;
            }

            var identifier = assignment.FindFirstChildNode<IdentifierNameSyntax>();
            var name = assignment.FindFirstChildNode<LiteralExpressionSyntax>();

            if (identifier == null || name == null)
            {
                return false;
            }

            if (identifier.ToString() == "Layout")
            {
                context.Model.Layout = name.ToString().Replace(".cshtml", "").Replace("\"", "");
                context.Liquid.Insert(0, $"{{% layout '{context.Model.Layout}' %}}");
                return true;
            }
            return false;
        }

        private void HandleIncompleteMember(IncompleteMemberSyntax node, ReadingContext context)
        {
            var identifierName = node.FindFirstChildNode<IdentifierNameSyntax>();
            if (identifierName == null)
            {
                throw new InvalidOperationException($"Missing Identifier for IncompleteNode {node}");
            }

            var name = identifierName.ToString();
            if (name == "model")
            {
                context.Hint = ReadingHint.SkipNextMarkupLine;
            }
        }
    }
}