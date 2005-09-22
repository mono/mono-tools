//
// PrettyPrinter.cs: ASN.1 PrettyPrinter
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

using Mono.Security;

namespace Mono.Tools {

	public enum OidFormat {
		ITU,
		IETF,
		URN
	}

	public enum OidSource {
		None,
		Alvestrand,
		Elibel
	}

	[Serializable]
	public struct PrettyPrinterOptions {
		public const string EmptyIdent = "  ";
		public const string DottedIndent = ". ";
		
		public bool ViewPosition;
		public bool ViewTag;
		public bool ViewLength;
		public bool DottedIndentation;
		public bool ShowTagClass;
		public bool IncludeEncapsulated;
		public OidFormat OidFormat;
		public OidSource OidSource;
		public string FontName;
		
		public void Copy (PrettyPrinterOptions options)
		{
			ViewPosition = options.ViewPosition;
			ViewTag = options.ViewTag;
			ViewLength = options.ViewLength;
			DottedIndentation = options.DottedIndentation;
			ShowTagClass = options.ShowTagClass;
			IncludeEncapsulated = options.IncludeEncapsulated;
			OidFormat = options.OidFormat;
			OidSource = options.OidSource;
			FontName = options.FontName;
		}
		
		public bool IsHeader ()
		{
			return (ViewPosition || ViewTag || ViewLength);
		}
		
		public string Indentation {
			get { return (DottedIndentation) ? DottedIndent : EmptyIdent; }
		}
		
		static public PrettyPrinterOptions GetDefaults ()
		{
			PrettyPrinterOptions ppo = new PrettyPrinterOptions ();
			ppo.ViewPosition = true;
			ppo.ViewTag = true;
			ppo.ViewLength = true;
			ppo.DottedIndentation = false;
			ppo.OidFormat = OidFormat.IETF;
			ppo.OidSource = OidSource.None;
			ppo.ShowTagClass = false;
			ppo.IncludeEncapsulated = false;
			ppo.FontName = null;
			return ppo;
		}
	}

	public class PrettyPrinter {

		private static PrettyPrinterOptions _defaults;
		private static OidCache _cache;
		
		static PrettyPrinter ()
		{
			_defaults = PrettyPrinterOptions.GetDefaults ();
			_cache = new OidCache ();
		}

		public static OidCache Cache {
			get { return _cache; }
		}

		private ASN1Element _asn;
		private PrettyPrinterOptions _options;
		private int _prefix;
		
		public PrettyPrinter (ASN1Element asn)
		{
			_asn = asn;
			_options.Copy (_defaults);
		}
		
		public ASN1Element Document {
			get { return _asn; }
			set {
				_asn = value;
				_options.Copy (_defaults);
			}
		}
		
		public PrettyPrinterOptions Options {
			get { return _options; }
			set { _options = value; }
		}
		
		public override string ToString ()
		{
			if (_asn == null)
				return String.Empty;

			StringBuilder sb = new StringBuilder ();
			_prefix = 0;
			PrintElement (sb, 0, _asn);
			return sb.ToString ();
		}
		
		// TODO > 9999 (dec) or FFFF (hex)	
		private void Header (StringBuilder sb, ASN1Element asn) 
		{
			if (asn == null) {
				if (_options.IsHeader ()) {
					if (_options.ViewPosition)
						sb.Append ("     ");
					if (_options.ViewTag)
						sb.Append ("   ");
					if (_options.ViewLength)
						sb.Append ("     ");
					sb.Append (": ");
				}
			} else {
				// _prefix is used for encapsulated data
				if (_options.ViewPosition)
					sb.AppendFormat ("{0} ", (_prefix + asn.Position).ToString ("D4"));
				if (_options.ViewTag)
					sb.AppendFormat ("{0} ", asn.Tag.ToString ("X2"));
				if (_options.ViewLength)
					sb.AppendFormat ("{0} ", (asn.IsUndefinedLength) ? "NDEF" : asn.Length.ToString ("D4"));
				if (_options.IsHeader ())
					sb.Append (": ");
			}
		}
		
		private void Indent (StringBuilder sb, int level)
		{
			for (int i = 0; i < level; i++)	{
				sb.Append (_options.Indentation);
			}
		}

