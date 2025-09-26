using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChiefoUtilities
{
    namespace Encryption
    {
        public enum KeySets
        {
            q = 10,
            w,
            e,
            r,
            t,
            y,
            u,
            i,
            o,
            p,
            a,
            s,
            d,
            f,
            g,
            h,
            j,
            k,
            l,
            z,
            x,
            c,
            v,
            b,
            n,
            m,
            Q,
            W,
            E,
            R,
            T,
            Y,
            U,
            I,
            O,
            P,
            A,
            S,
            D,
            F,
            G,
            H,
            J,
            K,
            L,
            Z,
            X,
            C,
            V,
            B,
            N,
            M,
            comma,
            dot,
            space,
            rightslash,
            quotationmarks,
            bracketleft,
            bracketright,
            doubledot,
            number0,
            number1,
            number2,
            number3,
            number4,
            number5,
            number6,
            number7,
            number8,
            number9,
        };


        public class ChiefoEncryptor
        {
            private static Dictionary<char, int> EncryptionChars;
            private static Dictionary<int, char> DecryptionIDsChars;
            public static string Encrypt(string _context)
            {
                CreateConverters();

                string encryptedString = string.Empty;
                foreach (char Character in _context)
                {
                    if (EncryptionChars.ContainsKey(Character))
                    {
                        encryptedString = encryptedString + EncryptionChars[Character] + ",";
                    }
                }
                string returnstring = string.Empty;
                for (int i = 0; i < encryptedString.Length - 1; i++)
                {
                    returnstring = returnstring + encryptedString[i];
                }
                return returnstring;
            }

            public static string DecryptString(string _context)
            {
                CreateConverters();

                string decryptedtext = string.Empty;
                string[] Sets = _context.Split(',');

                foreach (string set in Sets)
                {
                    int _index = System.Convert.ToInt32(set);
                    if (DecryptionIDsChars.ContainsKey(_index))
                    {
                        decryptedtext = decryptedtext + DecryptionIDsChars[_index];
                    }
                }
                return decryptedtext;
            }

            private static void CreateConverters()
            {
                if (EncryptionChars == null && DecryptionIDsChars == null)
                {
                    Create();
                }
            }

            private static void Create()
            {
                EncryptionChars = new Dictionary<char, int>
                {
                    {'q',(int)KeySets.q },
                    {'w',(int)KeySets.w },
                    {'e',(int)KeySets.e },
                    {'r',(int)KeySets.r },
                    {'t',(int)KeySets.t },
                    {'y',(int)KeySets.y },
                    {'u',(int)KeySets.u },
                    {'i',(int)KeySets.i },
                    {'o',(int)KeySets.o },
                    {'p',(int)KeySets.p },
                    {'a',(int)KeySets.a },
                    {'s',(int)KeySets.s },
                    {'d',(int)KeySets.d },
                    {'f',(int)KeySets.f },
                    {'g',(int)KeySets.g },
                    {'h',(int)KeySets.h },
                    {'j',(int)KeySets.j },
                    {'k',(int)KeySets.k },
                    {'l',(int)KeySets.l },
                    {'z',(int)KeySets.z },
                    {'x',(int)KeySets.x },
                    {'c',(int)KeySets.c },
                    {'v',(int)KeySets.v },
                    {'b',(int)KeySets.b },
                    {'n',(int)KeySets.n },
                    {'m',(int)KeySets.m },
                    {'Q',(int)KeySets.Q },
                    {'W',(int)KeySets.W },
                    {'E',(int)KeySets.E },
                    {'R',(int)KeySets.R },
                    {'T',(int)KeySets.T },
                    {'Y',(int)KeySets.Y },
                    {'U',(int)KeySets.U },
                    {'I',(int)KeySets.I },
                    {'O',(int)KeySets.O },
                    {'P',(int)KeySets.P },
                    {'A',(int)KeySets.A },
                    {'S',(int)KeySets.S },
                    {'D',(int)KeySets.D },
                    {'F',(int)KeySets.F },
                    {'G',(int)KeySets.G },
                    {'H',(int)KeySets.H },
                    {'J',(int)KeySets.J },
                    {'K',(int)KeySets.K },
                    {'L',(int)KeySets.L },
                    {'Z',(int)KeySets.Z },
                    {'X',(int)KeySets.X },
                    {'C',(int)KeySets.C },
                    {'V',(int)KeySets.V },
                    {'B',(int)KeySets.B },
                    {'N',(int)KeySets.N },
                    {'M',(int)KeySets.M },
                    {',',(int)KeySets.comma },
                    {'.',(int)KeySets.dot },
                    {' ',(int)KeySets.space },
                    {'{',(int)KeySets.bracketleft },
                    {'}',(int)KeySets.bracketright },
                    {'"',(int)KeySets.quotationmarks },
                    {':',(int)KeySets.doubledot },
                    {'\\',(int)KeySets.rightslash },
                    {'0',(int)KeySets.number0 },
                    {'1',(int)KeySets.number1 },
                    {'2',(int)KeySets.number2 },
                    {'3',(int)KeySets.number3 },
                    {'4',(int)KeySets.number4 },
                    {'5',(int)KeySets.number5 },
                    {'6',(int)KeySets.number6 },
                    {'7',(int)KeySets.number7 },
                    {'8',(int)KeySets.number8 },
                    {'9',(int)KeySets.number9 },
                };  
                DecryptionIDsChars = new Dictionary<int, char>()
                {
                    {(int)KeySets.q,'q' },
                    {(int)KeySets.w,'w' },
                    {(int)KeySets.e,'e' },
                    {(int)KeySets.r,'r' },
                    {(int)KeySets.t,'t' },
                    {(int)KeySets.y,'y' },
                    {(int)KeySets.u,'u' },
                    {(int)KeySets.i,'i' },
                    {(int)KeySets.o,'o' },
                    {(int)KeySets.p,'p' },
                    {(int)KeySets.a,'a' },
                    {(int)KeySets.s,'s' },
                    {(int)KeySets.d,'d' },
                    {(int)KeySets.f,'f' },
                    {(int)KeySets.g,'g' },
                    {(int)KeySets.h,'h' },
                    {(int)KeySets.j,'j' },
                    {(int)KeySets.k,'k' },
                    {(int)KeySets.l,'l' },
                    {(int)KeySets.z,'z' },
                    {(int)KeySets.x,'x' },
                    {(int)KeySets.c,'c' },
                    {(int)KeySets.v,'v' },
                    {(int)KeySets.b,'b' },
                    {(int)KeySets.n,'n' },
                    {(int)KeySets.m,'m' },
                    {(int)KeySets.Q,'Q' },
                    {(int)KeySets.W,'W' },
                    {(int)KeySets.E,'E' },
                    {(int)KeySets.R,'R' },
                    {(int)KeySets.T,'T' },
                    {(int)KeySets.Y,'Y' },
                    {(int)KeySets.U,'U' },
                    {(int)KeySets.I,'I' },
                    {(int)KeySets.O,'O' },
                    {(int)KeySets.P,'P' },
                    {(int)KeySets.A,'A' },
                    {(int)KeySets.S,'S' },
                    {(int)KeySets.D,'D' },
                    {(int)KeySets.F,'F' },
                    {(int)KeySets.G,'G' },
                    {(int)KeySets.H,'H' },
                    {(int)KeySets.J,'J' },
                    {(int)KeySets.K,'K' },
                    {(int)KeySets.L,'L' },
                    {(int)KeySets.Z,'Z' },
                    {(int)KeySets.X,'X' },
                    {(int)KeySets.C,'C' },
                    {(int)KeySets.V,'V' },
                    {(int)KeySets.B,'B' },
                    {(int)KeySets.N,'N' },
                    {(int)KeySets.M,'M' },
                    {(int)KeySets.comma,',' },
                    {(int)KeySets.dot,'.' },
                    {(int)KeySets.space,' ' },
                    {(int)KeySets.bracketleft,'{' },
                    {(int)KeySets.bracketright,'}' },
                    {(int)KeySets.quotationmarks,'"' },
                    {(int)KeySets.doubledot,':'},
                    {(int)KeySets.rightslash,'\\' },
                    {(int)KeySets.number0,'0' },
                    {(int)KeySets.number1,'1' },
                    {(int)KeySets.number2,'2' },
                    {(int)KeySets.number3,'3' },
                    {(int)KeySets.number4,'4' },
                    {(int)KeySets.number5,'5' },
                    {(int)KeySets.number6,'6' },
                    {(int)KeySets.number7,'7' },
                    {(int)KeySets.number8,'8' },
                    {(int)KeySets.number9,'9' },
                };
            }
        }
    }
}
