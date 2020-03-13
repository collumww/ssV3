using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    class ssString {
        public static bool AtEOLN(string s, int i, string EOLN) {
            if (i == s.Length || (0 <= i && i <= (s.Length - EOLN.Length) && s.Substring(i, EOLN.Length) == EOLN))
                return true;
            else return false;
            }


        public static bool AtBOLN(string s, int i, string EOLN) {
            if (i == 0 || (i >= EOLN.Length && i <= s.Length && s.Substring(i - EOLN.Length, EOLN.Length) == EOLN))
                return true;
            else return false;
            }


        public static bool Ended(string s, string EOLN) {
            return ssString.AtEOLN(s, s.Length - EOLN.Length, EOLN);
             }
        }
    }
