using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ss
{
    public partial class ssEd
    {
        public enum SubType { match, one, zero, alphaUpper, alphaLower }


        public class SubList
        {
            public SubType typ;
            public int loc;
            public string res;
            public SubList nxt;

            public SubList(SubType t, int i)
            {
                typ = t;
                loc = i;
            }
        }


        string PreEscape(string s)
        {
            string res = "";
            bool esc = false;
            foreach (char c in s) {
                if (esc) {
                    switch (c) {
                        case '&':  // Change \& to \\& so Regex.Unescape doesn't remove the \
                            res += "\\\\";
                            res += '&';
                            break;
                        case 'N':  // ss custom line ending escape.
                            if (txt != null)
                                res += txt.Eoln;
                            else
                                res += 'N';
                            break;
                        default:
                            res += '\\'; // Preserve for Regex.Unescape 
                            res += c;
                            break;
                    }
                    esc = false;
                }
                else {
                    switch (c) {
                        case '\\':
                            esc = true;
                            break;
                        default:
                            res += c;
                            break;
                    }
                }
            }
            return res;
            //return SubFound(s, 'N', txt.Eoln, false); // \N will allow commands to work across line endings.
        }


        string Unescape(string s)
        {
            s = PreEscape(s);
            try {
                return Regex.Unescape(s);
            }
            catch {
                return s;
            }
        }

        enum subMode { scan, sub, esc, esc2 }

        SubList PrepForSub(ref string s, bool allowmatch)
        {
            SubList l = new SubList(SubType.match, 0);
            SubList lt = l;
            string res = "";
            int i = 0;
            subMode md = subMode.scan;
            char c = '\0';
            char pc = '\0';
            while (i < s.Length) {
                pc = c;
                c = s[i];
                switch (md) {
                    case subMode.scan:
                        switch (c) {
                            case '\\':
                                md = subMode.esc;
                                break;
                            case '&':
                                md = subMode.sub;
                                break;
                            default:
                                res += c;
                                break;
                        }
                        break;
                    case subMode.esc:
                        md = subMode.scan;
                        break;
                    case subMode.sub:
                        switch (c) {
                            case 'Z':
                                lt.nxt = new SubList(SubType.zero, res.Length);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            case 'O':
                                lt.nxt = new SubList(SubType.one, res.Length);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            case 'U':
                                lt.nxt = new SubList(SubType.alphaUpper, res.Length);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            case 'L':
                                lt.nxt = new SubList(SubType.alphaLower, res.Length);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            default:
                                if (allowmatch) {
                                    switch (c) {
                                        case '\\':
                                            lt.nxt = new SubList(SubType.match, res.Length);
                                            lt = lt.nxt;
                                            md = subMode.scan;
                                            break;
                                        case '&':
                                            lt.nxt = new SubList(SubType.match, res.Length);
                                            lt = lt.nxt;
                                            break;
                                        default:
                                            lt.nxt = new SubList(SubType.match, res.Length);
                                            lt = lt.nxt;
                                            md = subMode.scan;
                                            res += c;
                                            break;
                                    }
                                }
                                else {
                                    switch (c) {
                                        case '\\':
                                            //throw new ssException("substitution not allowed");
                                            md = subMode.esc2;
                                            res += pc;
                                            break;
                                        default:
                                            md = subMode.scan;
                                            res += pc;
                                            res += c;
                                            break;
                                        }
                                }
                                break;
                        }
                        break;
                    case subMode.esc2:
                        md = subMode.scan;
                        res += c;
                        break;
                    }
                i++;
            }
            if (md == subMode.sub) {
                if (allowmatch) lt.nxt = new SubList(SubType.match, res.Length);
                else {
                    if (i > 1) res += pc;
                    else res += '&';
                    }
            }
            if (l.nxt != null) l.nxt.res = res;
            else if (res != s) s = res;

            //MsgLn("/" + res + "/");
            //for (SubList x = l; x != null; x = x.nxt) MsgLn(x.loc.ToString() + " " + x.typ.ToString());
            return l.nxt;
        }


        string DoSubs(string m, SubList l)
        {
            string res = l.res;
            int adj = 0;
            while (l != null) {
                switch (l.typ) {
                    case SubType.zero:
                        string s = zero.ToString();
                        res = res.Insert(l.loc + adj, s);
                        adj += s.Length;
                        zero++;
                        break;
                    case SubType.one:
                        s = one.ToString();
                        res = res.Insert(l.loc + adj, s);
                        adj += s.Length;
                        one++;
                        break;
                    case SubType.alphaUpper:
                        s = ToAlphaString(alphaUpper, 'A');
                        res = res.Insert(l.loc + adj, s);
                        adj += s.Length;
                        alphaUpper++;
                        break;
                    case SubType.alphaLower:
                        s = ToAlphaString(alphaLower, 'a');
                        res = res.Insert(l.loc + adj, s);
                        adj += s.Length;
                        alphaLower++;
                        break;
                    case SubType.match:
                        res = res.Insert(l.loc + adj, m);
                        adj += m.Length;
                        break;
                }
                l = l.nxt;
            }
            return res;
        }

    }
}