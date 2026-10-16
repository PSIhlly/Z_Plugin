using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Z_Map;
using Z_UnitSystem;

// Isolated runtime-pool lifecycle checks; no production scenes or saves are loaded.
public static class InstancePoolNameRegression
{
    public static void Run()
    {
        int checks = 0;
        var cleanup = new List<GameObject>();
        InstancePoolManager manager = null;
        InstancePool removed = null;
        try
        {
            var holder = new GameObject("pool-test-root");
            cleanup.Add(holder);
            var first = Template("named-pool-A", cleanup);
            var second = Template("named-pool-B", cleanup);
            var third = Template("named-pool-C", cleanup);
            var unknown = Template("not-registered", cleanup);
            var managerObject = new GameObject("pool-test-manager");
            managerObject.SetActive(false);
            cleanup.Add(managerObject);
            manager = managerObject.AddComponent<InstancePoolManager>();
            manager.defaultRoot = holder.transform;
            typeof(InstancePoolManager).GetField("prefabs", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(manager, new List<GameObject> { first, second });
            manager.Init();
            Check(manager.pools.Count == 2 && manager.pools[first.name].prefab == first,
                "Init indexes authored prefabs by name", ref checks);
            Check(manager.GetPrefab(second.name) == second && manager.GetPrefab(first.name.ToLowerInvariant()) == null
                && manager.GetPrefab(null) == null, "lookup keeps exact case-sensitive name matching", ref checks);

            var live = manager.CreateInstance(first);
            cleanup.Add(live);
            Check(live.name == first.name && live.activeSelf, "named acquisition activates and retains the prefab name", ref checks);
            using (manager.BeginViewRefreshReuse())
            {
                manager.DeleteInstance(live, first);
                Check(live.activeSelf, "named return retains an eligible instance during refresh", ref checks);
                Check(manager.CreateInstance(first) == live, "named acquisition reuses the same pending live instance", ref checks);
                manager.DeleteInstance(live, first);
            }
            Check(!live.activeSelf, "refresh disposal deactivates pending instances from dictionary Values", ref checks);
            Check(manager.CreateInstance(first) == live && live.activeSelf, "ordinary name lookup reuses the inactive instance", ref checks);

            var duplicate = new InstancePool(first, holder.transform);
            bool rejected = false;
            try { manager.Register(duplicate); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected && manager.pools.Count == 2, "duplicate name registration cannot overwrite a live pool", ref checks);
            manager.Unregister(duplicate);
            Check(manager.GetPrefab(first.name) == first, "unregistering a different same-name pool leaves the registered pool intact", ref checks);

            removed = new InstancePool(third, holder.transform);
            using (manager.BeginViewRefreshReuse())
            {
                manager.Register(removed);
                var extra = manager.CreateInstance(third);
                cleanup.Add(extra);
                manager.DeleteInstance(extra, third);
                Check(extra.activeSelf, "a pool registered during refresh joins the active reuse scope", ref checks);
                manager.Unregister(removed);
                Check(!extra.activeSelf && manager.GetPrefab(third.name) == null,
                    "unregistration ends pending reuse and removes its name", ref checks);
            }
            removed.Destroy();
            removed = null;

            var fallback = manager.CreateInstance(unknown);
            cleanup.Add(fallback);
            fallback.SetActive(true);
            manager.DeleteInstance(fallback, unknown);
            Check(fallback.name == unknown.name && !fallback.activeSelf && manager.GetPrefab(unknown.name) == null,
                "unregistered names retain instantiate/deactivate fallback behavior", ref checks);

            var stale = manager.BeginViewRefreshReuse();
            manager.Clear();
            Check(manager.pools.Count == 0 && !live.activeSelf, "Clear detaches and destroys all named pools", ref checks);
            manager.Register(new InstancePool(first, holder.transform));
            using (manager.BeginViewRefreshReuse())
            {
                var replacement = manager.CreateInstance(first);
                cleanup.Add(replacement);
                stale.Dispose();
                manager.DeleteInstance(replacement, first);
                Check(replacement.activeSelf, "an old scope cannot end a replacement named pool's refresh", ref checks);
            }
            Check(manager.pools.Count == 1, "replacement pool remains registered after scope completion", ref checks);
            Debug.Log("INSTANCE_POOL_NAME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (manager != null) manager.Clear();
            if (removed != null) removed.Destroy();
            for (int i = cleanup.Count - 1; i >= 0; i--)
                if (cleanup[i] != null) UnityEngine.Object.DestroyImmediate(cleanup[i]);
        }
    }

    private static GameObject Template(string name, List<GameObject> cleanup)
    {
        var prefab = new GameObject(name);
        prefab.SetActive(false);
        prefab.AddComponent<TileInstance>();
        cleanup.Add(prefab);
        return prefab;
    }

    private static void Check(bool condition, string message, ref int checks)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
