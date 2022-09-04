using EvDevSharp.Enums;
// ReSharper disable UnusedMember.Local

namespace EvDevSharp;

internal static class IoCtlRequest {
	private const int IocSizebits = 14;
	private const int IocDirbits  = 2;
	private const int IocNrbits   = 8;
	private const int IocTypebits = 8;

	private const int IocNrmask   = (1 << IocNrbits)   - 1;
	private const int IocTypemask = (1 << IocTypebits) - 1;
	private const int IocSizemask = (1 << IocSizebits) - 1;
	private const int IocDirmask  = (1 << IocDirbits)  - 1;

	private const int IocNrshift   = 0;
	private const int IocTypeshift = IocNrshift   + IocNrbits;
	private const int IocSizeshift = IocTypeshift + IocTypebits;
	private const int IocDirshift  = IocSizeshift + IocSizebits;

	private const int IocNone  = 0;
	private const int IocWrite = 1;
	private const int IocRead  = 2;

	public const uint EVIOCGVERSION    = 2147763457;
	public const uint EVIOCGID         = 2148025602;
	public const uint EVIOCGREP        = 2148025603;
	public const uint EVIOCSREP        = 1074283779;
	public const uint EVIOCGKEYCODE    = 2148025604;
	public const uint EVIOCGKEYCODE_V2 = 2150122756;
	public const uint EVIOCSKEYCODE    = 1074283780;
	public const uint EVIOCSKEYCODE_V2 = 1076380932;


	private static uint _IOC(long dir, long type, long nr, long size) {
		return (uint)(dir  << IocDirshift  |
		              type << IocTypeshift |
		              nr   << IocNrshift   |
		              size << IocSizeshift);
	}

	private static uint _IO(long type, long nr) {
		return _IOC(IocNone, type, nr, 0);
	}
	private static uint _IOR(long type, long nr, long size) {
		return _IOC(IocRead, type, nr, size);
	}
	private static uint _IOW(long type, long nr, long size) {
		return _IOC(IocWrite, type, nr, size);
	}

	public static uint Eviocgname(long len) {
		return _IOC(IocRead, 'E', 0x06, len);
	}
	public static uint Eviocgphys(long len) {
		return _IOC(IocRead, 'E', 0x07, len);
	}
	public static uint Eviocguniq(long len) {
		return _IOC(IocRead, 'E', 0x08, len);
	}
	public static uint Eviocgprop(long len) {
		return _IOC(IocRead, 'E', 0x09, len);
	}
	public static uint Eviocgbit(EvDevEventType ev, long len) {
		return _IOC(IocRead, 'E', 0x20 + (long)ev, len);
	}
	public static uint Eviocgabs(int abs) {
		return _IOR('E', 0x40 + abs, 24);
	}
}
