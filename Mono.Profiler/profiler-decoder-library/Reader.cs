// Author:
// Massimiliano Mantione (massi@ximian.com)
//
// (C) 2008 Novell, Inc  http://www.novell.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections.Generic;

namespace  Mono.Profiler {
	public interface ILogFileReader {
		BlockData ReadBlock ();
		bool HasEnded {get;}
	}
	
	public interface IDataBlockRecycler {
		void RecycleData (byte [] data);
		byte [] NewData (int size);
	}
	
	public class SyncLogFileReader : ILogFileReader, IDataBlockRecycler {
		System.IO.FileStream stream;
		bool hasEnded = false;
		public bool HasEnded {
			get {
				return hasEnded;
			}
		}
		ulong counter = 0;
		
		byte[] cachedData;
		public virtual void RecycleData (byte [] data) {
			if (cachedData == null || cachedData.Length < data.Length) {
				cachedData = data;
			}
		}
		public virtual byte[] NewData (int size) {
			if (cachedData != null && cachedData.Length > size) {
				byte[] result = cachedData;
				cachedData = null;
				return result;
			} else {
				return new byte[size];
			}
		}
		
		uint fileOffset = 0;
		
		public SyncLogFileReader (System.IO.FileStream stream) {
			this.stream = stream;
		}
		public SyncLogFileReader (string fileName) {
			this.stream = new System.IO.FileStream (fileName, System.IO.FileMode.Open);
		}
		
		public void Close () {
			stream.Close ();
		}

		public BlockData ReadBlock () {
			if (! hasEnded) {
				byte [] header;
				byte [] block;
				BlockCode code;
				int length;
				int bytesRead;
				BlockData result = null;
				
				header = new byte [BlockData.BLOCK_HEADER_SIZE];
				bytesRead = stream.Read (header, 0, BlockData.BLOCK_HEADER_SIZE);
				if (bytesRead == 0) {
					return null;
				} else if (bytesRead < BlockData.BLOCK_HEADER_SIZE) {
					throw new DecodingException (result, 0, String.Format ("Invalid header: length is {0} instead of {1}", bytesRead, BlockData.BLOCK_HEADER_SIZE));
				}
				fileOffset += (uint) BlockData.BLOCK_HEADER_SIZE;
				
				code = BlockData.DecodeHeaderBlockCode (header);
				length = BlockData.DecodeHeaderBlockLength (header);
				if (code == BlockCode.END) {
					hasEnded = true;
				}
				counter += BlockData.DecodeHeaderBlockCounterDelta (header);
				
				block = NewData (length);
				stream.Read (block, 0, length);
				result =  new BlockData (fileOffset, code, length, counter, block);
				fileOffset += (uint) length;
				                         
				return result;
			} else {
				return null;
			}
		}
	}

	public class SeekableLogFileReader : IDataBlockRecycler {
		public class Block {
			uint fileOffset;
			public uint FileOffset {
				get {
					return fileOffset;
				}
			}
			
			BlockCode code;
			public BlockCode Code {
				get {
					return code;
				}
			}
			
			uint length;
			public uint Length {
				get {
					return length;
				}
			}
			
			ulong counter;
			public ulong Counter {
				get {
					return counter;
				}
			}
			
			TimeSpan timeFromStart;
			public TimeSpan TimeFromStart {
				get {
					return timeFromStart;
				}
				internal set {
					timeFromStart = value;
				}
			}
			
			public Block (uint fileOffset, BlockCode code, uint length, ulong counter) {
				this.fileOffset = fileOffset;
				this.code = code;
				this.length = length;
				this.counter = counter;
			}
		}
		
		ulong startCounter;
		public ulong StartCounter {
			get {
				return startCounter;
			}
		}
		
		DateTime startTime;
		public DateTime StartTime {
			get {
				return startTime;
			}
		}
		
		ulong endCounter;
		public ulong EndCounter {
			get {
				return endCounter;
			}
		}
		
		DateTime endTime;
		public DateTime EndTime {
			get {
				return endTime;
			}
		}
		
		byte[] cachedData;
		public virtual void RecycleData (byte [] data) {
			if (cachedData == null || cachedData.Length < data.Length) {
				cachedData = data;
			}
		}
		public virtual byte[] NewData (int size) {
			if (cachedData != null && cachedData.Length > size) {
				byte[] result = cachedData;
				cachedData = null;
				return result;
			} else {
				return new byte[size];
			}
		}
		
		System.IO.FileStream stream;
		
		Block[] blocks;
		public Block[] Blocks {
			get {
				return blocks;
			}
		}
		
		void InitializeBlocks () {
			uint fileOffset = 0;
			bool hasEnded = false;
			List<Block> result = new List<Block> ();
			ulong counter = 0;
			byte [] header = new byte [BlockData.BLOCK_HEADER_SIZE];
			ProfilerEventHandler eventProcessor = new ProfilerEventHandler ();
			
			while (! hasEnded) {
				int bytesRead = stream.Read (header, 0, BlockData.BLOCK_HEADER_SIZE);
				if (bytesRead != BlockData.BLOCK_HEADER_SIZE) {
					if (bytesRead == 0) {
						Console.WriteLine ("WARNING: File truncated at offset {0} without end block", fileOffset);
						break;
					} else {
						throw new Exception (String.Format ("At file offset {0} block header is not complete", fileOffset));
					}
				}
				fileOffset += (uint) BlockData.BLOCK_HEADER_SIZE;
				counter += BlockData.DecodeHeaderBlockCounterDelta (header);
				
				Block block = new Block (fileOffset, BlockData.DecodeHeaderBlockCode (header), (uint) BlockData.DecodeHeaderBlockLength (header), counter);
				result.Add (block);
				
				fileOffset += block.Length;
				stream.Seek (fileOffset, SeekOrigin.Begin);
				if (block.Code == BlockCode.INTRO) {
					ReadBlock (block).Decode (eventProcessor);
					startCounter = eventProcessor.StartCounter;
					startTime = eventProcessor.StartTime;
				}
				if (block.Code == BlockCode.END) {
					hasEnded = true;
					ReadBlock (block).Decode (eventProcessor);
					endCounter = eventProcessor.EndCounter;
					endTime = eventProcessor.EndTime;
				}
			}
			
			blocks = result.ToArray ();
			
			foreach (Block block in blocks) {
				block.TimeFromStart = eventProcessor.ClicksToTimeSpan (block.Counter);
			}
		}
		
		public BlockData ReadBlock (Block block) {
			stream.Seek (block.FileOffset, SeekOrigin.Begin);
			byte[] data = NewData ((int) block.Length);
			stream.Read (data, 0, (int) block.Length);
			return new BlockData (block.FileOffset, block.Code, (int) block.Length, block.Counter, data);
		}
		
		public SeekableLogFileReader (System.IO.FileStream stream) {
			this.stream = stream;
			InitializeBlocks ();
		}
		public SeekableLogFileReader (string fileName) {
			this.stream = new System.IO.FileStream (fileName, System.IO.FileMode.Open);
			InitializeBlocks ();
		}
	}
	
#if false
	public class AsyncLogFileReader {
		System.IO.FileStream stream;
		System.IAsyncResult nextHeaderOperation;
		System.IAsyncResult nextBlockOperation;
		byte [] nextHeader;
		byte [] nextBlock;
		bool hadEnded;
		
		public void ReadBlock () {
			if (! hadEnded) {
			} else {
				
			}
		}
		
	}
#endif
}
