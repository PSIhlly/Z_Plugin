public static class EventHelperRegression
{
    private sealed class Event : Z_Event { }
    private sealed class Other : Z_Event { }
    private sealed class Probe : IZ_Listener<Event>
    {
        public Action action;
        public int count;
        public void OnEvent(Event evt) { count++; action?.Invoke(); }
    }
    private sealed class OtherProbe : IZ_Listener<Other>
    {
        public int count;
        public void OnEvent(Other evt) { count++; }
    }
    private static int checks;
    public static int Run()
    {
        var evt = new Event(); var plain = new Probe(); var named = new Probe(); var other = new OtherProbe();
        plain.Register(); plain.Register(); named.Register("uid:1"); named.Register("uid:1"); named.Register("uid:2");
        other.Register("uid:1");
        Z_EventHelper.Invoke(evt);
        Check(plain.count == 1 && named.count == 0, "empty category and duplicate registration");
        Z_EventHelper.Invoke(evt, "uid:1"); Z_EventHelper.Invoke(evt, "missing");
        Check(plain.count == 1 && named.count == 1 && other.count == 0, "exact category and event type isolation");
        named.Unregister("uid:1"); Z_EventHelper.Invoke(evt, "uid:1"); Z_EventHelper.Invoke(evt, "uid:2");
        Check(named.count == 2, "unregister removes only one named category");
        Z_EventHelper.Invoke(new Other(), "uid:1"); Check(other.count == 1, "second event type keeps its own channel");
        Z_EventHelper.Invoke(evt, null); Check(plain.count == 2, "null category normalizes to empty");
        plain.Unregister(); named.Unregister("uid:2"); other.Unregister("uid:1");

        var order = new List<string>(); var a = new Probe(); var b = new Probe(); var c = new Probe(); var nested = new Probe();
        a.action = () => { order.Add("a"); b.Unregister("mutating"); c.Register("mutating"); Z_EventHelper.Invoke(evt, "nested"); };
        b.action = () => order.Add("b"); c.action = () => order.Add("c"); nested.action = () => order.Add("nested");
        a.Register("mutating"); b.Register("mutating"); nested.Register("nested");
        Z_EventHelper.Invoke(evt, "mutating");
        Check(string.Join(",", order) == "a,nested,b", "reentrant publication and listener mutation preserve the current snapshot");
        order.Clear(); Z_EventHelper.Invoke(evt, "mutating");
        Check(string.Join(",", order) == "a,nested,c", "new registrations enter only subsequent snapshots");
        a.Unregister("mutating"); b.Unregister("mutating"); c.Unregister("mutating"); nested.Unregister("nested");

        var failure = new InvalidOperationException("fixture");
        var throwing = new Probe { action = () => throw failure }; throwing.Register("exception");
        try { Z_EventHelper.Invoke(evt, "exception"); throw new Exception("exception missing"); }
        catch (InvalidOperationException error) { Check(ReferenceEquals(error, failure), "listener exceptions propagate"); }
        throwing.Unregister("exception");
        var steady = new Probe(); steady.Register("steady");
        for (int i = 0; i < 100; i++) Z_EventHelper.Invoke(evt, "steady");
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++) Z_EventHelper.Invoke(evt, "steady");
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(steady.count == 10100, "pool recovers after a throwing or nested callback");
        Check(bytes == 0, "warmed publication allocates no listener snapshots: " + bytes);
        steady.Unregister("steady");
        return checks;
    }
    private static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); checks++; }
}
