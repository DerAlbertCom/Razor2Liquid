using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
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
                    throw new ArgumentException(nameof(node), $"Node-Kind of {node.Kind()} is not allowed {node}");
            }
        }

        private void HandleIf(SyntaxNode node)
        {
            if (node is IfStatementSyntax ifSyntax)
            {
                StartCode();
                _context.Liquid.Append("if ");
                var old = _context.Hint;
                _context.Hint = ReadingHint.Expression;
                WriteNode(ifSyntax.Condition);
                EndCode();
                WriteNode(ifSyntax.Statement);
                _context.Hint = old;
                _context.Liquid.AppendLine();
                _context.Inner.Push("if");
                return;
            }

            WriteAsComment(node.ToString());
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

            if (_context.AsComment != null)
            {
                WriteAsComment(_context.AsComment.ToString());
                _context.AsComment = null;
            }

            if (_context.CodeCounter < 0)
            {
                throw new InvalidOperationException($"CodeCounter is {_context.CodeCounter}");
            }
        }

        void WriteAsComment(string value, Type getType = null)
        {
            _context.Model.Liquid.AppendLine();
            _context.Model.Liquid.AppendLine("{% comment %}");
            if (getType != null)
            {
                _context.Liquid.AppendFormat("---Expression: {0} ----", getType.Name);
                _context.Liquid.AppendLine();
            }

            _context.Model.Liquid.AppendLine(value);
            _context.Model.Liquid.AppendLine("{% endcomment %}");
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
            else if (node is ExpressionStatementSyntax expressionStatement)
            {
                WriteExpressionStatement(expressionStatement);
            }
            else if (node is IdentifierNameSyntax identifierName)
            {
                WriteIdentifierName(identifierName);
            }
            else if (node is BlockSyntax block)
            {
                WriteBlock(block);
            }
            else if (ShouldAsComment(node))
            {
                WriteAsComment(node.ToString(), node.GetType());
            }
            else if (node is LocalDeclarationStatementSyntax localDeclarationStatement)
            {
                WriteAsComment(node.ToString(), node.GetType());
            }
            else
            {
                throw new NotSupportedException($"{node.GetType().Name} is not supported {node.ToString()}");
            }
        }

        void WriteExpressionStatement(ExpressionStatementSyntax expressionStatement)
        {
            if (expressionStatement.Expression is AssignmentExpressionSyntax ae)
            {
                _context.Liquid.AppendLine();
                _context.Liquid.Append("{% assign ");
                WriteExpression(ae.Left);
                _context.Liquid.Append(" = ");
                WriteExpression(ae.Right);
                _context.Liquid.Append(" %}");
            }
        }

        void WriteExpression(ExpressionSyntax expression)
        {
            _context.Liquid.Append(expression);
        }

        bool ShouldAsComment(SyntaxNode node)
        {
            if (node is PrefixUnaryExpressionSyntax)
            {
                return true;
            }

            if (node is ElementAccessExpressionSyntax)
            {
                return true;
            }

            if (node is LocalDeclarationStatementSyntax)
            {
                return true;
            }

            if (node is IfStatementSyntax)
            {
                return true;
            }

            return false;
        }

        void WriteBlock(BlockSyntax block)
        {
            var blocks = block.ChildNodes().ToArray();
            foreach (var node in blocks)
            {
                WriteNode(node);
            }
        }

        void WriteIdentifierName(IdentifierNameSyntax identifierName)
        {
            _context.Liquid.Append(identifierName);
        }

        private void WriteVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
        {
            foreach (var variable in variableDeclaration.Variables)
            {
                StartCode();
                if (IsCultureInfo(variable.Initializer))
                {
                    WriteCulture(variable);
                }
                else
                {
                    _context.Liquid.Append("assign ");
                    _context.Liquid.Append(variable.Identifier);
                    _context.Liquid.Append(" ");
                    if (IsSimple(variable.Initializer))
                    {
                        _context.Liquid.Append(variable.Initializer);
                    }
                    else
                    {
                        _context.Liquid.Append("= TODO_COMMENT");
                        _context.AsComment = variable.Initializer;
                    }
                }

                EndCode();
            }
        }

        bool IsSimple(EqualsValueClauseSyntax variableInitializer)
        {
            return variableInitializer?.Value is LiteralExpressionSyntax;
        }

        bool IsCultureInfo(EqualsValueClauseSyntax syntax)
        {
            var invocation = syntax.FindFirstChildNode<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                return false;
            }

            return invocation.ToString().Contains("System.Globalization.CultureInfo.GetCultureInfo");
        }

        private void WriteCulture(VariableDeclaratorSyntax variable)
        {
            var initializer = variable.Initializer;
            var invocation = initializer.FindFirstChildNode<InvocationExpressionSyntax>();
            if (invocation == null)
            {
                throw new InvalidOperationException("initializer is not CultureInfo");
            }

            if (invocation.ToString().Contains("System.Globalization.CultureInfo.GetCultureInfo"))
            {
                if (!string.IsNullOrWhiteSpace(_context.CurrentCulture))
                {
                    throw new InvalidOperationException("Only One Culture is allowed");
                }

                _context.CurrentCulture = variable.Identifier.ToString();
                var argument = invocation.ArgumentList.Arguments.First();
                var name = argument.ToString().Replace("\"", "");
                _context.Liquid.AppendFormat("culture '{0}'", name);
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
                    throw new InvalidOperationException($"Unexpected Content {content}");
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

            var arguments = invocation.ArgumentList.Arguments.Where(NoCultureInfoFilter).Skip(1).ToArray();
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

        bool NoCultureInfoFilter(ArgumentSyntax arg)
        {
            var name = arg.ToString();
            if (string.Equals(name, _context.CurrentCulture))
            {
                return false;
            }

            return true;
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
            if (_context.Hint != ReadingHint.Expression)
            {
                StartBars();
                _context.Model.Liquid.Append(memberAccess.ToString());
                EndBars();
            }
            else
            {
                _context.Model.Liquid.Append(memberAccess.ToString());
            }
        }
    }
}