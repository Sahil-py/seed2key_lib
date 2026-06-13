using System;
using System.Collections.Generic;

namespace seed2key
{
    /// <summary>
    /// Built-in seed-to-key implementation using XOR + rotate algorithm.
    /// Replace the placeholder masks/rotations with your actual ECU values.
    ///
    /// Algorithm per level:
    ///   1. Fold seed bytes into a uint32 (XOR of 4-byte big-endian chunks).
    ///   2. Rotate left by LevelParameters.RotateBits.
    ///   3. XOR with LevelParameters.Mask.
    ///   4. Return as 4-byte big-endian key.
    /// </summary>
    public sealed class Seed2KeyCalculator : ISeedToKey
    {
        private readonly Dictionary<SecurityAccessLevel, LevelParameters> _params;

        public string PluginVersion => "1.0.0";
        public string AlgorithmName => "XOR+Rotate (placeholder – replace masks for production)";
        public string SupplierName  => "CANdoit Built-in";

        public Seed2KeyCalculator()
        {
            // *** REPLACE these mask/rotate values with your actual ECU parameters ***
            _params = new Dictionary<SecurityAccessLevel, LevelParameters>
            {
                { SecurityAccessLevel.Programming,      new LevelParameters(0xDEADBEEF, 3)  },
                { SecurityAccessLevel.VariantCoding,    new LevelParameters(0xC0FFEE01, 7)  },
                { SecurityAccessLevel.UnlockingControl, new LevelParameters(0xA5A5A5A5, 11) },
                { SecurityAccessLevel.EngineeringCtrl,  new LevelParameters(0x12345678, 5)  },
                { SecurityAccessLevel.SupplierEOL,      new LevelParameters(0xFEDCBA98, 9)  },
            };
        }

        /// <summary>Override the algorithm parameters for a specific level.</summary>
        public void SetParameters(SecurityAccessLevel level, LevelParameters p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            _params[level] = p;
        }

        /// <summary>
        /// Compute the 4-byte key for the given seed and SA level (typed enum overload).
        /// </summary>
        public byte[] ComputeKey(byte[] seed, SecurityAccessLevel level)
        {
            if (seed == null) throw new ArgumentNullException(nameof(seed));
            if (seed.Length == 0) throw new ArgumentException("Seed must not be empty.", nameof(seed));

            if (!_params.TryGetValue(level, out LevelParameters p))
                throw new NotSupportedException($"No parameters configured for level 0x{(byte)level:X2}.");

            uint folded  = FoldSeed(seed);
            uint rotated = RotateLeft(folded, p.RotateBits);
            uint key     = rotated ^ p.Mask;

            return new byte[]
            {
                (byte)(key >> 24),
                (byte)(key >> 16),
                (byte)(key >>  8),
                (byte)(key),
            };
        }

        // ISeedToKey – bridges raw byte level to typed enum
        byte[] ISeedToKey.ComputeKey(byte[] seed, byte accessLevel, byte[]? parameters)
            => ComputeKey(seed, (SecurityAccessLevel)accessLevel);

        // ── helpers ──────────────────────────────────────────────────────────

        private static uint FoldSeed(byte[] seed)
        {
            uint result = 0;
            int i = 0;
            while (i < seed.Length)
            {
                uint chunk = 0;
                for (int b = 0; b < 4 && i < seed.Length; b++, i++)
                    chunk |= (uint)seed[i] << (24 - b * 8);
                result ^= chunk;
            }
            return result;
        }

        private static uint RotateLeft(uint value, int bits)
            => bits == 0 ? value : (value << bits) | (value >> (32 - bits));
    }
}
