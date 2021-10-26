using System;
using System.Collections.Generic;
using System.Linq;
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
            if (node is CompilationUnitSyntax compilationUnit)
            {
                HandleCompilationUnit(compilationUnit, context);
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
            else
            {
        //        throw new NotSupportedException($"HandleKind: {node.GetType().Name}");
            }
        }

        void HandleCompilationUnit(CompilationUnitSyntax compilationUnit, ReadingContext context)
        {
            var childNodes = compilationUnit.ChildNodes().ToArray();
            if (childNodes.Length == 1)
            {
                if (childNodes[0].Kind() == SyntaxKind.IncompleteMember)
                {
                    var name = childNodes[0].ToString();
                    if (name != "model")
                    {
                        context.Liquid.AppendFormat("{{{{ {0} }}}}", name);
                    }
                }
            }
            else 
            {
                while (context.Inner.Count > 0)
                {
                    var what = context.Inner.Pop();
                    context.Liquid.AppendLine();
                    context.Liquid.AppendFormat("{{% end{0} %}}", what);
                }
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
 
                var helper = FindHelper(childNode).ToArray();
                HandleCode(childNode, context);
            }
        }


        private void HandleCode(SyntaxNode node, ReadingContext context)
        {
            var transformer = new CodeTransformer(context);
            transformer.TransformNode(node);
        }

        private IEnumerable<SyntaxNode> FindHelper(SyntaxNode node)
        {
            var nodes = node.ChildNodes().ToArray();
            foreach (var childNode in nodes)
            {
                var identifier = childNode.FindFirstChildNode<IdentifierNameSyntax>();
                if (identifier != null)
                {
                    var name = identifier.ToString();
                    if (name == "helper")
                    {
                        yield return childNode;
                    }
                }
            }
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