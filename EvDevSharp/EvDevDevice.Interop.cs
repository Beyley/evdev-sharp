using System.Runtime.InteropServices;

namespace EvDevSharp;

public unsafe partial class EvDevDevice {
	[DllImport("libc", SetLastError = true)]
	private static extern int ioctl(IntPtr fd, nuint request, void* data);

	[DllImport("libc", SetLastError = true)]
	private static extern int ioctl(IntPtr fd, nuint request, [Out] byte[] data);
}
