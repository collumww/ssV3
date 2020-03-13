using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssRawTextV2 {
        public ssRawTextV2(string s) {
            newTxt = new StringBuilder();
            sb = new StringBuilder();
            Reset(s);
            }

        string oldTxt;
        StringBuilder newTxt;
        StringBuilder sb;

        class piece {
            public int loc;
            public int len;
            public bool old;
            public piece prv;
            public piece nxt;
            }

        piece head;
        piece tail;

        int len;

        int logI;   // Logical location in the text

        int phyI;   // The equivalent physical location in the structure of logI
        piece phyP;

        int pCnt;
        const int pCntThresh = 1000;



        public bool DoMaint() {
            if (pCnt > pCntThresh) {
                string sa = ToString();
                Reset(sa);
                return true;
                }
            return false;
            }


         void Reset(string s) {
            oldTxt = s;
            newTxt.Clear();
            //GC.Collect();
            head = new piece();
            tail = new piece();
            head.nxt = tail;
            head.prv = null;
            head.len = -1;
            head.old = true;
            tail.nxt = null;
            tail.prv = head;
            tail.len = -2;
            tail.old = true;

            logI = 0;
            phyP = tail;
            phyI = 0;
            len = 0;
            pCnt = 0;

            if (s.Length != 0) {
                phyP = head;
                piece p = new piece();
                p.loc = 0;
                p.len = s.Length;
                p.old = true;
                InsPiece(p);
                phyP = p;
                phyI = 0;
                len = s.Length;
                pCnt = 1;
                }
            }



        public int Length {
            get { return len; }
            }


        public void Show(ssEd ed) {
            ed.MsgLn("Supposedly " + len + " chars, " + pCnt + " pieces");
            int cnt = 0;
            int pcnt = 0;
            for (piece p = head.nxt; p != tail; p = p.nxt) {
                cnt += p.len;
                pcnt++;
                /*ed.MsgLn((p.old ? "- " : "+ ")
                    + p.loc + ", "
                    + p.len
                    //+ (p.old ? oldTxt.Substring(p.loc, p.len) : newTxt.ToString(p.loc, p.len))
                    );*/
                }
            ed.MsgLn("Counted " + cnt + " chars, " + pcnt + " pieces" );
            if (pCnt > pCntThresh) {
                //Reset(ToString());
                }
            }


        public string ToString() {
            StringBuilder sbb = new StringBuilder();
            for (piece p = head.nxt; p != tail; p = p.nxt) {
                if (p.old) sbb.Append(oldTxt.Substring(p.loc, p.len));
                else sbb.Append(newTxt.ToString(p.loc, p.len));
                }
            return sbb.ToString();
            }


        public char this[int i] {
            get {
                if (i < 0 || i >= len) throw new ssException("raw index range");
                Seek(i);
                return phyP.old ? oldTxt[phyP.loc + phyI] : newTxt[phyP.loc + phyI];
                }
            }

        public string ToString(int i, int n) {
            if ((i == 0 && n == len && pCnt > 1) || pCnt > pCntThresh) {
                string sa = ToString();
                Reset(sa);
                }
            sb.Clear();
            Seek(i);
            piece p = phyP;
            int pi = phyI;
            while (n > 0) {
                int loc = p.loc + pi;
                int take = Math.Min(p.len - pi, n);
                sb.Append(p.old ? oldTxt.Substring(loc, take) : newTxt.ToString(loc, take));
                n -= take;
                if (n > 0) {
                    p = p.nxt;
                    if (p == tail)
                        throw new ssException("raw tostring range");
                    pi = 0;
                    }
                }
            return sb.ToString();
            }



        public void Remove(int i, int len) {
            Seek(i + len);
            piece tp = phyP;
            int tpi = phyI;
            Seek(i);
            piece hp = phyP;
            int hpi = phyI;
            if (hp != tp) {
                hp.len -= (hp.len - hpi);
                if (hp.len == 0) hp = hp.prv;
                tp.loc = tp.loc + tpi;
                tp.len -= tpi;
                if (tp.len == 0) tp = tp.nxt;
                hp.nxt = tp;
                tp.prv = hp;
                phyP = tp;
                phyI = 0;
                logI = i;
                }
            else {
                piece np = new piece();
                np.old = hp.old;
                np.loc = hp.loc + hpi + len;
                np.len = hp.len - hpi - len;
                hp.len = hpi;

                InsPiece(np);

                phyP = np;
                phyI = 0;
                logI = i;
                }
            this.len -= len;
            }



        public void Insert(int i, string s) {
            Seek(i);
            piece p;
            if (phyI == 0) { // Beginning of block. Move to end of previous block
                phyP = phyP.prv;
                phyI = phyP.len;
                }
            else {
                if (phyI != phyP.len) { // Middle of block. Split the block
                    p = new piece();
                    p.old = phyP.old;
                    p.loc = phyP.loc + phyI;
                    p.len = phyP.len - phyI;
                    phyP.len = phyI;
                    InsPiece(p);
                    }
                }
            if (!phyP.old && phyP.loc + phyI == newTxt.Length) { // See if we can just append to the newTxt.
                phyP.len += s.Length;
                newTxt.Append(s);
                }
            else {
                p = new piece();  // Inserting at end of block
                p.old = false;
                p.loc = newTxt.Length;
                p.len = s.Length;
                newTxt.Append(s);
                InsPiece(p);
                phyP = p;
                phyI = 0;
                logI = i;
                }
            len += s.Length;
            }




        void InsPiece(piece p) { // Inserts after phyP
            p.nxt = phyP.nxt;
            p.prv = phyP;
            phyP.nxt = p;
            if (p.nxt != null) {
                p.nxt.prv = p;
                }
            pCnt++;
            }



        void Seek(int i) {
            int pb = logI - phyI;  // gets logical location of beginning of piece
            piece p = phyP;
            if (i > logI) {
                while (p != tail && (pb + p.len) <= i) {
                    pb += p.len;
                    p = p.nxt;
                    }
                phyP = p;
                phyI = i - pb;
                logI = i;
                if (p == tail && phyI != 0)
                    throw new ssException("raw text index high");
                }
            else {
                while (pb > i) {
                    p = p.prv;
                    if (p == head) throw new ssException("raw text index low");
                    pb -= p.len;
                    }
                phyP = p;
                phyI = i - pb;
                logI = i;
                }
            }





        }
    }
