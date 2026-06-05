using System;
using System.Collections.Generic;
using System.Text;

public static class MctsNodePool
{
    private static readonly Stack<MctsNode> Pool = new Stack<MctsNode>();

    public static MctsNode Rent()
    {
        return Pool.Count > 0 ? Pool.Pop() : new MctsNode();
    }

    public static void Return(MctsNode node)
    {
        node.Gra = null!;
        node.Rodzic = null;
        node.Ruch = null;
        node.ActionIndex = -1;
        node.PolicyPrior = 0f;
        node.PolicyPriors = null;
        node.Wizyty = 0;
        node.Wygrane = 0;
        node.Dostepnosc = 0;
        node.ZakrytePola?.Clear();
        node.ZakrytePola = null;

        if (node.Dzieci != null)
        {
            ListPool<MctsNode>.Return(node.Dzieci);
            node.Dzieci = null!;
        }

        if (node.NieprzetestowaneRuchy != null)
        {
            ListPool<Ruch>.Return(node.NieprzetestowaneRuchy);
            node.NieprzetestowaneRuchy = null!;
        }

        Pool.Push(node);
    }

    public static void ReturnTree(MctsNode root)
    {
        var stack = new Stack<MctsNode>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (node.Dzieci != null)
            {
                for (int i = 0; i < node.Dzieci.Count; i++)
                    stack.Push(node.Dzieci[i]);
            }

            Return(node);
        }
    }
}