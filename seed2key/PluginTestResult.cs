namespace seed2key
{
    public sealed class PluginTestResult
    {
        public bool   IsValid       { get; set; }
        public string Version       { get; set; }
        public string AlgorithmName { get; set; }
        public string SupplierName  { get; set; }
        public string Error         { get; set; }

        public PluginTestResult()
        {
            Version       = string.Empty;
            AlgorithmName = string.Empty;
            SupplierName  = string.Empty;
            Error         = string.Empty;
        }
    }
}
