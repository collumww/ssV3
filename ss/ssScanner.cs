using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssScanner {
        public ssScanner(string s, bool comments) {
            txt = s;
            sb = new StringBuilder();
            SmallInit(comments);
            }


        public void Init(string s, bool comments) {
            txt = s;
            sb.Clear();
            SmallInit(comments);
            }


        void SmallInit(bool com) {
            i = 0;
            itemLen = 0;
            delims = "";
            cancomment = com;
            }

        public bool EOT() {
            return c == '\0';
            }

        char getch() {
            if (i < txt.Length) return txt[i++];
            return '\0';
            }


        public char GetChar() {
            char x;
            lastc = c;
            if (cancomment) {
                if ((x = getch()) == '#') {
                    do {
                        x = getch();
                        }
                    while (x != '\0' && x != '\n');
                    }
                }
            else x = getch();
            if (x == '\r') x = getch();   // Assuming Windows line endings and throwing out the cr's.
            c = x;
            return c;
            }


        public int GetNum() {
            int m = itemLen = 0;
            while (char.IsDigit(c)) {
                m = m * 10 + ((int)c - (int)'0');
                itemLen++;
                GetChar();
                }
            n = m;
            return n;
            }


        public string GetStrSpDelim() {
            itemLen = 0;
            sb.Clear();
            while (!EOT() && char.IsWhiteSpace(c)) {
                GetChar();
                }
            while (!EOT() && !char.IsWhiteSpace(c)) {
                sb.Append(c);
                itemLen++;
                GetChar();
                }
            while (!EOT() && char.IsWhiteSpace(c)) {
                GetChar();
                }
            s = sb.ToString();
            return s;
            }


        public void SetDelim(string dlm) {
            delims = dlm;
            }

        public void SetDelim(char c) {
            delims = c.ToString();
            }


        bool IsDelim() {
            foreach (char x in delims) {
                if (c == x && lastc != '\\') return true;
                }
            return false;
            }


        public string GetStr() {
            itemLen = 0;
            sb.Clear();
            while (!IsDelim() && !EOT()) {
                sb.Append(c);
                itemLen++;
                GetChar();
                }
            if (IsDelim()) GetChar();
            s = sb.ToString();
            return s;
            }


        public char C {
            get { return c; }
            }
    

        public string S {
            get { return s; }
            }


        public int N {
            get { return n; }
            }

        public int Len {
            get { return itemLen; }
            }

        public bool Nothing {
            get { return itemLen == 0; }
            }

        public bool AllowComment {
            get { return cancomment; }
            set { cancomment = value; }
            }


        //---- Private data -----------------------

        string txt;
        int i;
        StringBuilder sb;
        string delims;
        bool cancomment;

        char c;
        char lastc;
        string s;
        int n;
        int itemLen;
        }
    }
