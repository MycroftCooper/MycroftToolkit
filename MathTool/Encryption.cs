using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MycroftToolkit.MathTool {
    public static class Encryption {
        /// <summary>
        /// MD5加密
        /// </summary>
        public static string MD5Encrypt(string str) {
            if (string.IsNullOrEmpty(str)) return "";
            string cl = str;
            string md5Str = "";
            MD5 myMd5 = MD5.Create();
            byte[] bytes = myMd5.ComputeHash(Encoding.UTF8.GetBytes(cl));
            foreach (var t in bytes) {
                md5Str += t.ToString("x2");
            }

            return md5Str;
        }

        public static string GetMD5FromFile(string fullPath) {
            try {
                FileStream fs = new FileStream(fullPath, FileMode.Open);
                MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
                byte[] data = md5Hasher.ComputeHash(fs);
                fs.Close();
                StringBuilder sBuilder = new StringBuilder();
                foreach (var t in data) {
                    sBuilder.Append(t.ToString("x2"));
                }

                return sBuilder.ToString();
            }
            catch (System.Exception ex) {
                Debug.LogError("GetMD5FromFile error, exp: " + ex);
            }

            return string.Empty;
        }
    }
}