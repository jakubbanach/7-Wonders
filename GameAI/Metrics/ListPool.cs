using System;
using System.Collections.Generic;
using System.Text;

public static class ListPool<T>
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