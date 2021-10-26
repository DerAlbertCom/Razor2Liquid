using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
                case ConditionalExpressionSyntax conditionalExpression:
                    WriteConditionalExpression(conditionalExpression);
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
        
        private void WriteConditionalExpression(ConditionalExpressionSyntax conditionalExpression)
        {
            if (conditionalExpression.Condition is IdentifierNameSyntax)
            {
                TransformExpression(conditionalExpression.Condition);
                _context.Liquid.Append(" | tenary: ");
                TransformExpression(conditionalExpression.WhenTrue);
                _context.Liquid.Append(", ");
                TransformExpression(conditionalExpression.WhenFalse);
                
            }
            else
            {
                WriteAsComment(conditionalExpression);
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
                case PrefixUnaryExpressionSyntax prefixUnaryExpression:
                    WritePrefixUnaryExpression(prefixUnaryExpression);
                    break;
                case ConditionalExpressionSyntax conditionalExpressionSyntax:
                    WriteConditionalExpression(conditionalExpressionSyntax);
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

        private void WritePrefixUnaryExpression(PrefixUnaryExpressionSyntax prefixUnaryExpression)
        {
            var kind = prefixUnaryExpression.OperatorToken.Kind();
            _context.OperatorKind = kind;
            TransformExpression(prefixUnaryExpression.Operand);
            if (_context.Hint == ReadingHint.Expression)
            {
                if (prefixUnaryExpression.Operand is IdentifierNameSyntax)
                {
                    if (kind == SyntaxKind.ExclamationToken)
                    {
                        _context.Liquid.Append(" == false");
                    }
                    else
                    {
                        _context.Liquid.Append(" == true");
                    }
                }
                else if (prefixUnaryExpression.Operand is MemberAccessExpressionSyntax)
                {
                    if (kind == SyntaxKind.ExclamationToken)
                    {
                        _context.Liquid.Append(" == false");
                    }
                    else
                    {
                        _context.Liquid.Append(" == true");
                    }
                }
                else if (prefixUnaryExpression.Operand is InvocationExpressionSyntax invocation)
                {
                    var argument = invocation.ArgumentList.Arguments.FirstOrDefault();
                    if (argument == null)
                    {
                        if (kind == SyntaxKind.ExclamationToken)
                        {
                            _context.Liquid.Append(" == false");
                        }
                        else
                        {
                            _context.Liquid.Append(" == true");
                        }
                    }
                }
            }

            _context.OperatorKind = SyntaxKind.None;
        }

        void WriteCastExpression(CastExpressionSyntax castExpression)
        {
            var oldHint = _context.Hint;
            _context.Hint = ReadingHint.Expression;
            TransformExpression(castExpression.Expression);
            _context.Hint = oldHint;
        }

        void WriteAsComment(ExpressionSyntax conditionalExpression, [CallerMemberName] string memberName = "")
        {
            _context.Liquid.Append("TODO_COMMENT %}");
            WriteAsComment(conditionalExpression.ToString(), conditionalExpression.GetType(), memberName);
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

        private void WriteIf(IfStatementSyntax ifSyntax, bool code = true)
        {
            var cond = ifSyntax.Condition.ToString();
            if (string.IsNullOrWhiteSpace(cond))
            {
                if (ifSyntax.Else != null)
                {
                    _context.Liquid.AppendLine("");
                    WriteElse(ifSyntax.Else);
                    return;
                }
            }

            if (AppendIf(ifSyntax))

            {
                _context.Liquid.AppendLine("");
                _context.Liquid.AddIndent(_context.Inner.Count);
                StartCode();
                _context.Inner.Push("if");
                _context.Liquid.Append("if ");
            }


            var old = _context.Hint;
            _context.Hint = ReadingHint.Expression;
            TransformExpression(ifSyntax.Condition);
            if (AppendIf(ifSyntax))
            {
                EndCode();
            }

            TransformCSharpSyntax(ifSyntax.Statement);
            if (code) _context.Liquid.AppendLine();
            WriteElse(ifSyntax.Else);

            _context.Hint = old;

            bool AppendIf(IfStatementSyntax ifStatementSyntax)
            {
                if (ifStatementSyntax.Parent is ElseClauseSyntax)
                {
                    return false;
                }

                var condition = ifStatementSyntax.Condition.ToString();
                return !string.IsNullOrWhiteSpace(condition);
            }

            void WriteElse(ElseClauseSyntax elseClauseSyntax)
            {
                if (elseClauseSyntax != null)
                {
                    WriteElseClause(elseClauseSyntax);
                    // assumption that there is not markup in between, the write endif
                    if (elseClauseSyntax.Statement is BlockSyntax blockSyntax)
                    {
                        WriteEndIf(blockSyntax);
                    }

                    else if (elseClauseSyntax.Statement is IfStatementSyntax ifStatementSyntax)
                    {
                        if (ifStatementSyntax.Statement is BlockSyntax blockSyntax2)
                        {
                            WriteEndIf(blockSyntax2);
                        }
                    }
                }
            }
        }

        void WriteEndIf(BlockSyntax blockSyntax)
        {
            if (blockSyntax == null)
            {
                EndIf();
            }
            else if (blockSyntax.Statements.Count > 0)
            {
                if (_context.Inner.Count > 0)
                {
                    EndIf();
                }
            }

            void EndIf()
            {
                var what = _context.Inner.Pop();
                _context.Liquid.AppendLine("");
                _context.Liquid.AddIndent(_context.Inner.Count);
                _context.Liquid.AppendLine($"{{% end{what} %}}");
            }
        }

        void WriteElseClause(ElseClauseSyntax elseClause)
        {
            var statement = false;
            if (elseClause.Statement is IfStatementSyntax ifStatement)
            {
                _context.Liquid.AppendLine("");
                _context.Liquid.AddIndent(_context.Inner.Count - 1);
                StartCode();
                _context.Liquid.Append("elsif ");
                WriteIf(ifStatement, false);
                statement = true;
                EndCode();
                _context.Liquid.AppendLine("");
            }
            else
            {
                _context.Liquid.AppendLine("");
                _context.Liquid.AddIndent(_context.Inner.Count - 1);
                StartCode();
                _context.Liquid.Append("else");
                EndCode();
                _context.Liquid.AppendLine("");
            }

            if (!statement)
            {
                TransformCSharpSyntax(elseClause.Statement);
            }
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

        void WriteAsComment(string value, Type getType = null, [CallerMemberName] string memberName = "")
        {
            _context.Model.Liquid.AppendLine();
            _context.Model.Liquid.AppendLine("{% comment %}");
            if (getType != null)
            {
                _context.Liquid.AppendFormat("---Expression: {0} ---- From: {1}", getType.Name, memberName);
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

            //      WriteEndIf(block, true);
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
                WriteFunctionInvocation(invocation);
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

        private void WriteFunctionInvocation(InvocationExpressionSyntax invocation)
        {
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var name = memberAccess.Name.ToString();
                switch (name)
                {
                    case "Equals":
                    {
                        TransformExpression(memberAccess.Expression);
                        if (_context.OperatorKind == SyntaxKind.ExclamationToken)
                        {
                            _context.Liquid.Append(" != ");
                        }
                        else
                        {
                            _context.Liquid.Append(" == ");
                        }

                        var argument = invocation.ArgumentList.Arguments.First();
                        TransformExpression(argument.Expression);
                        return;
                    }
                    case "ToString":
                        TransformExpression(memberAccess.Expression);
                        return;
                    case "IsNullOrEmpty":
                    {
                        var argument = invocation.ArgumentList.Arguments.First();
                        TransformExpression(argument.Expression);
                        _context.Liquid.Append(" | is_null_or_empty");
                        if (_context.OperatorKind == SyntaxKind.ExclamationToken)
                        {
                            _context.Liquid.Append(" == false");
                        }

                        return;
                    }
                }
            }

            WriteAsComment(invocation);
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
            else if (kind == SyntaxKind.BarBarToken)
            {
                _context.Liquid.AppendFormat(" or ");
            }
            else if (kind == SyntaxKind.AmpersandAmpersandToken)
            {
                _context.Liquid.AppendFormat(" and ");
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

    public static class StringBuilderExtensions
    {
        public static StringBuilder AddIndent(this StringBuilder builder, int count)
        {
            if (count > 0)
            {
                builder.Append("".PadLeft(count * 2));
            }

            return builder;
        }
    }
}