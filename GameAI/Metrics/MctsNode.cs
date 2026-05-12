using System.Collections.Generic;
using System.Linq;

class MctsNode
{
    public Gra Gra;
    public MctsNode? Rodzic;
    public List<MctsNode> Dzieci = null!;

    public Ruch? Ruch;

    public int Wizyty;
    public double Wygrane;

    public List<Ruch> NieprzetestowaneRuchy = null!;

    internal MctsNode()
    {
    }

    public static MctsNode Create(Gra gra, MctsNode? rodzic, Ruch? ruch = null)
    {
        var availableMoves = gra.DostepneRuchy();
        var node = MctsNodePool.Rent();
        node.Gra = gra;
        node.Rodzic = rodzic;
        node.Ruch = ruch;
        node.Wizyty = 0;
        node.Wygrane = 0;
        node.Dzieci = ListPool<MctsNode>.Rent();
        node.NieprzetestowaneRuchy = ListPool<Ruch>.Rent(availableMoves.Count);
        node.NieprzetestowaneRuchy.AddRange(availableMoves);
        return node;
    }
}

static class ListPool<T>
{
    private static readonly Stack<List<T>> Pool = new Stack<List<T>>();

    public static List<T> Rent(int capacity = 0)
    {
        var list = Pool.Count > 0 ? Pool.Pop() : new List<T>(capacity > 0 ? capacity : 4);
        if (capacity > list.Capacity)
            list.Capacity = capacity;
        return list;
    }

    public static void Return(List<T> list)
    {
        list.Clear();
        Pool.Push(list);
    }
}

static class MctsNodePool
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
        node.Wizyty = 0;
        node.Wygrane = 0;

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