		private void PrintElement (StringBuilder sb, int level, ASN1Element asn)
		{
			// used in undefined length encoding
			if (asn.Tag == 0x00)
				return;

			Header (sb, asn);
			Indent (sb, level);

			GetTagName (sb, level, asn);
			
			if (asn.Count > 0) {
				sb.Append (Environment.NewLine);
				for (int i = 0; i < asn.Count; i++)
					PrintElement (sb, level + 1, asn [i]);
			}
			
			if (asn.IsConstructed) {
				if (asn.Count > 0) {
					Header (sb, null);
					Indent (sb, level);
				}
				sb.Append ("}"); 
			}
			sb.Append (Environment.NewLine);
		}
		
		private void GetTagName (StringBuilder sb, int level, ASN1Element asn)
		{
			switch (asn.Tag) {
			case 0x01:
				PrintBoolean (sb, level, asn);
				break;
			case 0x02:
				PrintInteger (sb, level, asn);
				break;
			case 0x03:
				PrintBitString (sb, level, asn);
				break;
			case 0x04:
				PrintOctetString (sb, level, asn);
				break;
			case 0x05:
				PrintNull (sb, level, asn);
				break;
			case 0x06:
				PrintOID (sb, level, asn);
				break;
			case 0x07:
				PrintObjectDescriptor (sb, level, asn);
				break;
			case 0x08:
				PrintExternal (sb, level, asn);
				break;
			case 0x09:
				PrintReal (sb, level, asn);
				break;
			case 0x0A:
				PrintEnumerated (sb, level, asn);
				break;
			case 0x0B:
				PrintEmbeddedPDV (sb, level, asn);
				break;
			case 0x0C:
				PrintUtf8String  (sb, level, asn);
				break;
			case 0x0D:
				PrintRelativeOid (sb, level, asn);
				break;
			// 0x0E	RFU
			// 0x0F	RFU
			case 0x10:
				sb.Append ("SEQUENCE");	// non-constructed version
				break;
			case 0x11:
				sb.Append ("SET"); 	// non-constructed version
				break;
			case 0x12:
				PrintNumericString (sb, level, asn);
				break;
			case 0x13:
				PrintPrintableString  (sb, level, asn);
				break;
			case 0x14:
				PrintT61String  (sb, level, asn);
				break;
			case 0x15:
				PrintVideotexString (sb, level, asn);
				break;
			case 0x16:
				PrintIA5String (sb, level, asn);
				break;
			case 0x17:
				PrintUtcTime (sb, level, asn);
				break;
			case 0x18:
				PrintGeneralizedTime (sb, level, asn);
				break;
			case 0x19:
				PrintGraphicString (sb, level, asn);
				break;
			case 0x1A:
				PrintVisibleString (sb, level, asn);
				break;
			case 0x1B:
				PrintGeneralString (sb, level, asn);
				break;
			case 0x1C:
				PrintUniversalString (sb, level, asn);
				break;
			case 0x1D:
				PrintCharacterString (sb, level, asn);
				break;
			case 0x1E:
				PrintBMPString (sb, level, asn);
				break;
			case 0x30:
				sb.Append ("SEQUENCE {");
				break;
			case 0x31:
				sb.Append ("SET {");
				break;
			default:
				PrintOtherData (sb, level, asn);
				break;
			}
		}
		
		private void PrintEncapsulated (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append (", encapsulates {");
			sb.Append (Environment.NewLine);
			PrintElement (sb, level + 1, asn);
			Header (sb, null);
			Indent (sb, level);
			sb.Append ("}"); 
		}
		
		private void PrintData (StringBuilder sb, int level, byte[] data, int start, int length)
		{
			bool str = true;
			// first we look if the data is an ASCII string
			for (int i=0; i < length; i++) {
				if (!Char.IsLetterOrDigit ((char) data [i])) {
					str = false;
					break;
				}
			}
			
			if (str) {
				PrintString (sb, level, Encoding.ASCII.GetString (data, start, length));
			}
			else {
				PrintBinaryData (sb, level, data, start, length);
			}
		}
		
