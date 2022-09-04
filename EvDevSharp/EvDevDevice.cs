using System.ComponentModel;
using System.Runtime.InteropServices;
using EvDevSharp.Enums;
using EvDevSharp.InteropStructs;
using static EvDevSharp.IoCtlRequest;

namespace EvDevSharp;

public sealed unsafe partial class EvDevDevice : IDisposable {
	private const string InputPath              = "/dev/input/";
	private const string InputPathSearchPattern = "event*";

	private EvDevDevice(string path) {
		using FileStream eventFile = File.OpenRead(path);
		IntPtr           fd        = eventFile.SafeFileHandle!.DangerousGetHandle();

		this.DevicePath = path;

		int version = 0;
		if (ioctl(fd, EVIOCGVERSION, &version) == -1)
			throw new Win32Exception($"Unable to get evdev driver version for {path}");

		this.DriverVersion = new Version(version >> 16, version >> 8 & 0xff, version & 0xff);

		ushort* id = stackalloc ushort[4];

		if (ioctl(fd, EVIOCGID, id) == -1)
			throw new Win32Exception($"Unable to get evdev id for {path}");

		this.Id = new EvDevDeviceId {
			Bus     = id[0],
			Vendor  = id[1],
			Product = id[2],
			Version = id[3]
		};

		byte* str = stackalloc byte[256];
		if (ioctl(fd, Eviocgname(256), str) == -1)
			throw new Win32Exception($"Unable to get evdev name for {path}");

		this.Name = Marshal.PtrToStringAnsi(new IntPtr(str));

		//TODO: figure out why this is commented out
		// if (ioctl(fd, new CULong(EVIOCGUNIQ(256)), str) == -1)
		//     throw new Win32Exception($"Unable to get evdev unique ID for {path}");

		// UniqueId = Marshal.PtrToStringAnsi(new IntPtr(str));

		this.UniqueId = "Unknown";
		
		const int bitCount = (int)EvDevKeyCode.KeyMax;
		byte[]    bits     = new byte[bitCount / 8 + 1];

		ioctl(fd, Eviocgbit(EvDevEventType.EvSyn, bitCount), bits);
		List<EvDevEventType> supportedEvents = DecodeBits(bits).Cast<EvDevEventType>().ToList();
		foreach (EvDevEventType evType in supportedEvents) {
			if (evType == EvDevEventType.EvSyn)
				continue;

			Array.Clear(bits, 0, bits.Length);
			ioctl(fd, Eviocgbit(evType, bitCount), bits);
			this.RawEventCodes[evType] = DecodeBits(bits);
		}

		if (this.RawEventCodes.TryGetValue(EvDevEventType.EvKey, out List<int>? keys))
			this.Keys = keys.Cast<EvDevKeyCode>().ToList();

		if (this.RawEventCodes.TryGetValue(EvDevEventType.EvRel, out List<int>? rel))
			this.RelativeAxises = rel.Cast<EvDevRelativeAxisCode>().ToList();

		if (this.RawEventCodes.TryGetValue(EvDevEventType.EvAbs, out List<int>? abs))
			this.AbsoluteAxises = abs.ToDictionary(
				x => (EvDevAbsoluteAxisCode)x,
				x => {
					EvDevAbsAxisInfo absInfo = default;
					ioctl(fd, Eviocgabs(x), &absInfo);
					return absInfo;
				});

		Array.Clear(bits, 0, bits.Length);
		ioctl(fd, Eviocgprop((int)EvDevProperty.InputPropCnt), bits);
		this.Properties = DecodeBits(bits, (int)EvDevProperty.InputPropCnt).Cast<EvDevProperty>().ToList();

		this.GuessedDeviceType = this.GuessDeviceType();
	}

	public EvDevDeviceId Id            { get; }
	public string?       UniqueId      { get; }
	public Version       DriverVersion { get; }
	public string?       Name          { get; }
	public string        DevicePath    { get; }

