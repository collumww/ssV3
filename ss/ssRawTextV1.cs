using System;
using System.Text;

namespace ss {
	/// <summary>
	/// Summary description for ssText.
	/// </summary>
	public class ssRawTextV1 {
		public ssRawTextV1(string s) {
			first = new blk();
			last = first;
			curblk = first;
			curoff = 0;
			curpos = 0;
			len = 0;
		}

		public const int blksiz = 2048;
		private const int fill = blksiz * 3 / 4;

		private class blk {
			public blk prv;
			public int n;
			public char[] data;
			public blk nxt;
			public blk() {
				prv = null;
				nxt = null;
				n = 0;
				data = new char[blksiz];
			}
		}

		private blk first;
		private blk last;
		private blk curblk;
		private int curoff;		// offset into current block
		private int curpos;		// current overall file position
		private int len;		// current length of file


		private void RangeErr() {
			throw new ArgumentOutOfRangeException("i", "", "Parameter out of range in ssText");
		}

		private void Seek(int i) {
			if (i < 0 || len < i) {RangeErr();}
			if (i == 0) {curblk = first; curoff = 0; curpos = 0; return;}
			if (i == len) {curblk = last; curoff = curblk.n; curpos = len; return;}
			if (i > curpos) {
				curpos -= curoff;
				while (i >= curpos + curblk.n) {
					curpos += curblk.n;
					if (curblk.nxt != null) {
						curblk = curblk.nxt;
					} else {
						curoff = curblk.n;
						return;
					}
				}
				curoff = i - curpos;
			} else if (i < curpos) {
				curpos -= curoff;
				while (i < curpos) {
					if (curblk.prv != null) {
						curblk = curblk.prv;
					} else {
						curoff = 0;
						return;
					}
					curpos -= curblk.n;
				}
				curoff = i - curpos;
			}
			curpos = i;
		}

		public void Insert(int i, char c) {
			string s = c.ToString();
			Insert(i, s);
		}

		private void StrIns(ref string s, int soff, blk b, int o, int m) {
			int src = b.n;
			int dst = src + m;
			while (src > o) {
				b.data[--dst] = b.data[--src];
			}
			src = soff;
			dst = o;
			int i = 0;
			while (i++ < m) {
				b.data[dst++] = s[src++];
			}
			b.n += m;
			len += m;
		}

		private blk BlkIns(blk a) {
			blk b = new blk();
			b.nxt = a;
			b.prv = a.prv;
			if (b.prv != null) {
				b.prv.nxt = b;
			} else {
				first = b;
			}
			a.prv = b;
			return b;
		}

		private blk BlkApp(blk a) {
			blk b = new blk();
			b.nxt = a.nxt;
			b.prv = a;
			if (b.nxt != null) {
				b.nxt.prv = b;
			} else {
				last = b;
			}
			a.nxt = b;
			return b;
		}

		private void BlkSplit(blk a, int o) {
			blk b = BlkApp(a);
			int src = o;
			int dst = 0;
			while (src < a.n) {
				b.data[dst++] = a.data[src++];
			}
			a.n = o;
			b.n = dst;
		}

		public void Insert(int i, string s) {
			Seek(i);
			if (curoff == 0 && curblk.prv != null) 
			{
				curblk = curblk.prv;
				curoff = curblk.n;
			}
			int o = curoff;
			blk b = curblk;
			int soff = 0;
			int n = s.Length;
			while (n > 0) {
				if (n + b.n <= blksiz && b.n < fill) {			// it fits in the block
					StrIns(ref s, soff, b, o, n);
					n = 0;
				} else if (o != 0 && o != b.n) {	// doesn't fit, in the middle
					BlkSplit(b, o);
				} else if (o == b.n) {				// doesn't fit, at the end
					if (b.n < fill) {						// filled up enough?
						int dn = fill - b.n;
						StrIns(ref s, soff, b, o, dn);
						n -= dn;
						o += dn;
						soff += dn;
					} else {								// too filled, add block
						b = BlkApp(b);
						o = 0;
					}
				} else {							// doesn't fit, at the beginning
					b = BlkIns(b);
					o = 0;
					curblk = b;
				}
			}
			if (curoff == curblk.n && curblk.nxt != null) {
				curblk = curblk.nxt;
				curoff = 0;
			}
		}

		private void StrDel(blk b, int o, int n) {
			int src = o + n;
			while (src < b.n) {
				b.data[o++] = b.data[src++];
			}
			b.n -= n;
			len -= n;
		}

		private blk BlkDel(blk b) {
			len -= b.n;
			
			if (b.nxt == null) {b.n = 0; return b;}
			else {
				b.nxt.prv = b.prv;
				if (b.prv != null) {
					b.prv.nxt = b.nxt;
				} else {
					first = b.nxt;
				}
				return b.nxt;
			}
		}

		public void Remove(int i, int n) {
			if (i + n > len) {RangeErr();}
			Seek(i);
			blk b = curblk;
			int o = curoff;
			while (n > 0) {
				if (o == 0 && n >= b.n) {		// at beginning, deleting all or more
					n -= b.n;
					b = BlkDel(b);
					curblk = b;
					curoff = 0;
				} else {
					if (o + n < b.n) {			// beginning or middle, deleting less than all
						StrDel(b, o, n);
						n = 0;
					} else {					// middle or end, deleting all or more
						int dn = b.n - o;
						n -= dn;
						len -= dn;
						b.n = o;
						if (n > 0) {
							b = b.nxt;
							o = 0;
						}
					}
				}
			}
		}

		public string ToString(int i, int n) {
			if (i + n > len) {RangeErr();}
			Seek(i);
			int o = curoff;
			blk b = curblk;
			StringBuilder s = new StringBuilder("", n);
			for (int j = 0; j < n; j++) {
				if (o == b.n) {
					b = b.nxt;
					o = 0;
				}
				s.Append(b.data[o++]);
			}
			return s.ToString();
		}

		public char data(int i) {
			if (i >= len) {RangeErr();}
			Seek(i);
			return curblk.data[curoff];
		}

		public char this [int i]{
			get {
				return data(i);
			}
		}
		public int Length {
			get {
				return len;
			}
		}

		public override string ToString() {
			blk b = first;
			string s = "";
			int pos = 0;
			int k = 0; 
			int kb = 0;
			while (b != null) {
				if (curblk == b) {kb = k;}
				s += k + ": #" + pos + ",#" + (pos + b.n) + ", " + b.n + ", \"";
				for (int i = 0; i < b.n; i++) {
					char c = b.data[i];
					if (c == '\r') {s += "\\r";}
					else if (c == '\n') {s += "\\n";}
					else {s += b.data[i];}
				}
				s += "\"\r\n";
				pos += b.n;
				b = b.nxt;
				k++;
			}
			s += "\r\nblk: " + kb + ", " + curoff + ", pos: " + curpos + "\r\n";
			return s;
		}
	}
}
