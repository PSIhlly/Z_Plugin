using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public abstract class Z_Event
{

}
public interface IZ_Listener<T>
{
    public abstract void OnEvent(T evt);
}
public static class Z_EventHelper
{
    private sealed class Listeners
    {
        internal readonly LinkedList<object> ordered = new LinkedList<object>();
        internal readonly Dictionary<object, LinkedListNode<object>> nodes = new Dictionary<object, LinkedListNode<object>>();
    }
    private static readonly Dictionary<(Type, string), Listeners> type2Listener = new Dictionary<(Type, string), Listeners>();
    private static readonly Stack<List<object>> snapshots = new Stack<List<object>>();

    // Categories match exactly. The empty category keeps existing calls isolated
    // from named channels; it is not a wildcard subscription.
    public static void Register<T>(this IZ_Listener<T> listener, string category = "")
    {
        if (listener == null) return;
        var key = (typeof(T), category ?? "");
        if (!type2Listener.TryGetValue(key, out var bucket)) type2Listener[key] = bucket = new Listeners();
        if (!bucket.nodes.ContainsKey(listener)) bucket.nodes.Add(listener, bucket.ordered.AddLast(listener));
    }
    public static void Unregister<T>(this IZ_Listener<T> listener, string category = "")
    {
        if (listener == null) return;
        var key = (typeof(T), category ?? "");
        if (type2Listener.TryGetValue(key, out var bucket) && bucket.nodes.TryGetValue(listener, out var node))
        {
            bucket.nodes.Remove(listener); bucket.ordered.Remove(node);
            if (bucket.nodes.Count == 0) type2Listener.Remove(key);
        }
    }

    public static void Invoke<T>(T evt, string category = "")
    {
        if (evt == null) throw new ArgumentNullException(nameof(evt));
        if (!type2Listener.TryGetValue((evt.GetType(), category ?? ""), out var bucket)) return;
        var snapshot = snapshots.Count > 0 ? snapshots.Pop() : new List<object>();
        try
        {
            foreach (var listener in bucket.ordered) snapshot.Add(listener);
            foreach (var listener in snapshot)
                if (listener != null) ((IZ_Listener<T>)listener).OnEvent(evt);
        }
        finally { snapshot.Clear(); snapshots.Push(snapshot); }
    }
}
