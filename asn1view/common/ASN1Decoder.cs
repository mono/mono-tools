//
// ASN1Decoder.cs: ASN.1 Decoder
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Text;

// NOTE: This class is a prototype. A future version of it will be included into
// Mono.Security.dll in Mono 2.0. Use at your own risk (as the API _will_ change).

namespace Mono.Security {

	public class ASN1Element {

		private byte [] _data;
		private int _position;
		private int _valueLength;
		private int _valuePosition;
		private ArrayList _childs;

		public ASN1Element (byte[] buffer, int start) 
		{
			_data = buffer;
			_position = start;
			_valuePosition = start + 1;

			_valueLength = _data [_valuePosition++];

			// special case #1 : undefined length
			if (_valueLength == 0x80) {
				// Value.Length will have it anyway
				_valueLength = -1;
			}
			// special case where L contains the Length of the Length + 0x80
			else if ((_valueLength & 0x80) == 0x80)	{
				int len = _valueLength & 0x7F;
				_valueLength = 0;
				for (int i = 0; i < len; i++)
					_valueLength = _valueLength * 256 + _data [_valuePosition++];
			}

			if (IsConstructed && (_valueLength != 0)) {
				DecodeChilds ();
				if (_valueLength == -1) {
					// update to the true (known) length
					int childLength = 0;
					if ((_childs != null) && (_childs.Count > 0)) {
						foreach (ASN1Element child in _childs) {
							childLength += child.TotalLength;
						}
					}
					_valueLength = childLength;
				}
			}
		}

		public byte Tag {
			get { return _data [_position]; }
		}

		public int Length {
			get { return _valueLength; }
		}

		public byte[] Value {
			get {
				if (_valueLength < 0)
					return null;
				byte[] value = new byte [_valueLength];
				Buffer.BlockCopy (_data, _valuePosition, value, 0, value.Length);
				return value; 
			}
		}

		public bool IsConstructed {
			get { return ((_data [_position] & 0x20) == 0x20); }
		}

		public bool IsUndefinedLength {
			get { return (_data [_position + 1]  == 0x80); }
		}

		public int Count {
			get {
				if (_childs == null)
					return 0;
				return _childs.Count;
			}
		}

		public ASN1Element this [int index] {
			get {
				try {
					if ((index < 0) || (index >= _childs.Count))
						return null;
					return (ASN1Element)_childs [index];
				}
				catch (ArgumentOutOfRangeException) {
					return null;
				}
			}
		}
		
		public int Position {
			get { return _position; }
		}

		internal int TotalLength {
			get { return _valuePosition - _position + _valueLength; }
		}
		
		// note: Length has a variable length ;-)
		public int ValuePosition {
			get { return _valuePosition; }
		}

		private void DecodeChilds ()
		{
			_childs = new ArrayList ();
			int childpos = _valuePosition;
			int end = childpos + _valueLength;
			while ((_valueLength == -1) || (childpos < end)) {
				ASN1Element el = new ASN1Element (_data, childpos);
				_childs.Add (el);
				childpos += el.TotalLength;
				// exit condition for undefined length (_valueLength == -1)
				if ((el.Tag == 0x00) && (el.Length == 0))
					break;
			}
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			BuildString (sb, 0);
			return sb.ToString ();
		}

		internal void BuildString (StringBuilder sb, int level)
		{
			for (int i = 0; i < level; i++)	{
				sb.Append ("  ");
			}
			if (this.Value != null)	{
				if (this.Count > 0) {
					sb.AppendFormat ("Tag {0} Length {1} {2} {3}",
						this.Tag.ToString ("X2"),
						this.Length, "{",
						Environment.NewLine);

					for (int j = 0; j < this.Count; j++) {
						(this [j] as ASN1Element).BuildString (sb, level + 1);
					}
					for (int i = 0; i < level; i++) {
						sb.Append ("  ");
					}
					sb.AppendFormat ("{0} {1}", "}", Environment.NewLine);
				}
				else {
					sb.AppendFormat ("Tag {0} Length {1} Value {2} {3}",
						this.Tag.ToString ("X2"),
						this.Length,
						(this.Length == 0) ? String.Empty : BitConverter.ToString (this.Value),
						Environment.NewLine);
				}
			}
			else {
				sb.AppendFormat ("Tag {0} Length {1} {2}",
					this.Tag.ToString ("X2"),
					this.IsUndefinedLength ? "Undefined" : "0",
					Environment.NewLine);
			}
		}
	}
}
