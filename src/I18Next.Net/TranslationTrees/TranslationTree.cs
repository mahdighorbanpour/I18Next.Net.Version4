using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace I18Next.Net.TranslationTrees;

public class TranslationTree : ITranslationTree
{
    public TranslationTree(TranslationGroup rootNode)
    {
        Root = rootNode;
    }

    public TranslationTreeNode Root { get; set; }

    private readonly ConcurrentDictionary<TranslationGroup, Dictionary<string, TranslationTreeNode>> _childrenLookup
        = new();

    public IDictionary<string, string> GetAllValues()
    {
        var result = new Dictionary<string, string>();

        if (Root == null)
            return result;

        var stack = new Stack<TranslationTreeNode>();
        stack.Push(Root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (node is TranslationGroup group)
            {
                var children = group.Children;
                for (var i = 0; i < children.Length; i++)
                {
                    var child = children[i];
                    if (child is Translation translation)
                        result.Add(translation.Name, translation.Value);
                    else
                        stack.Push(child);
                }
            }
            else if (node is Translation translation)
            {
                result.Add(translation.Name, translation.Value);
            }
        }

        return result;
    }

    public string GetValue(string key, IDictionary<string, object> args)
    {
        if (Root == null)
            return null;

        var span = key.AsSpan();
        var node = Root;
        var start = 0;

        for (var i = 0; i <= span.Length; i++)
        {
            if (i < span.Length && span[i] != '.')
                continue;

            var partSpan = span[start..i];
            var part = partSpan.ToString();

            if (node is TranslationGroup group)
            {
                var lookup = _childrenLookup.GetOrAdd(group, BuildLookup);
                if (lookup.TryGetValue(part, out var foundNode))
                {
                    node = foundNode;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                throw new TranslationKeyInvalidException(key,
                    $"The key `{key}` ends up in a final translation at part `{part}`. Cannot go down further the translation tree. Please check the key you've provided.");
            }

            start = i + 1;
        }

        if (node is TranslationGroup)
            throw new TranslationKeyInvalidException(key,
                $"The key `{key}` leads to a group of translations. Unable to resolve a final value for the given key. Please check the key you've provided.");

        return ((Translation)node).Value;
    }

    public string Namespace { get; set; }

    private static Dictionary<string, TranslationTreeNode> BuildLookup(TranslationGroup group)
    {
        var children = group.Children;
        var dict = new Dictionary<string, TranslationTreeNode>(children.Length, StringComparer.Ordinal);
        for (var i = 0; i < children.Length; i++)
        {
            var child = children[i];
            if (!dict.ContainsKey(child.Name))
                dict[child.Name] = child;
        }
        return dict;
    }
}
