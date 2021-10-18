using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Razor2Liquid
{
    internal class CodeTransformer
    {
        private readonly ReadingContext _context;

        public CodeTransformer(ReadingContext context)
        {
            _context = context;
        }

        public void TransformStatement(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ExpressionStatement:
                    HandleExpression(node);
                    break;
                case SyntaxKind.LocalDeclarationStatement:
                    HandleLocalDeclaration(node);
                    break;
                case SyntaxKind.ForEachStatement:
                    HandleForEach(node);
                    break;
                case SyntaxKind.IfStatement:
                    HandleIf(node);
                    break;
                case SyntaxKind.EmptyStatement:
                    break;
                case SyntaxKind.Block:
                    break;

                default:
                    throw new ArgumentException(nameof(node), $"Node-Kind of {node.Kind()} is not allowed");
            }
        }

        private void HandleIf(SyntaxNode node)
        {
//            throw new NotImplementedException();
        }

        private void HandleForEach(SyntaxNode node)
        {
//            throw new NotImplementedException();
        }

        private void HandleLocalDeclaration(SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes())
            {
                WriteNode(childNode);
            }
        }

        private void HandleExpression(SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes())
            {
                WriteNode(childNode);
            }
        }

        private void StartBars()
        {
            if (_context.BarsCounter == 0)
            {
                _context.Model.Liquid.Append("{{ ");
            }

            _context.BarsCounter++;
        }
        
        private void StartCode()
        {
            if (_context.CodeCounter == 0 && _context.CodeCounter == 0)
            {
                _context.Model.Liquid.Append("{% ");
            }

            _context.CodeCounter++;
        }

        private void EndBars()
        {
            _context.BarsCounter--;

            if (_context.BarsCounter == 0 && _context.CodeCounter == 0)
            {
                _context.Model.Liquid.Append(" }}");
            }

            if (_context.BarsCounter < 0)
            {
                throw new InvalidOperationException($"BarsCounter is {_context.BarsCounter}");
            }

        }
        
        private void EndCode()
        {
            _context.CodeCounter--;

            if (_context.CodeCounter == 0)
            {
                _context.Model.Liquid.Append(" %}");
            }

            if (_context.CodeCounter < 0)
            {
                throw new InvalidOperationException($"CodeCounter is {_context.CodeCounter}");
            }

        }

        private void WriteNode(SyntaxNode node)
        {
            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                WriteMemberAccess(memberAccess);
            }
            else if (node is BinaryExpressionSyntax binary)
            {
                WriteBinary(binary);
            }
            else if (node is LiteralExpressionSyntax literal)
            {
                WriteLiteral(literal);
            }
            else if (node is InvocationExpressionSyntax invocation)
            {
                WriteInvocation(invocation);
            }
            else if (node is VariableDeclarationSyntax variableDeclaration)
            {
                WriteVariableDeclaration(variableDeclaration);
            }
        }

        private void WriteVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
        {
            foreach (var variable in variableDeclaration.Variables)
            {
                StartCode();
                _context.Liquid.Append("assign ");
                _context.Liquid.Append(variable.Identifier);
                _context.Liquid.Append(" = ");
                WriteInitializer(variable.Initializer);
                EndCode();
            }
        }

        private void WriteInitializer(EqualsValueClauseSyntax initializer)
        {
            var invocation = initializer.FindFirstChildNode<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                return;
            }

            if (invocation.ToString().Contains("System.Globalization.CultureInfo.GetCultureInfo"))
            {
                var argument = invocation.ArgumentList.Arguments.First();
                var name = argument.ToString().Replace("\"", "");
                _context.Liquid.AppendFormat("\"---cultureinfo---{0}\"", name);
            }
        }

        private void WriteInvocation(InvocationExpressionSyntax invocation)
        {
            var name = invocation.FindFirstChildNode<IdentifierNameSyntax>();

            if (name == null)
            {
                return;
            }

            if (name.ToString() == "Translate")
            {
                WriteTranslate(invocation);
            }

            if (name.ToString() == "TranslateFormat")
            {
                WriteTranslate(invocation);
            }

            if (name.ToString() == "Raw")
            {
                WriteRaw(invocation);
            }
        }

        private void WriteRaw(InvocationExpressionSyntax invocation)
        {
            var argument = invocation.ArgumentList.Arguments.First();
            if (argument.Expression is LiteralExpressionSyntax literal)
            {
                var content = literal.ToString();
                if (content.StartsWith("\""))
                {
                    _context.Liquid.Append(content.Replace("\"", ""));
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected Contant {content}");
                }
            }
        }

        private void WriteTranslate(InvocationExpressionSyntax invocation)
        {
            StartBars();
            var argument = invocation.ArgumentList.FindFirstChildNode<ArgumentSyntax>();
            if (argument == null)
            {
                throw new InvalidOperationException($"Argument Expression is expected for {invocation}");
            }

            _context.Liquid.Append("\"");
            _context.Liquid.Append(argument);
            _context.Liquid.Append("\"");
            _context.Liquid.Append(" | translate");

            var arguments = invocation.ArgumentList.Arguments.Skip(1).ToArray();
            if (arguments.Length > 0)
            {
                _context.Liquid.Append(": ");
                for (var index = 0; index < arguments.Length; index++)
                {
                    var argumentSyntax = arguments[index];
                    _context.Liquid.Append(argumentSyntax);
                    if (index < arguments.Length - 1)
                    {
                        _context.Liquid.Append(", ");
                    }
                }
            }
            EndBars();
        }

        private void WriteLiteral(LiteralExpressionSyntax literal)
        {
            _context.Liquid.Append(literal);
        }

        private void WriteBinary(BinaryExpressionSyntax binary)
        {
            StartBars();
            WriteNode(binary.Left);
            var kind = binary.OperatorToken.Kind();
            if (kind == SyntaxKind.PlusToken)
            {
                _context.Liquid.AppendFormat(" | append: ");
            }

            WriteNode(binary.Right);
            EndBars();
        }

        private void WriteMemberAccess(MemberAccessExpressionSyntax memberAccess)
        {
            StartBars();
            _context.Model.Liquid.Append(memberAccess.ToString());
            EndBars();
        }
    }
}