namespace seed2key
{
    /// <summary>
    /// Plugin interface for UDS Security Access seed-to-key calculation.
    /// Third-party DLLs (e.g. Marelli eSA) must implement this interface.
    /// </summary>
    public interface ISeedToKey
    {
        string PluginVersion  { get; }
        string AlgorithmName  { get; }
        string SupplierName   { get; }

        /// <summary>Compute the UDS 0x27 sendKey bytes from a seed received from the ECU.</summary>
        /// <param name="seed">Seed bytes from the ECU's requestSeed response.</param>
        /// <param name="accessLevel">The odd-numbered requestSeed subfunction byte (e.g. 0x01, 0x03).</param>
        /// <param name="parameters">Optional algorithm parameters; may be null.</param>
        byte[] ComputeKey(byte[] seed, byte accessLevel, byte[]? parameters);
    }
}