	public EvDevGuessedDeviceType                               GuessedDeviceType { get; set; }
	public Dictionary<EvDevEventType, List<int>>                RawEventCodes     { get; } = new();
	public List<EvDevKeyCode>?                                  Keys              { get; set; }
	public List<EvDevRelativeAxisCode>?                         RelativeAxises    { get; set; }
	public Dictionary<EvDevAbsoluteAxisCode, EvDevAbsAxisInfo>? AbsoluteAxises    { get; set; }
	public List<EvDevProperty>                                  Properties        { get; set; }

	private static List<int> DecodeBits(byte[] arr, int? max = null) {
		List<int> rv = new();
		max ??= arr.Length * 8;
		for (int idx = 0; idx < max; idx++) {
			byte b     = arr[idx / 8];
			int  shift = idx % 8;
			int  v     = b >> shift & 1;
			if (v != 0)
				rv.Add(idx);
		}

		return rv;
	}

	private EvDevGuessedDeviceType GuessDeviceType() {
		if (this.Name != null) {
			// Often device name says what it is
			bool isAbsolutePointingDevice = this.AbsoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.AbsX) == true;

			string n = this.Name.ToLowerInvariant();
			if (n.Contains("touchscreen")
			    && isAbsolutePointingDevice
			    && this.Keys?.Contains(EvDevKeyCode.BtnTouch) == true)
				return EvDevGuessedDeviceType.TouchScreen;

			if (n.Contains("tablet")
			    && isAbsolutePointingDevice
			    && this.Keys?.Contains(EvDevKeyCode.BtnLeft) == true)
				return EvDevGuessedDeviceType.Tablet;

			if (n.Contains("touchpad")
			    && isAbsolutePointingDevice
			    && this.Keys?.Contains(EvDevKeyCode.BtnLeft) == true)
				return EvDevGuessedDeviceType.TouchPad;

			if (n.Contains("keyboard")
			    && this.Keys != null)
				return EvDevGuessedDeviceType.Keyboard;

			if (n.Contains("gamepad") || n.Contains("joystick")
			    && this.Keys != null)
				return EvDevGuessedDeviceType.GamePad;
		}

		if (this.Keys?.Contains(EvDevKeyCode.BtnTouch) == true
		    && this.Properties.Contains(EvDevProperty.InputPropDirect))
			return EvDevGuessedDeviceType.TouchScreen;

		if (this.Keys?.Contains(EvDevKeyCode.BtnSouth) == true)
			return EvDevGuessedDeviceType.GamePad;

		if (this.Keys?.Contains(EvDevKeyCode.BtnLeft) == true && this.Keys?.Contains(EvDevKeyCode.BtnRight) == true) {
			if (this.AbsoluteAxises != null)
				if (this.AbsoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.AbsX) == true) {
					if (this.Properties.Contains(EvDevProperty.InputPropDirect))
						return EvDevGuessedDeviceType.Tablet;

					return EvDevGuessedDeviceType.TouchPad;
				}

			if (this.RelativeAxises?.Contains(EvDevRelativeAxisCode.RelX) == true && this.RelativeAxises.Contains(EvDevRelativeAxisCode.RelY))
				return EvDevGuessedDeviceType.Mouse;
		}

		if (this.Keys != null)
			return EvDevGuessedDeviceType.Keyboard;

		return EvDevGuessedDeviceType.Unknown;
	}

	/// <summary>
	///     This method enumerates all of the Linux event files and generates a <c>EvDevDevice</c> object for each file.
	/// </summary>
	/// <exception cref="System.PlatformNotSupportedException"></exception>
	public static IEnumerable<EvDevDevice> GetDevices() {
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			throw new PlatformNotSupportedException();

		return Directory.GetFiles(InputPath, InputPathSearchPattern)
			.AsParallel()
			.Select(path => new EvDevDevice(path));
	}
}
