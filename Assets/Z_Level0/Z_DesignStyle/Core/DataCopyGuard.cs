using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Z_DesignStyle
{
    // Track the active instance path, not types or all previously visited rows.
    // Shared acyclic references can be copied independently by different fields.
    public static class DataCopyGuard
    {
        [ThreadStatic] private static HashSet<object> active;
        [ThreadStatic] private static List<object> path;

        public static Scope Enter(object source)
        {
            if (active == null)
            {
                active = new HashSet<object>(IdentityComparer.Instance);
                path = new List<object>();
            }
            if (!active.Add(source))
            {
                var message = new StringBuilder("Form.Data.Copy() circular object reference: ");
                foreach (var item in path)
                    AppendObject(message, item).Append(" -> ");
                AppendObject(message, source);
                throw new InvalidOperationException(message.ToString());
            }
            path.Add(source);
            return new Scope(source);
        }

        private static StringBuilder AppendObject(StringBuilder text, object source) => text
            .Append(source.GetType().FullName).Append('#').Append(RuntimeHelpers.GetHashCode(source).ToString("x"));

        public readonly struct Scope : IDisposable
        {
            private readonly object source;
            internal Scope(object source) { this.source = source; }
            public void Dispose()
            {
                active.Remove(source);
                path.RemoveAt(path.Count - 1);
            }
        }

        private sealed class IdentityComparer : IEqualityComparer<object>
        {
            public static readonly IdentityComparer Instance = new IdentityComparer();
            public new bool Equals(object left, object right) => ReferenceEquals(left, right);
            public int GetHashCode(object value) => RuntimeHelpers.GetHashCode(value);
        }
    }
}
