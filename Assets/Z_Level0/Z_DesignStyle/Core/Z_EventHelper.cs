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
    public void GG()
    {

    }
}
public static class Z_EventHelper
{
    private static Dictionary<Type, List<object>> type2Listener=new Dictionary<Type, List<object>>();
    public static void Register<T>(this IZ_Listener<T> listener)
    {
        var type = typeof(T);
        if(!type2Listener.ContainsKey(type))
        {
            type2Listener[type] = new List<object>(); 
        }
        type2Listener[type].Add(listener);
    }

    public static void Invoke<T>(T evt)
    {
        var type = evt.GetType();
        foreach (var listener in type2Listener[type])
        {
            if(listener!=null)
            ((IZ_Listener<T>)listener).OnEvent(evt);
        }
    }
}
