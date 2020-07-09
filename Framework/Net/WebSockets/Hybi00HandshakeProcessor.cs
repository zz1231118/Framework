using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Framework.Net.WebSockets
{
    class Hybi00HandshakeProcessor : HandshakeProcessor
    {
        private const byte spaceChar = 32;
        private const string m_SecurityKeyRegex = "[^0-9]";
        private static readonly List<char> charList = new List<char>();
        private static readonly List<char> digList = new List<char>();

        static Hybi00HandshakeProcessor()
        {
            //ascii encode
            for (int i = 33; i <= 126; i++)
            {
                char c = (char)i;
                if (char.IsLetter(c))
                {
                    charList.Add(c);
                }
                else if (char.IsDigit(c))
                {
                    digList.Add(c);
                }
            }
        }

        public Hybi00HandshakeProcessor(WebSocketListener webSocketListener, Encoding encoding)
            : base(webSocketListener, encoding)
        { }

        protected override bool CheckSignKey(HandshakeData handshakeData)
        {
            if (handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecAccept, out byte[] secAccept) &&
                handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecSignKey, out byte[] signKey))
            {
                return MathUtils.IndexOf(secAccept, signKey) != -1;
            }
            return false;
        }

        protected override bool ResponseHandshake(WebSocket socket, HandshakeData handshakeData)
        {
            if (handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecKey1, out string secKey1) &&
                handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecKey2, out string secKey2) &&
                handshakeData.ParamItems.TryGet(HandshakeHeadKeys.SecKey3, out byte[] secKey3))
            {
                //The minimum version support 
                StringBuilder response = new StringBuilder();
                response.AppendLine(HandshakeHeadKeys.RespHead_00);
                response.AppendLine(HandshakeHeadKeys.RespUpgrade00);
                response.AppendLine(HandshakeHeadKeys.RespConnection);
                if (handshakeData.ParamItems.TryGet(HandshakeHeadKeys.Origin, out string origin))
                {
                    response.AppendLine(string.Format(HandshakeHeadKeys.RespOriginLine, origin));
                }
                response.AppendLine(string.Format(HandshakeHeadKeys.SecLocation, handshakeData.UriSchema, handshakeData.Host, handshakeData.UrlPath));
                if (!string.IsNullOrEmpty(handshakeData.Protocol))
                {
                    response.AppendLine(string.Format(HandshakeHeadKeys.RespProtocol, handshakeData.Protocol));
                }
                response.AppendLine();
                socket.PostSend(response.ToString());
                //Encrypt message
                byte[] securityKey = GetResponseSecurityKey(secKey1, secKey2, secKey3);
                socket.PostSend(securityKey);
                return true;
            }

            return false;
        }

        private byte[] GetResponseSecurityKey(string secKey1, string secKey2, byte[] secKey3)
        {
            //Remove all symbols that are not numbers
            string k1 = Regex.Replace(secKey1, m_SecurityKeyRegex, String.Empty);
            string k2 = Regex.Replace(secKey2, m_SecurityKeyRegex, String.Empty);

            //Convert received string to 64 bit integer.
            long intK1 = long.Parse(k1);
            long intK2 = long.Parse(k2);

            //Dividing on number of spaces
            int k1Spaces = secKey1.Count(c => c == ' ');
            int k2Spaces = secKey2.Count(c => c == ' ');
            int k1FinalNum = (int)(intK1 / k1Spaces);
            int k2FinalNum = (int)(intK2 / k2Spaces);

            //Getting byte parts
            byte[] b1 = BitConverter.GetBytes(k1FinalNum).Reverse().ToArray();
            byte[] b2 = BitConverter.GetBytes(k2FinalNum).Reverse().ToArray();
            //byte[] b3 = Encoding.UTF8.GetBytes(secKey3);
            byte[] b3 = secKey3;

            //Concatenating everything into 1 byte array for hashing.
            byte[] bChallenge = new byte[b1.Length + b2.Length + b3.Length];
            Array.Copy(b1, 0, bChallenge, 0, b1.Length);
            Array.Copy(b2, 0, bChallenge, b1.Length, b2.Length);
            Array.Copy(b3, 0, bChallenge, b1.Length + b2.Length, b3.Length);

            //Hash and return
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(bChallenge);
            }
        }

        private byte[] GenerateSecKey()
        {
            return GenerateSecKey(16);
        }

        private byte[] GenerateSecKey(int count)
        {
            var random = new Random();
            int spaceNum = random.Next(1, count / 2 + 1);
            int charNum = random.Next(3, count - spaceNum);
            int digNum = count - spaceNum - charNum;
            byte[] array = new byte[count];
            int pos = 0;
            for (int i = 0; i < spaceNum; i++)
            {
                array[pos++] = spaceChar;
            }
            for (int j = 0; j < charNum; j++)
            {
                array[pos++] = (byte)charList[random.Next(0, charList.Count)];
            }
            for (int k = 0; k < digNum; k++)
            {
                array[pos++] = (byte)digList[random.Next(0, digList.Count)];
            }

            int num = array.Length / 2;
            for (int i = 0; i < num; i++)
            {
                int num2 = random.Next(0, array.Length);
                int num3 = random.Next(0, array.Length);
                if (num2 != num3)
                {
                    byte t = array[num3];
                    array[num3] = array[num2];
                    array[num2] = t;
                }
            }
            return array;
        }
    }
}
