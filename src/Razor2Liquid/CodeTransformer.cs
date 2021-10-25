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

        public void TransformNode(SyntaxNode node)
        {
            if (node is ExpressionSyntax expressionSyntax)
            {
                TransformExpression(expressionSyntax);
                return;
            }
            else if (node is CSharpSyntaxNode statementSyntax)
            {
                TransformCSharpSyntax(statementSyntax);
                return;
            }

            throw new NotSupportedException(
                $"TransformNode: {node.GetType().Name} is not supported for {node.ToString()}");
        }

        void TransformCSharpSyntax(CSharpSyntaxNode statementSyntax)
        {
            var name = statementSyntax.FindFirstChildNode<IdentifierNameSyntax>();
            if (name != null)
            {
                var identifier = name.ToString();
            }
            switch (statementSyntax)
            {
                case ExpressionStatementSyntax expressionStatementSyntax:
                    HandleExpressionsStatement(expressionStatementSyntax);
                    break;
                case LocalDeclarationStatementSyntax localDeclarationStatement:
                    HandleLocalDeclaration(localDeclarationStatement);
                    break;
                case VariableDeclarationSyntax variableDeclaration:
                    WriteVariableDeclaration(variableDeclaration);
                    break;
                case ForEachStatementSyntax forEachStatement:
                    WriteForEach(forEachStatement);
                    break;
                case IfStatementSyntax ifStatement:
                    WriteIf(ifStatement);
                    break;
                case EqualsValueClauseSyntax equalsValueClause:
                    WriteEqualsValueClause(equalsValueClause);
                    break;
                case BracketedArgumentListSyntax bracketedArgumentList:
                    WriteBracketedArgumentList(bracketedArgumentList);
                    break;
                case EmptyStatementSyntax emptyStatement:
                    break;
                case BlockSyntax block:
                    WriteBlock(block);
                    break;
                default:
                {
                    if (ShouldAsComment(statementSyntax))
                    {
                        WriteAsComment(statementSyntax.ToString(), statementSyntax.GetType());
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"{statementSyntax.GetType().Name} is not supported for {statementSyntax.ToString()}");
                    }

                    break;
                }
            }
        }

        void WriteBracketedArgumentList(BracketedArgumentListSyntax bracketedArgumentList)
        {
            foreach (var argument in bracketedArgumentList.Arguments)
            {
                var exp = argument.Expression.ToString();
                if (int.TryParse(exp, out var index))
                {
                    WriteIndex(index);
                    continue;
                }

                throw new NotSupportedException($"BracketedArgumentListSyntax {bracketedArgumentList}");
            }

            void WriteIndex(int index)
            {
                if (index == 0)
                {
                    _context.Liquid.Append(" | first");
                    return;
                }

                throw new NotImplementedException($"BracketedArgumentListSyntax, Index {index}");
            }
        }


        void WriteEqualsValueClause(EqualsValueClauseSyntax equalsValueClause)
        {
            _context.Liquid.Append($"{equalsValueClause.EqualsToken} ");
            TransformExpression(equalsValueClause.Value);
        }


        void TransformExpression(ExpressionSyntax expressionSyntax)
        {
            expressionSyntax.FindFirstChildNode<IdentifierNameSyntax>();
            switch (expressionSyntax)
            {
                case AssignmentExpressionSyntax assignmentExpression:
                    WriteAssignmentExpression(assignmentExpression);
                    break;
                case MemberAccessExpressionSyntax memberAccess:
                    WriteMemberAccess(memberAccess);
                    break;
                case BinaryExpressionSyntax binary:
                    WriteBinary(binary);
                    break;
                case LiteralExpressionSyntax literal:
                    WriteLiteral(literal);
                    break;
                case InvocationExpressionSyntax invocation:
                    WriteInvocation(invocation);
                    break;
                case IdentifierNameSyntax identifierName:
                    WriteIdentifierName(identifierName);
                    break;
                case ElementAccessExpressionSyntax elementAccessExpression:
                    WriteElementAccessExpression(elementAccessExpression);
                    break;
                case CastExpressionSyntax castExpression:
                    WriteCastExpression(castExpression);
                    break;
                default:
                {
                    if (ShouldAsComment(expressionSyntax))
                    {
                        WriteAsComment(expressionSyntax);
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"{expressionSyntax.GetType().Name} is not supported for {expressionSyntax.ToString()}");
                    }

                    break;
                }
            }
        }

        void WriteCastExpression(CastExpressionSyntax castExpression)
        {
            var oldHint = _context.Hint;
            _context.Hint = ReadingHint.Expression;
            TransformExpression(castExpression.Expression);
            _context.Hint = oldHint;
        }

        void WriteAsComment(ExpressionSyntax conditionalExpression)
        {
            _context.Liquid.Append("TODO_COMMENT %}");
            WriteAsComment(conditionalExpression.ToString(), conditionalExpression.GetType());
        }

        void WriteElementAccessExpression(ElementAccessExpressionSyntax elementAccessExpression)
        {
            StartBars();
            TransformExpression(elementAccessExpression.Expression);
            WriteBracketedArgumentList(elementAccessExpression.ArgumentList);
            EndBars();
        }

        void HandleExpressionsStatement(ExpressionStatementSyntax expressionStatementSyntax)
        {
            TransformExpression(expressionStatementSyntax.Expression);
        }

        private void WriteIf(IfStatementSyntax ifSyntax)
        {
            var isUnless = IsUnless(ifSyntax);
            StartCode();
            if (isUnless)
            {
                _context.Liquid.Append("unless ");
                _context.Inner.Push("unless");
            }
            else
            {
                _context.Liquid.Append("if ");
                _context.Inner.Push("if");
            }

            var old = _context.Hint;
            _context.Hint = ReadingHint.Expression;
            if (isUnless && ifSyntax.Condition is PrefixUnaryExpressionSyntax prefix)
            {
                TransformExpression(prefix.Operand);
            }
            else
            {
                TransformExpression(ifSyntax.Condition);
            }

            EndCode();
            TransformCSharpSyntax(ifSyntax.Statement);
            _context.Liquid.AppendLine();
            if (ifSyntax.Else != null)
            {
                WriteElseClause(ifSyntax.Else);
                if (_context.Inner.Count > 0)
                {
                    _context.Inner.Pop();
                }

                _context.Liquid.AppendLine("");
                _context.Liquid.AppendLine("{% endif %}");
            }

            _context.Hint = old;

            bool IsUnless(IfStatementSyntax ifStatementSyntax)
            {
                if (ifStatementSyntax.Condition is PrefixUnaryExpressionSyntax p)
                {
                    var token = p.OperatorToken.Text;
                    return token == "!";
                }

                return false;
            }
        }

        void WriteElseClause(ElseClauseSyntax elseClause)
        {
            StartCode();
            _context.Liquid.Append("else");
            EndCode();
            TransformCSharpSyntax(elseClause.Statement);
        }

        private void WriteForEach(ForEachStatementSyntax node)
        {
            StartCode();
            var oldHint = _context.Hint;
            _context.Hint = ReadingHint.Expression;
            _context.Liquid.Append("for ");
            _context.Liquid.Append(node.Identifier);
            _context.Liquid.Append(" in ");
            TransformExpression(node.Expression);
            EndCode();
            _context.Liquid.AppendLine();
            TransformCSharpSyntax(node.Statement);
            _context.Liquid.AppendLine();
            _context.Hint = oldHint;
            _context.Inner.Push("for");
        }

        private void HandleLocalDeclaration(LocalDeclarationStatementSyntax node)
        {
            TransformCSharpSyntax(node.Declaration);
        }


        private void StartBars()
        {
            if (_context.Hint == ReadingHint.Expression)
            {
                return;
            }

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
            if (_context.Hint == ReadingHint.Expression)
            {
                return;
            }

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

        void WriteAssignmentExpression(AssignmentExpressionSyntax ae)
        {
            _context.Liquid.AppendLine();
            _context.Liquid.Append("{% assign ");
            TransformExpression(ae.Left);
            _context.Liquid.Append(" = ");
            TransformExpression(ae.Right);
            _context.Liquid.Append(" %}");
        }

        bool ShouldAsComment(SyntaxNode node)
        {
            if (node is ConditionalExpressionSyntax)
            {
                return true;
            }

            if (node is PrefixUnaryExpressionSyntax)
            {
                return true;
            }


            if (node is PostfixUnaryExpressionSyntax)
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
            foreach (var statement in block.Statements)
            {
                TransformCSharpSyntax(statement);
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
                    if (variable.Initializer != null)
                    {
                        WriteEqualsValueClause(variable.Initializer);
                    }
                    else
                    {
                        _context.Liquid.Append("= \"\"");
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

            var methodName = name.ToString();
            switch (methodName)
            {
                case "Translate":
                case "TranslateFormat":
                    WriteTranslate(invocation, string.Empty);
                    break;
                case "TranslateRaw":
                    WriteTranslate(invocation, "raw");
                    break;
                case "Raw":
                    WriteRaw(invocation);
                    break;
                case "GetFormattedPrice":
                    WriteMethod("format_price", invocation);
                    break;
                case "ToCurrencyString":
                    WriteMethod("currency", invocation);
                    break;
                case "RenderBody":
                    WriteRenderbody(invocation);
                    break;
                case "ShowBoleto":
                    WritePartial("Boleto", invocation);
                    break;
                case "ShowWireTransfer":
                    WritePartial("WireTransfer", invocation);
                    break;
                default:
                    throw new NotSupportedException($"WriteInvocation: Unknown Method {methodName}");
            }
        }

        void WritePartial(string partialName, InvocationExpressionSyntax invocation)
        {
            StartCode();
            var oldHint = _context.Hint;
            _context.Hint = ReadingHint.Expression;
            _context.Liquid.Append($"partial '{partialName}'");
            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                _context.Liquid.Append(", ");
                TransformExpression(argument.Expression);
            }
            _context.Hint = oldHint;

            EndCode();
        }

        void WriteRenderbody(InvocationExpressionSyntax invocation)
        {
            StartCode();
            _context.Liquid.Append("renderbody");
            EndCode();
        }

        void WriteMethod(string filter, InvocationExpressionSyntax invocation)
        {
            var argument = invocation.ArgumentList.FindFirstChildNode<ArgumentSyntax>();
            if (argument == null)
            {
                throw new InvalidOperationException($"Argument Expression is expected for {invocation}");
            }

            TransformExpression(argument.Expression);

            _context.Liquid.Append($" | {filter}");

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
            else
            {
                StartBars();
                TransformExpression(argument.Expression);
                _context.Liquid.Append(" | raw");
                EndBars();
            }
        }

        private void WriteTranslate(InvocationExpressionSyntax invocation, string lastFilter)
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

            if (!string.IsNullOrWhiteSpace(lastFilter))
            {
                _context.Liquid.Append($" | {lastFilter}");
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
            TransformExpression(binary.Left);
            var kind = binary.OperatorToken.Kind();
            if (kind == SyntaxKind.PlusToken)
            {
                _context.Liquid.AppendFormat(" | append: ");
            }
            else
            {
                _context.Liquid.AppendFormat(" {0} ", binary.OperatorToken);
            }

            TransformExpression(binary.Right);
            EndBars();
        }

        private void WriteMemberAccess(MemberAccessExpressionSyntax memberAccess)
        {
            if (_context.Hint != ReadingHint.Expression)
            {
                StartBars();
                TransformExpression(memberAccess.Expression);
                _context.Liquid.Append(memberAccess.OperatorToken);
                TransformExpression(memberAccess.Name);
                EndBars();
            }
            else
            {
                TransformExpression(memberAccess.Expression);
                _context.Liquid.Append(memberAccess.OperatorToken);
                TransformExpression(memberAccess.Name);
            }
        }
    }
}