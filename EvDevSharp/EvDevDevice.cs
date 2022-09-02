using System.ComponentModel;
using System.Runtime.InteropServices;
using EvDevSharp.InteropStructs;
using static EvDevSharp.IoCtlRequest;

namespace EvDevSharp;

public sealed unsafe partial class EvDevDevice : IDisposable {
	private const string InputPath              = "/dev/input/";
	private const string InputPathSearchPattern = "event*";

	private EvDevDevice(string path) {
		using FileStream eventFile = File.OpenRead(path);
		IntPtr           fd        = eventFile.SafeFileHandle.DangerousGetHandle();

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
		if (ioctl(fd, EVIOCGNAME(256), str) == -1)
			throw new Win32Exception($"Unable to get evdev name for {path}");

		this.Name = Marshal.PtrToStringAnsi(new IntPtr(str));

		// if (ioctl(fd, new CULong(EVIOCGUNIQ(256)), str) == -1)
		//     throw new Win32Exception($"Unable to get evdev unique ID for {path}");

		// UniqueId = Marshal.PtrToStringAnsi(new IntPtr(str));

		int    bitCount = (int)EvDevKeyCode.KEY_MAX;
		byte[] bits     = new byte[bitCount / 8 + 1];

		ioctl(fd, EVIOCGBIT(EvDevEventType.EV_SYN, bitCount), bits);
		List<EvDevEventType> supportedEvents = DecodeBits(bits).Cast<EvDevEventType>().ToList();
		foreach (EvDevEventType evType in supportedEvents) {
			if (evType == EvDevEventType.EV_SYN)
				continue;

			Array.Clear(bits, 0, bits.Length);
			ioctl(fd, EVIOCGBIT(evType, bitCount), bits);
			this.RawEventCodes[evType] = DecodeBits(bits);
		}

		if (this.RawEventCodes.TryGetValue(EvDevEventType.EV_KEY, out List<int>? keys))
			this.Keys = keys.Cast<EvDevKeyCode>().ToList();

		if (this.RawEventCodes.TryGetValue(EvDevEventType.EV_REL, out List<int>? rel))
			this.RelativeAxises = rel.Cast<EvDevRelativeAxisCode>().ToList();

		if (this.RawEventCodes.TryGetValue(EvDevEventType.EV_ABS, out List<int>? abs))
			this.AbsoluteAxises = abs.ToDictionary(
				x => (EvDevAbsoluteAxisCode)x,
				x => {
					EvDevAbsAxisInfo absInfo = default;
					ioctl(fd, EVIOCGABS(x), &absInfo);
					return absInfo;
				});

		Array.Clear(bits, 0, bits.Length);
		ioctl(fd, EVIOCGPROP((int)EvDevProperty.INPUT_PROP_CNT), bits);
		this.Properties = DecodeBits(bits, (int)EvDevProperty.INPUT_PROP_CNT).Cast<EvDevProperty>().ToList();

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
			bool isAbsolutePointingDevice = this.AbsoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.ABS_X) == true;

			string n = this.Name.ToLowerInvariant();
			if (n.Contains("touchscreen")
			    && isAbsolutePointingDevice
			    && this.Keys?.Contains(EvDevKeyCode.BTN_TOUCH) == true)
				return EvDevGuessedDeviceType.TouchScreen;

			if (n.Contains("tablet")
			    && isAbsolutePointingDevice
			    && this.Keys?.Contains(EvDevKeyCode.BTN_LEFT) == true)
				return EvDevGuessedDeviceType.Tablet;

			if (n.Contains("touchpad")
			    && isAbsolutePointingDevice
			    && this.Keys?.Contains(EvDevKeyCode.BTN_LEFT) == true)
				return EvDevGuessedDeviceType.TouchPad;

			if (n.Contains("keyboard")
			    && this.Keys != null)
				return EvDevGuessedDeviceType.Keyboard;

			if (n.Contains("gamepad") || n.Contains("joystick")
			    && this.Keys != null)
				return EvDevGuessedDeviceType.GamePad;
		}

		if (this.Keys?.Contains(EvDevKeyCode.BTN_TOUCH) == true
		    && this.Properties.Contains(EvDevProperty.INPUT_PROP_DIRECT))
			return EvDevGuessedDeviceType.TouchScreen;

		if (this.Keys?.Contains(EvDevKeyCode.BTN_SOUTH) == true)
			return EvDevGuessedDeviceType.GamePad;

		if (this.Keys?.Contains(EvDevKeyCode.BTN_LEFT) == true && this.Keys?.Contains(EvDevKeyCode.BTN_RIGHT) == true) {
			if (this.AbsoluteAxises != null)
				if (this.AbsoluteAxises?.ContainsKey(EvDevAbsoluteAxisCode.ABS_X) == true) {
					if (this.Properties.Contains(EvDevProperty.INPUT_PROP_DIRECT))
						return EvDevGuessedDeviceType.Tablet;

					return EvDevGuessedDeviceType.TouchPad;
				}

			if (this.RelativeAxises?.Contains(EvDevRelativeAxisCode.REL_X) == true && this.RelativeAxises.Contains(EvDevRelativeAxisCode.REL_Y))
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