		private void PrintBinaryData (StringBuilder sb, int level, byte[] data, int start, int length)
		{
			int lines = length >> 5;		// 32 per line
			for (int i = 0; i < lines; i++) {
				sb.Append (Environment.NewLine);
				Header (sb, null);
				Indent (sb, level + 1);
				sb.Append (BitConverter.ToString (data, start, 32));
				start += 32;
			}
			int mod = (length - (lines << 5));	// remainder
			if (mod > 0) {
				sb.Append (Environment.NewLine);
				Header (sb, null);
				Indent (sb, level + 1);
				sb.Append (BitConverter.ToString (data, start, mod));
			}
		}
		
		private void PrintString (StringBuilder sb, int level, string s)
		{
			int lines = s.Length >> 6;		// 64 per line
			int start = 0;
			for (int i = 0; i < lines; i++) {
				sb.Append (Environment.NewLine);
				Header (sb, null);
				Indent (sb, level + 1);
				sb.AppendFormat ("'{0}'", s.Substring (start, 64));
				start += 64;
			}
			int mod = (s.Length - (lines << 6));	// remainder
			if (mod > 0) {
				sb.Append (Environment.NewLine);
				Header (sb, null);
				Indent (sb, level + 1);
				sb.AppendFormat ("'{0}'", s.Substring (start));
			}
		}
		
		private void PrintToDo (string s, StringBuilder sb, int level, ASN1Element asn)
		{
			sb.AppendFormat ("{0} (TODO)");
			byte[] value = asn.Value;
			PrintData (sb, level, value, 0, value.Length);
		}
		
		// 0x01 BOOLEAN
		private void PrintBoolean (StringBuilder sb, int level, ASN1Element asn)
		{
			byte[] value = asn.Value;
			sb.AppendFormat ("BOOLEAN {0}", (value [0] == 1) ? "FALSE" : "TRUE");
		}
		
		// 0x02 INTEGER
		private void PrintInteger (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("INTEGER ");
			byte[] value = asn.Value;
			// case 1 - it fits into a ulong (8 bytes)
			if (value.Length <= 8) {
				ulong integer = 0;
				for (int i=0; i < value.Length; i++)
					integer = (integer << 8) + value [i];
				sb.Append (integer);
			} else {
				// TODO: Use BigInteger class to display the beast
				PrintBinaryData (sb, level, value, 0, value.Length); 
			}
		}	

		// 0x03 BIT STRING
		private void PrintBitString (StringBuilder sb, int level, ASN1Element asn)
		{
			byte[] value = asn.Value;
			bool printed = false;
			sb.AppendFormat ("BIT STRING ({0} unused bits)", value [0]);
			if (_options.IncludeEncapsulated) {
				try {
					ASN1Element enc = new ASN1Element (value, 1);
					if ((enc.Tag <= 0x31) && (enc.Length < asn.Length)) {
						_prefix = asn.ValuePosition;
						PrintEncapsulated (sb, level, enc);
						printed = true;
					}
				}
				catch {
					// it may not be ASN.1 !
				}
				finally {
					_prefix = 0;
				}
			}
			
			if (!printed) {
				// special case for small data (e.g. bitmask)
				if (value.Length == 2) {
					sb.Append (Environment.NewLine);
					Header (sb, null);
					Indent (sb, level + 1);

					byte b = value [1];
					// skip the unused bits
					for (int i = 0; i < value [0]; i++)
						b >>= 1;
					// and show the remaining bits
					sb.Append ("'");
					for (int i = value [0]; i < 8; i++) {
						sb.Append (((b & 1) == 1) ? "1" : "0");
						b >>= 1;
					}
					sb.Append ("'B");
				} else {
					PrintData (sb, level, value, 1, value.Length - 1);
				}
			}
		}

		// 0x04 OCTET STRING
		private void PrintOctetString (StringBuilder sb, int level, ASN1Element asn)
		{
			byte[] value = asn.Value;
			bool printed = false;
			sb.Append ("OCTET STRING");
			if (_options.IncludeEncapsulated) {
				try {
					ASN1Element enc = new ASN1Element (value, 0);
					if ((enc.Tag <= 0x31) && (enc.Length < asn.Length)) {
						_prefix = asn.ValuePosition;
						PrintEncapsulated (sb, level, enc);
						printed = true;
					}
				}
				catch {
					// it may not be ASN.1 !
				}
				finally {
					_prefix = 0;
				}
			}

			if (!printed) {
				PrintData (sb, level, value, 0, value.Length);
			}
		}
		
