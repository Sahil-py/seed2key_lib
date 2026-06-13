using System;
using System.IO;
using System.Reflection;

namespace seed2key
{
    /// <summary>
    /// Loads an external seed-to-key plugin DLL at runtime via reflection.
    /// The DLL must export a public concrete class implementing <see cref="ISeedToKey"/>.
    /// </summary>
    public static class SeedToKeyLoader
    {
        /// <summary>Load and return the plugin, or throw if not found/invalid.</summary>
        public static ISeedToKey Load(string dllPath)
        {
            if (dllPath == null) throw new ArgumentNullException(nameof(dllPath));
            if (!File.Exists(dllPath))
                throw new FileNotFoundException("Plugin DLL not found.", dllPath);

            // If loading our own DLL (same assembly that defines ISeedToKey), return the built-in.
            // Using name compare avoids path/identity issues from Assembly.LoadFrom.
            string asmName = AssemblyName.GetAssemblyName(dllPath).Name;
            if (string.Equals(asmName, typeof(ISeedToKey).Assembly.GetName().Name,
                    StringComparison.OrdinalIgnoreCase))
                return new Seed2KeyCalculator();

            Assembly asm        = Assembly.LoadFrom(dllPath);
            string   ifaceName  = typeof(ISeedToKey).FullName;

            foreach (Type t in asm.GetExportedTypes())
            {
                if (t.IsAbstract || t.IsInterface) continue;
                // Compare by full name to avoid cross-context identity mismatch
                bool implements = Array.Exists(t.GetInterfaces(),
                    i => string.Equals(i.FullName, ifaceName, StringComparison.Ordinal));
                if (!implements) continue;

                object raw = Activator.CreateInstance(t);
                // Wrap via reflection so the cast works across assembly load contexts
                return new ReflectionSeedToKeyProxy(raw, t);
            }

            throw new InvalidOperationException(
                $"No type implementing ISeedToKey found in '{Path.GetFileName(dllPath)}'. " +
                "Ensure the DLL references seed2key.dll and implements ISeedToKey.");
        }

        // ── Reflection proxy ─────────────────────────────────────────────────
        // Used when a plugin DLL is loaded from a different path than the referenced seed2key.dll,
        // causing type-identity mismatch that prevents direct casting.

        private sealed class ReflectionSeedToKeyProxy : ISeedToKey
        {
            private readonly object     _inner;
            private readonly MethodInfo _computeKey;
            private readonly string     _version, _algo, _supplier;

            internal ReflectionSeedToKeyProxy(object inner, Type t)
            {
                _inner = inner;
                _computeKey = t.GetMethod("ComputeKey",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _version  = t.GetProperty("PluginVersion") ?.GetValue(inner) as string ?? "?";
                _algo     = t.GetProperty("AlgorithmName")?.GetValue(inner) as string ?? "?";
                _supplier = t.GetProperty("SupplierName") ?.GetValue(inner) as string ?? "?";
            }

            public string PluginVersion => _version;
            public string AlgorithmName => _algo;
            public string SupplierName  => _supplier;

            public byte[] ComputeKey(byte[] seed, byte accessLevel, byte[]? parameters)
            {
                if (_computeKey == null)
                    throw new MissingMethodException("ComputeKey not found via reflection.");
                return (byte[])_computeKey.Invoke(_inner,
                    new object[] { seed, accessLevel, parameters });
            }
        }

        /// <summary>Test a plugin DLL without throwing — returns a result with IsValid/Error.</summary>
        public static PluginTestResult Test(string dllPath)
        {
            try
            {
                ISeedToKey plugin = Load(dllPath);

                // Smoke-test: compute key from a dummy 4-byte seed
                byte[] key = plugin.ComputeKey(new byte[] { 0xA3, 0xB4, 0xC5, 0xD6 }, 0x01, new byte[0]);
                if (key == null || key.Length == 0)
                    return new PluginTestResult { IsValid = false, Error = "ComputeKey returned null or empty key." };

                return new PluginTestResult
                {
                    IsValid       = true,
                    Version       = plugin.PluginVersion,
                    AlgorithmName = plugin.AlgorithmName,
                    SupplierName  = plugin.SupplierName,
                };
            }
            catch (Exception ex)
            {
                return new PluginTestResult { IsValid = false, Error = ex.Message };
            }
        }
    }
}
