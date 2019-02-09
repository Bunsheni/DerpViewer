using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using Xamarin.Forms;

namespace DerpViewer
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string s)
        {

            var upperCase = s.ToUpper();
            var words = upperCase.Split(' ');

            var minorWords = new String[] {"ON", "IN", "AT", "OFF", "WITH", "TO", "AS", "BY",//prepositions
                                   "THE", "A", "OTHER", "ANOTHER",//articles
                                   "AND", "BUT", "ALSO", "ELSE", "FOR", "IF"};//conjunctions

            var acronyms = new String[] {"UK", "USA", "US",//countries
                                   "BBC",//TV stations
                                   "TV"};//others

            //The first word.
            //The first letter of the first word is always capital.
            if (acronyms.Contains(words[0]))
            {
                words[0] = words[0].ToUpper();
            }
            else
            {
                words[0] = words[0].ToPascalCase();
            }

            //The rest words.
            for (int i = 0; i < words.Length; i++)
            {
                if (minorWords.Contains(words[i]))
                {
                    words[i] = words[i].ToLower();
                }
                else if (acronyms.Contains(words[i]))
                {
                    words[i] = words[i].ToUpper();
                }
                else
                {
                    words[i] = words[i].ToPascalCase();
                }
            }

            return string.Join(" ", words);

        }

        public static string ToPascalCase(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
            }
            else
            {
                return String.Empty;
            }
        }
    }
    public static class Library
    {
        public static int CompareInt(string x, string y)
        {

            int xi, yi, res;
            bool xb = Int32.TryParse(x, out xi);
            bool yb = Int32.TryParse(y, out yi);
            if (xb && yb)
                res = xi > yi ? 1 : -1;
            else if (xb)
                res = 1;
            else if (yb)
                res = -1;
            else
                res = string.Compare(x, y);
            return res;
        }

        public static void EncryptFile(string key, string ReadFilename, string WriteFilename)  //파일 암호화
        {
            FileStream fsInput = new FileStream(ReadFilename, FileMode.Open, FileAccess.Read, FileShare.None);
            FileStream fsEncrypted = new FileStream(WriteFilename, FileMode.Create, FileAccess.Write);

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(key);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(key);

            ICryptoTransform Encrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, Encrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            fsEncrypted.Flush();
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        public static void DecryptFile(string key, string ReadFilename, string WriteFilename)//파일 복호화
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Key = ASCIIEncoding.ASCII.GetBytes(key);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(key);

            FileStream fsread = new FileStream(ReadFilename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            ICryptoTransform Decrypt = DES.CreateDecryptor();
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, Decrypt, CryptoStreamMode.Read);

            StreamWriter fsDecrypted = new StreamWriter(WriteFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }

        public static string ByteUnitTransform(double len)
        {
            string length;
            int i;
            for (i = 0; len > 1024; i++)
            {
                len = len / 1024;
            }
            if (i == 0)
            {
                length = len.ToString() + "Byte";
            }
            else if (i == 1)
            {
                length = len.ToString("F0") + "KB";
            }
            else if (i == 2)
            {
                length = len.ToString("F0") + "MB";
            }
            else if (i == 3)
            {
                length = len.ToString("#0.00") + "GB";
            }
            else
            {
                length = "";
            }
            return length;
        }

        public static bool isMark(char ch)
        {
            if ((0x2070 <= ch && ch <= 0x2BFF) || ch == '*' || ch == '+')
                return true;
            else return false;
        }

        public static bool isWord(string str, string word)
        {
            if ((str.IndexOf(word) == 0 || str[str.IndexOf(word) - 1] == ' '))
                if (str.IndexOf(word) + word.Length == str.Length || str[str.IndexOf(word) + word.Length] == ' ')
                    return true;
            return false;
            //return ((str.IndexOf(word) == 0 || str[str.IndexOf(word) - 1] == ' ') && (str.IndexOf(word) + word.Length == str.Length || str[str.IndexOf(word) + word.Length] == ' ')) ? true : false;
        }

        public static bool isUpperCase(string str)
        {
            foreach (char ch in str)
            {
                if ('a' < ch && ch < 'z')
                {
                    return false;
                }
            }
            return true;
        }

        public static bool isNumber(char ch)
        {
            if ('0' <= ch && ch <= '9')
                return true;
            else return false;
        }

        public static bool isNumber(string str)
        {
            if (str.Length != 0 && isNumber(str.First()) && isNumber(str.Last()))
                return true;
            else return false;
        }

        public static bool isMark(string str)
        {
            foreach (char ch in str)
            {
                if (!Library.isMark(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool isJapanese(string str)
        {
            foreach (char ch in str)
            {
                if ((0x3040 < (int)ch && (int)ch < 0x3100) || (0xF900 < (int)ch && (int)ch < 0xFAFF) || (0x4E00 < (int)ch && (int)ch < 0x9FFF))
                    return true;
            }
            return false;
        }
        public static bool isEnglish(string str)
        {
            foreach (char ch in str)
            {
                if ('A' <= ch && ch <= 'z')
                {
                    return true;
                }
            }
            return false;
        }
        public static bool isEnglish(char ch)
        {
            if ('A' <= ch && ch <= 'z')
            {
                return true;
            }
            else
                return false;
        }
        public static bool isKorean(char ch)
        {
            if (0xAC00 <= ch && ch <= 0xD7AF)
            {
                return true;
            }
            else
                return false;
        }
        public static bool isKorean(string str)
        {
            foreach (char ch in str)
            {
                if (0xAC00 <= ch && ch <= 0xD7AF)
                {
                    return true;
                }
            }
            return false;
        }
        //문자열 추출
        public static string extractionString(string original, string front, string back)
        {
            int n = original.IndexOf(front);
            string extract, temp;
            if (n < 0) return "";
            temp = original.Substring(n + front.Length);
            int m = temp.IndexOf(back);
            if (m < 0) return "";
            extract = temp.Substring(0, m);
            temp = temp.Substring(temp.IndexOf(back) + back.Length);
            return extract;
        }

        public static void extractionString(string original, out string rest, string back, out string extract)
        {
            int i = original.IndexOf(back);
            if (i < 0)
            {
                extract = "";
                rest = original;
            }
            else
            {
                extract = original.Substring(0, original.IndexOf(back)).Trim();
                rest = original.Substring(i + back.Length);
            }
        }

        public static void extractionString(string original, out string rest, string front, string back, out string extract)
        {
            int i = original.IndexOf(back);
            int j = original.IndexOf(front) + front.Length;
            if (i < 0)
            {
                extract = "";
                rest = original;
            }
            else
            {
                extract = original.Substring(j, i - j).Trim();
                rest = original.Substring(i + back.Length);
            }
        }

        //문자열 분리
        public static List<string> stringDivider(string strWithSlash, string divider)
        {
            List<string> eachString = new List<string>();
            string temp;
            if (strWithSlash == null || strWithSlash.Length == 0) return eachString;
            if (divider.Length == 0)
            {
                eachString.Add(strWithSlash);
                return eachString;
            }

            while (true)
            {
                if (strWithSlash.Contains(divider))
                {
                    temp = strWithSlash.Substring(0, strWithSlash.IndexOf(divider));
                    eachString.Add(temp.Trim());
                    strWithSlash = strWithSlash.Substring(strWithSlash.IndexOf(divider) + divider.Length);
                    if (strWithSlash.Length == 0)
                    {
                        eachString.Add(string.Empty);
                        break;
                    }
                }
                else
                {
                    eachString.Add(strWithSlash.Trim());
                    break;
                }
            }
            return eachString;
        }

        public static Image getWebImage(string url)
        {
            Image res;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            res = new Image { Source = ImageSource.FromStream(new Func<Stream>(response.GetResponseStream)) };

            //WebClient Downloader = new WebClient();
            //Stream ImageStream = new WebClient().OpenRead(url);
            //res = Image.FromStream(ImageStream);

            return res;
        }

        public static string translateJA(string input)
        {
            string[] ja = { "あ", "い", "う", "え", "お",
                "か", "き", "く", "け", "こ",
                "さ", "し", "す", "せ", "そ",
                "た", "ち", "つ", "て", "と",
                "ま", "み", "む", "め", "も",
                "は", "ひ", "ふ", "へ", "ほ",
                "や", "ゆ", "よ", "を", "ん" };

            string[] ko = { "아", "이", "우", "에", "오",
                "카", "키", "쿠", "케", "코",
                "사", "시", "스", "세", "소",
                "타", "치", "츠", "테", "토",
                "마", "미", "무", "메", "모",
                "하", "히", "후", "헤", "호",
                "야", "유", "요", "오", "응" };
            for (int i = 0; i < 35; i++)
            {
                input = input.Replace(ja[i], ko[i]);
            }
            return input;
        }
    }

    public class WebClientWithTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
            wr.Timeout = 5000; // timeout in milliseconds (ms)
            return wr;
        }
    }
}
