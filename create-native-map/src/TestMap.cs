// Test file for make-map.cs
using System;
using System.Runtime.InteropServices;
using System.Text;

// Make sure that a null namespace doesn't kill make-map
class GlobalClass {}

namespace MakeMap.Test {
	struct ForDelegate {int i;}
	[Map]
	delegate string MyDelegate (
			bool b1, byte b2, sbyte b3, short s1, ushort us1, 
			int i1, uint ui1, long l1, ulong ul1, 
			IntPtr p1, UIntPtr p2, string s2, StringBuilder sb1,
			HandleRef h, ForDelegate fd);

	[Map]
	enum TestEnum {
		Foo,
		Bar,
		Baz,
		Qux,
	}

	[Map, Flags]
	enum SimpleFlagsEnum {
		None  = 0,
		A     = 1,
		B     = 2,
		C     = 4,
		D     = 8,
	}

	[Map, Flags]
	enum FlagsEnum {
		None  = 0,
		A     = 1,
		B     = 2,
		C     = 4,
		D     = 8,
		All   = A | B | C | D,
		// Device types
		S_IFMT      = 0xF000, // Bits which determine file type
		[Map(SuppressFlags="S_IFMT")]
		S_IFDIR     = 0x4000, // Directory
		[Map(SuppressFlags="S_IFMT")]
		S_IFCHR     = 0x2000, // Character device
		[Map(SuppressFlags="S_IFMT")]
		S_IFBLK     = 0x6000, // Block device
		[Map(SuppressFlags="S_IFMT")]
		S_IFREG     = 0x8000, // Regular file
		[Map(SuppressFlags="S_IFMT")]
		S_IFIFO     = 0x1000, // FIFO
		[Map(SuppressFlags="S_IFMT")]
		S_IFLNK     = 0xA000, // Symbolic link
		[Map(SuppressFlags="S_IFMT")]
		S_IFSOCK    = 0xC000, // Socket
	}

	[Map ("struct foo")]
	struct Foo {
		public int foo;

		public IntPtr p;

		// this should be within a #ifdef HAVE_AUTOCONF_ME block, due to
		// --autoconf-member.
		public long autoconf_me;
	}

	[Map ("struct foo_holder")]
	struct FooHolder {
		public Foo      foo;
		public TestEnum mode;
	}

	delegate void DelFoo (int i, Foo f);
	delegate void DelRefFoo (int i, ref Foo f);
	delegate void DelArrayFoo (int i, Foo[] f);
	delegate void DelRefArrayFoo (int i, ref Foo[] f);
	delegate void DelBaz (int i, Baz b);
	delegate void DelRefBaz (int i, ref Baz b);
	delegate void DelArrayBaz (int i, Baz[] b);
	delegate void DelRefArrayBaz (int i, ref Baz[] b);

	[StructLayout (LayoutKind.Sequential)]
	class Baz {
		public DelFoo b1;
		public DelRefFoo b2;
		public DelArrayFoo b3;
		public DelRefArrayFoo b4;
		public DelBaz b5;
		public DelRefBaz b6;
		public DelArrayBaz b7;
		public DelRefArrayBaz b8;
	}

	[StructLayout (LayoutKind.Sequential)]
	class Qux {
		public int i;
		public Baz b;
	}

	class NativeMethods {
		[DllImport ("NativeLib")]
		private static extern void UseQux (DelFoo b, ref Qux q);

		// This shouldn't appear in test.h, due to --exclude-native-symbol
		[DllImport ("NativeLib")]
		private static extern void exclude_native_symbol ();
	}
}

// Testing namespace renaming; this should be NSTo within test.h
namespace MakeMap.ToBeRenamed {
	[Map]
	class Stat {
		// this should be st_atime_ in test.h due to --rename-member.
		[Map ("time_t")] public long st_atime;
	}

	[Map]
	enum Colors {
		Red, Blue, Green
	}
}

