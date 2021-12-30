using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ss {
    public partial class ssEd {
        public enum SubType { none, match, one, zero, alphaUpper, alphaLower }


        public class SubList {
            public SubType typ;
            public SubList nxt;

            public SubList(SubType t) {
                typ = t;
                }
            }


        string PreEscape(string s) {
            string res = "";
            bool esc = false;
            foreach (char c in s) {
                if (esc) {
                    switch (c) {
                        case 'N':  // ss custom line ending escape.
                            if (txt != null)
                                res += txt.Eoln;
                            else
                                res += "\\N";
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


        string Unescape(string s) {
            return Regex.Unescape(PreEscape(s));
            }

        enum subMode { scan, sub, esc, esc2 }

        SubList PrepForSub(ref string s, bool allowmatch) {
            SubList l = new SubList(SubType.none);
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
                            }
                        res += c;
                        break;
                    case subMode.esc:
                        switch (c) {
                            case '&':
                                lt.nxt = new SubList(SubType.none);
                                lt = lt.nxt;
                                break;
                            }
                        res += c;
                        md = subMode.scan;
                        break;
                    case subMode.sub:
                        switch (c) {
                            case 'Z':
                                lt.nxt = new SubList(SubType.zero);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            case 'O':
                                lt.nxt = new SubList(SubType.one);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            case 'U':
                                lt.nxt = new SubList(SubType.alphaUpper);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            case 'L':
                                lt.nxt = new SubList(SubType.alphaLower);
                                lt = lt.nxt;
                                md = subMode.scan;
                                break;
                            default:
                                if (allowmatch) {
                                    switch (c) {
                                        case '\\':
                                            lt.nxt = new SubList(SubType.match);
                                            lt = lt.nxt;
                                            md = subMode.scan;
                                            break;
                                        case '&':
                                            lt.nxt = new SubList(SubType.match);
                                            lt = lt.nxt;
                                            res += c;
                                            break;
                                        default:
                                            lt.nxt = new SubList(SubType.match);
                                            lt = lt.nxt;
                                            md = subMode.scan;
                                            res += c;
                                            break;
                                        }
                                    }
                                else {
                                    md = subMode.scan;
                                    res += c;
                                    }
                                break;
                            }
                        break;
                    }
                i++;
                }
            if (md == subMode.sub) {
                if (allowmatch) lt.nxt = new SubList(SubType.match);
                }
            s = res;

            //MsgLn("/" + res + "/");
            //for (SubList x = l; x != null; x = x.nxt) MsgLn(x.loc.ToString() + " " + x.typ.ToString());
            return l.nxt;
            }


        string DoSubs(string m, SubList sl, string rep) {
            string res = "";
            foreach (char c in rep) {
                if (c == '&' && sl != null) {
                    switch (sl.typ) {
                        case SubType.none:
                            res += c;
                            break;
                        case SubType.match:
                            res += m;
                            break;
                        case SubType.alphaLower:
                            res += ToAlphaString(alphaLower, 'a');
                            alphaLower++;
                            break;
                        case SubType.alphaUpper:
                            res += ToAlphaString(alphaUpper, 'A');
                            alphaUpper++;
                            break;
                        case SubType.one:
                            res += one.ToString();
                            one++;
                            break;
                        case SubType.zero:
                            res += zero.ToString();
                            zero++;
                            break;
                        }
                    sl = sl.nxt;
                    }
                else res += c;
                }
            return res;
            }
        }
    }