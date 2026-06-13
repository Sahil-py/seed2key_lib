namespace seed2key
{
    /// <summary>
    /// UDS Security Access subfunction levels (requestSeed value).
    /// Each odd value is the requestSeed subfunction; the corresponding
    /// sendKey subfunction is requestSeed + 1.
    /// </summary>
    public enum SecurityAccessLevel : byte
    {
        Programming      = 0x01,   // 0x01/0x02
        VariantCoding    = 0x03,   // 0x03/0x04
        UnlockingControl = 0x05,   // 0x05/0x06
        EngineeringCtrl  = 0x07,   // 0x07/0x08
        SupplierEOL      = 0x61,   // 0x61/0x62
    }
}
