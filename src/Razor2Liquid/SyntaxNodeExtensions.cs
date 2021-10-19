using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Razor2Liquid
{
    public static class SyntaxNodeExtensions
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<SyntaxNode> nodes) where T : SyntaxNode
        {
            return nodes.Where(n => n is T).Cast<T>();
        }

        public static T FindFirstChildNode<T>(this SyntaxNode node) where T : SyntaxNode
        {
            if (node == null)
            {
                return null;
            }
            return node.ChildNodes().Filter<T>().FirstOrDefault();
        }

        static T RecursiveFindFirstChildNode<T>(this SyntaxNode node) where T : SyntaxNode
        {
            var first = node.FindFirstChildNode<T>();
            if (first != null)
            {
                return first;
            }

            foreach (var childNode in node.ChildNodes())
            {
                childNode.RecursiveFindFirstChildNode<T>();
            }

            return null;
        }
    }
}