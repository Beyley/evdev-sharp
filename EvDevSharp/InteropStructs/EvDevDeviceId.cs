namespace EvDevSharp.InteropStructs;

public struct EvDevDeviceId {
    public ushort Bus { get; set; }
    public ushort Vendor { get; set; }
    public ushort Product { get; set; }
    public ushort Version { get; set; }

    public override string ToString() => $"Bus: 0x{this.Bus:x} Vendor: 0x{this.Vendor:x} Product: 0x{this.Product:x} Version: 0x{this.Version:x}";
}