		// 0x05 NULL
		private void PrintNull (StringBuilder sb, int level, ASN1Element asn)
		{
			// FIXME: report errors if length != 0 (in ASNElement ?)
			sb.Append ("NULL");
		}

		// 0x06 OBJECT IDENTIFIER
		private void PrintOID (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("OBJECT IDENTIFIER ");
			string oid = DecodeOid (asn.Value);
			string name = ResolveOid (oid);
			if (name.Length > 0)
				sb.AppendFormat ("{0} ", name);

			switch (_options.OidFormat) {
			case OidFormat.ITU:
				sb.AppendFormat ("({0})", oid.Replace ('.', ' '));
				break;	// 0x0C UTF8 STRING
			case OidFormat.URN:
				sb.AppendFormat ("(urn:oid:{0})", oid);
				break;
			default:
				sb.AppendFormat ("({0})", oid);
				break;
			}
		}
		
		// 0x07 OBJECT DESCRIPTOR
		private void PrintObjectDescriptor (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("ObjectDescriptor");
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}
		
		// 0x08 EXTERNAL
		private void PrintExternal (StringBuilder sb, int level, ASN1Element asn)
		{
			PrintToDo ("EXTERNAL", sb, level, asn);
		}
		
		// 0x09 REAL
		private void PrintReal (StringBuilder sb, int level, ASN1Element asn)
		{
			PrintToDo ("REAL", sb, level, asn);
		}

		// 0x0A ENUMERATED
		private void PrintEnumerated (StringBuilder sb, int level, ASN1Element asn)
		{
			PrintToDo ("ENUMERATED", sb, level, asn);
		}

		// 0x0B EMBEDDED PDV (PRESENTATION DATA VALUE)
		private void PrintEmbeddedPDV (StringBuilder sb, int level, ASN1Element asn)
		{
			PrintToDo ("EMBEDDED PDV", sb, level, asn);
		}
		
		// 0x0C UTF8 STRING
		private void PrintUtf8String (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("UTF8String");
			PrintString (sb, level, Encoding.UTF8.GetString (asn.Value));
		}

		// 0x0D RELATIVE-OID
		private void PrintRelativeOid (StringBuilder sb, int level, ASN1Element asn)
		{
			PrintToDo ("RELATIVE-OID", sb, level, asn);
		}

		// 0x12 NUMERIC STRING
		private void PrintNumericString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("NumericString");
			// FIXME: should contain only ' ' and '0' to '9'
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x13 PRINTABLE STRING
		private void PrintPrintableString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("PrintableString");	// ASCII subset
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x14 T61 STRING
		private void PrintT61String (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("TeletexString");
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x15 VIDEOTEX STRING
		private void PrintVideotexString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("VideotexString");
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x16 IA5 STRING
		private void PrintIA5String (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("IA5String");	// ASCII
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x17 UTC TIME
		private void PrintUtcTime (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.AppendFormat ("UTCTime '{0}'", Encoding.ASCII.GetString (asn.Value));
			// TODO: option to show as local time ?
		}

		// 0x18 GENERALIZED TIME
		private void PrintGeneralizedTime (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.AppendFormat ("GeneralizedTime '{0}'", Encoding.ASCII.GetString (asn.Value));
			// TODO: option to show as local time ?
		}

		// 0x19 GRAPHIC STRING
		private void PrintGraphicString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("GraphicString");
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x1A VISIBLE STRING
		private void PrintVisibleString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("VisibleString");	// ASCII subset
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x1B GENERAL STRING
		private void PrintGeneralString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("GeneralString");
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}

		// 0x1C UNIVERSAL STRING
		private void PrintUniversalString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("UniversalString");
			PrintString (sb, level, Encoding.ASCII.GetString (asn.Value));
		}
		
		// 0x1D CHARACTER STRING
		private void PrintCharacterString (StringBuilder sb, int level, ASN1Element asn)
		{
			PrintToDo ("CHARACTER STRING", sb, level, asn);
		}

		// 0x1E BMP STRING
		private void PrintBMPString (StringBuilder sb, int level, ASN1Element asn)
		{
			sb.Append ("BMPString");	// Unicode
			PrintString (sb, level, Encoding.BigEndianUnicode.GetString (asn.Value));
		}

