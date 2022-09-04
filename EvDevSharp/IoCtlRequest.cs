using EvDevSharp.Enums;
// ReSharper disable UnusedMember.Local

namespace EvDevSharp;

internal static class IoCtlRequest {
    private const int IOC_SIZEBITS = 14;
    private const int IOC_DIRBITS  = 2;
    private const int IOC_NRBITS   = 8;
    private const int IOC_TYPEBITS = 8;

    private const int IOC_NRMASK   = (1 << IOC_NRBITS) - 1;
    private const int IOC_TYPEMASK = (1 << IOC_TYPEBITS) - 1;
    private const int IOC_SIZEMASK = (1 << IOC_SIZEBITS) - 1;
    private const int IOC_DIRMASK  = (1 << IOC_DIRBITS) - 1;

    private const int IOC_NRSHIFT   = 0;
    private const int IOC_TYPESHIFT = IOC_NRSHIFT + IOC_NRBITS;
    private const int IOC_SIZESHIFT = IOC_TYPESHIFT + IOC_TYPEBITS;
    private const int IOC_DIRSHIFT  = IOC_SIZESHIFT + IOC_SIZEBITS;

    private const int IOC_NONE  = 0;
    private const int IOC_WRITE = 1;
    private const int IOC_READ  = 2;

    public const uint EVIOCGVERSION    = 2147763457;
    public const uint EVIOCGID         = 2148025602;
    public const uint EVIOCGREP        = 2148025603;
    public const uint EVIOCSREP        = 1074283779;
    public const uint EVIOCGKEYCODE    = 2148025604;
    public const uint EVIOCGKEYCODE_V2 = 2150122756;
    public const uint EVIOCSKEYCODE    = 1074283780;
    public const uint EVIOCSKEYCODE_V2 = 1076380932;


    private static uint _IOC(long dir, long type, long nr, long size) => (uint)(dir << IOC_DIRSHIFT | type << IOC_TYPESHIFT | nr << IOC_NRSHIFT | size << IOC_SIZESHIFT);

    private static uint _IO(long type, long nr) => _IOC(IOC_NONE,              type, nr, 0);
    private static uint _IOR(long type, long nr, long size) => _IOC(IOC_READ,  type, nr, size);
    private static uint _IOW(long type, long nr, long size) => _IOC(IOC_WRITE, type, nr, size);

    public static uint Eviocgname(long len) => _IOC(IOC_READ,                   'E', 0x06,            len);
    public static uint Eviocgphys(long len) => _IOC(IOC_READ,                   'E', 0x07,            len);
    public static uint Eviocguniq(long len) => _IOC(IOC_READ,                   'E', 0x08,            len);
    public static uint Eviocgprop(long len) => _IOC(IOC_READ,                   'E', 0x09,            len);
    public static uint Eviocgbit(EvDevEventType ev, long len) => _IOC(IOC_READ, 'E', 0x20 + (long)ev, len);
    public static uint Eviocgabs(int abs) => _IOR('E', 0x40 + abs, 24);
}
