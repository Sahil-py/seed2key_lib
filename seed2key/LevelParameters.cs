using System;

namespace seed2key
{
    /// <summary>
    /// Algorithm parameters for one Security Access level.
    /// XOR + rotate: key = RotateLeft(fold(seed), RotateBits) XOR Mask
    /// </summary>
    public sealed class LevelParameters
    {
        /// <summary>Secret XOR mask applied after rotation (must be 4 bytes).</summary>
        public uint Mask { get; }

        /// <summary>Number of bits to rotate left (0–31).</summary>
        public int RotateBits { get; }

        public LevelParameters(uint mask, int rotateBits)
        {
            if (rotateBits < 0 || rotateBits > 31)
                throw new ArgumentOutOfRangeException(nameof(rotateBits), "Must be 0–31.");
            Mask = mask;
            RotateBits = rotateBits;
        }
    }
}