		// Print non universal data (application, context, private)
		private void PrintOtherData (StringBuilder sb, int level, ASN1Element asn)
		{
			byte tag = asn.Tag;
			if (tag < 0x40) {
				PrintToDo ("UNKNOWN", sb, level, asn);
				return;
			}

			string type = null;
			if (tag >= 0xC0) {
				tag -= 0xC0;
				type = "PRIVATE";
			}
			if (tag >= 0x80) {
				tag -= 0x80;
				type = "CONTEXT";
			}
			if (tag >= 0x40) {
				tag -= 0x40;
				type = "APPLICATION";
			}
				
			byte[] value = asn.Value;
			
			sb.Append ("[");
			if (_options.ShowTagClass)
				sb.AppendFormat ("{0} ", type);
			if (asn.IsConstructed) {
				sb.AppendFormat ("{0}] {1}", (tag - 0x20), "{");
				// constructed data is automatically printed elsewhere
			} else {
				sb.AppendFormat ("{0}]", tag);
				PrintData (sb, level, value, 0, value.Length);
			}
		}
		
		// OID decoding / fetching stuff
		
		private string DecodeOid (byte[] oid)
		{
			StringBuilder sb = new StringBuilder ();
			// Pick apart the OID
			byte x = (byte) (oid [0] / 40);
			byte y = (byte) (oid [0] % 40);
			if (x > 2) {
				// Handle special case for large y if x = 2
				y += (byte) ((x - 2) * 40);
				x = 2;
			}
			sb.AppendFormat ("{0}.{1}", x, y);
			ulong val = 0;
			for (x = 1; x < oid.Length; x++) {
				val = ((val << 7) | ((byte) (oid [x] & 0x7F)));
				if ( !((oid [x] & 0x80) == 0x80)) {
					sb.AppendFormat (".{0}", val);
					val = 0;
				}
			}
			return sb.ToString ();
		}

		private string ResolveOid (string oid)
		{
			// 1. try to load from cache
			string name = _cache.Get (oid);
			if (name != null)
				return name;
				
			// 2. if not found then download from the web	
			switch (_options.OidSource) {
			case OidSource.Alvestrand:
				name = GetOidInfoFromAlvestrand (oid);
				_cache.Add (oid, name);
				break;
			case OidSource.Elibel:
				name = GetOidInfoFromElibel (oid);
				_cache.Add (oid, name);
				break;
			default:
				name = String.Empty;
				break;
			}

			return name;
		}
		
		private string GetOidInfoFromAlvestrand (string oid)
		{
			string name = String.Empty;
			try {
				string url = String.Concat ("http://www.alvestrand.no/objectid/", oid, ".html");
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				WebResponse resp = req.GetResponse ();
				Stream s = resp.GetResponseStream ();
				StreamReader sr = new StreamReader (s, Encoding.UTF8);
				string webpage = sr.ReadToEnd ();
				int end = webpage.IndexOf ("</h1>");
				if (end != -1) {
					int start = webpage.LastIndexOf (' ', end);
					if (start != -1) {
						name = webpage.Substring (start + 1, end - start - 1);
					}
				} 
			}
			catch (Exception) {
				// many things can go wrong but it's only some 
				// "additional" (not essential) data
				// FIXME: report error to UI
			}
			return name;
		}
		
		private string GetOidInfoFromElibel (string oid)
		{
			string name = String.Empty;
			try {
				string url = String.Concat ("http://asn1.elibel.tm.fr/cgi-bin/oid/display?oid=", oid, "&action=display");
				HttpWebRequest req = (HttpWebRequest) WebRequest.Create (url);
				WebResponse resp = req.GetResponse ();
				Stream s = resp.GetResponseStream ();
				StreamReader sr = new StreamReader (s, Encoding.UTF8);
				string webpage = sr.ReadToEnd ();
				int end = webpage.IndexOf (")}</title>");
				if (end != -1) {
					end = webpage.LastIndexOf ("(", end);
					int start = webpage.LastIndexOf (' ', end);
					if (start != -1) {
						name = webpage.Substring (start + 1, end - start - 1);
					}
				} 
			}
			catch (Exception) {
				// many things can go wrong but it's only some 
				// "additional" (not essential) data
				// FIXME: report error to UI
			}
			return name;
		}
	}
}
