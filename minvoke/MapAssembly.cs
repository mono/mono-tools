using System;
using System.Text;
using System.Runtime.InteropServices;

public class Advapi32 {
	[MapDllImport("advapi32.dll")]
	public static bool LogonUser (string lpszUsername,
				      string lpszDomain,
				      string lpszPassword,
				      int dwLogonType,
				      int dwLogonProvider,
				      out IntPtr phToken)
	{
		Console.WriteLine ("Advapi32:LogonUser called");
		phToken = (IntPtr)0;
		return true;
	}

	[MapDllImport("advapi32.dll")]
	public static bool DuplicateToken (IntPtr ExistingTokenHandle,
					   int SECURITY_IMPERSONATION_LEVEL,
					   out IntPtr DuplicateTokenHandle)
	{
		Console.WriteLine ("Advapi32:DuplicateToken called");
		DuplicateTokenHandle = (IntPtr)1;
		return true;
	}
}

public class User32 {
	[MapDllImport ("user32.dll")]
	public static IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam)
	{
		Console.WriteLine ("User32:SendMessage called");
		return (IntPtr)0xdead;
	}

	[MapDllImport ("user32.dll")]
	public static short GetKeyState (int nVirtKey)
	{
		Console.WriteLine ("User32:GetKeyState called");
		return 0;
	}
}

public class Kernel32 {
	[Flags]
	public enum FileSystemFeature : uint {
		CaseSensitiveSearch = 1,
		CasePreservedNames = 2,
		UnicodeOnDisk = 4,
		PersistentACLS = 8,
		FileCompression = 0x10,
		VolumeQuotas = 0x20,
		SupportsSparseFiles = 0x40,
		SupportsReparsePoints = 0x80,
		VolumeIsCompressed = 0x8000,
		SupportsObjectIDs = 0x10000,
		SupportsEncryption = 0x20000,
		NamedStreams = 0x40000,
		ReadOnlyVolume = 0x80000,
		SequentialWriteOnce = 0x100000,
		SupportsTransactions = 0x200000
	}

	[MapDllImport ("kernel32.dll")]
	public static long GetVolumeInformation (string PathName,
						 StringBuilder VolumeNameBuffer, int VolumeNameSize,
						 out uint VolumeSerialNumber,
						 out uint MaximumComponentLength,
						 out uint FileSystemFlags,
						 StringBuilder FileSystemNameBuffer,
						 uint FileSystemNameSize)
	{
		Console.WriteLine ("Kernel32:GetVolumeInformation called");
		VolumeSerialNumber = 0xdeadbeef;
		MaximumComponentLength = 128;
		FileSystemFlags = 0;

		return 1;
	}

	[MapDllImport ("kernel32.dll")]
	public static bool CloseHandle (IntPtr hObject)
	{
		Console.WriteLine ("Kernel32:CloseHandle called");
		return true;
	}

	[MapDllImport ("kernel32.dll")]
	public static bool QueryPerformanceCounter (out long performanceCount)
	{
		Console.WriteLine ("Kernel32:QueryPerformanceCounter called");
		performanceCount = 0;
		return true;
	}

	[Flags]
	public enum EXECUTION_STATE :uint {
		ES_SYSTEM_REQUIRED  = 0x00000001,
		ES_DISPLAY_REQUIRED = 0x00000002,
		// Legacy flag, should not be used.
		// ES_USER_PRESENT   = 0x00000004,
		ES_CONTINUOUS       = 0x80000000
	}

	[MapDllImport("kernel32.dll")]
	public static EXECUTION_STATE SetThreadExecutionState (EXECUTION_STATE esFlags)
	{
		Console.WriteLine ("Kernel32:SetThreadExecutionState called");
		return EXECUTION_STATE.ES_CONTINUOUS;
	}

        public struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
	}

	[MapDllImport("kernel32.dll")]
	public static bool SetLocalTime (ref SYSTEMTIME lpSystemTime)
	{
		Console.WriteLine ("Kernel32:SetLocalTime called");
		return true;
	}


	[MapDllImport("kernel32.dll")]
	public static IntPtr CreateFile (string lpFileName,
					 uint dwDesiredAccess,
					 uint dwShareMode,
					 IntPtr SecurityAttributes,
					 uint dwCreationDisposition,
					 uint dwFlagsAndAttributes,
					 IntPtr hTemplateFile)

	{
		Console.WriteLine ("Kernel32:CreateFile called");
		return IntPtr.Zero;
	}


	[MapDllImport("kernel32.dll")]
	public static bool ReadFile (IntPtr hFile, byte[] lpBuffer,
				     uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped)
	{
		Console.WriteLine ("Kernel32:ReadFile called");
		lpNumberOfBytesRead = 0;
		return true;
	}

	[MapDllImport("kernel32.dll")]
	public static bool WriteFile(IntPtr hFile, byte [] lpBuffer,
				     uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten,
				     IntPtr lpOverlapped)
	{
		Console.WriteLine ("Kernel32:WriteFile called");
		lpNumberOfBytesWritten = 0;
		return true;
	}

	[MapDllImport("kernel32.dll")]
	public static bool FlushFileBuffers (IntPtr hFile)
	{
		Console.WriteLine ("Kernel32:FlushFileBuffers called");
		return true;
	}

	public struct COMMTIMEOUTS {
		public uint ReadIntervalTimeout;
		public uint ReadTotalTimeoutMultiplier;
		public uint ReadTotalTimeoutConstant;
		public uint WriteTotalTimeoutMultiplier;
		public uint WriteTotalTimeoutConstant;
	}

	[MapDllImport("kernel32.dll")]
	public static bool SetCommTimeouts (IntPtr hFile, ref COMMTIMEOUTS lpCommTimeouts)
	{
		Console.WriteLine ("Kernel32:SetCommTimeouts called");
		return true;
	}

	[MapDllImport("kernel32.dll")]
	public static bool SetupComm (IntPtr hFile, uint dwInQueue, uint dwOutQueue)
	{
		Console.WriteLine ("Kernel32:SetupComm called");
		return true;
	}

	[MapDllImport("kernel32.dll")]
	public static bool PurgeComm (IntPtr hFile,
				      uint dwFlags)
	{
		Console.WriteLine ("Kernel32:PurgeComm called");
		return true;
	}

	public struct DCB {
		public uint DCBlength;
		public uint BaudRate;
#if once_i_figure_how_bit_sizes
		public uint fBinary  :1;
		public uint fParity  :1;
		public uint fOutxCtsFlow  :1;
		public uint fOutxDsrFlow  :1;
		public uint fDtrControl  :2;
		public uint fDsrSensitivity  :1;
		public uint fTXContinueOnXoff  :1;
		public uint fOutX  :1;
		public uint fInX  :1;
		public uint fErrorChar  :1;
		public uint fNull  :1;
		public uint fRtsControl  :2;
		public uint fAbortOnError  :1;
		public uint fDummy2  :17;
#endif
		public short wReserved;
		public short XonLim;
		public short XoffLim;
		public byte ByteSize;
		public byte Parity;
		public byte StopBits;
		public sbyte XonChar;
		public sbyte XoffChar;
		public sbyte ErrorChar;
		public sbyte EofChar;
		public sbyte EvtChar;
		public short wReserved1;
	}

	[MapDllImport("kernel32.dll")]
	public static bool GetCommState (IntPtr hFile, ref DCB lpDCB)
	{
		Console.WriteLine ("Kernel32:GetCommState called");
		return true;
	}

	[MapDllImport("kernel32.dll")]
	public static bool SetCommState(IntPtr hFile, ref DCB lpDCB)
	{
		Console.WriteLine ("Kernel32:SetCommState called");
		return true;
	}
}

