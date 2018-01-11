using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;

namespace AgsysServer
{

  //BEGIN Common
  class Common
  {
    private Stopwatch sw = null;

    private long _TickBegin = 0;
    private long _TickEnd = 0;
    private long _TickDiff = 0;

    public Common()
    {
      //sw = new Stopwatch();
    }

    public long TickBegin()
    {
      _TickBegin = DateTime.Now.Ticks;
      return _TickBegin; 
    }
    public long TickEnd()
    {
      _TickEnd = DateTime.Now.Ticks;
      _TickDiff = _TickEnd - _TickBegin;
      return _TickEnd; 
    }
    public long TickDiff
    {
      get { return _TickDiff; }
    }
    public double TickDiffSec
    {
      get { return (((double)TickDiff) / 10000000); }
    }


    public void TimeBegin()
    {
      sw = new Stopwatch();

      sw.Reset();
      sw.Start();
    }
    public void TimeEnd()
    {
      sw.Stop();
    }
    public string TimeSec
    {
      get { return (((double) sw.ElapsedMilliseconds) / 1000).ToString(); }
    }


    //BEGIN Left
    public string Left(string s = "", int l = 0, int t = 0)
    {
      if (s == null) { s = ""; }
      if (l < 0) { l = 0; }

      if (t == 0)
      {
        string r = "";        
        for (int i = 0; i <= (s.Length - 1); i++)
        {
          if (i <= (l - 1)) { r += s[i]; }
          else { break; }
        }
        return r;
      }
      else
      {
        var sb = new StringBuilder();
        for (int i = 0; i <= (s.Length - 1); i++)
        {
          if (i <= (l - 1)) { sb.Append(s[i]); }
          else { break; }      
        }
        return sb.ToString();
      }
    }
    //END Left

    //BEGIN LeftAndTrim
    public string Leftt(string s = "", int l = 0, int t = 0)
    {
      return Left(s, l, t).Trim();
    }
    //END LeftAndTrim



    //BEGIN IsStrNum
    public bool IsStrNum(string s, string t)
    {
      bool r = false;

      if (s == null) { s = ""; }

      string d = CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator;
      if (d == ".") { s = s.Replace(",", "."); }
      else if (d == ",") { s = s.Replace(".", ","); }

      if (t == "i")
      {
        int n;
        if (int.TryParse(s, out n)) { r = true; }
      }
      else if (t == "d")
      {
        double n;
        if (double.TryParse(s, out n)) { r = true; }
      }

      return r;
    }
    //END IsStrNum

    //BEGIN IsStrDt
    public bool IsStrDt(string s)
    {
      bool r = false;
      DateTime dt;
      if (DateTime.TryParse(s, out dt)) { r = true; }
      return r;
    }
    //END IsStrDt


    //BEGIN FileToHexString
    public string FileToHexString(string filePath = "")
    {
      StringBuilder sb = new StringBuilder();

      try
      {
        if (File.Exists(filePath))
        {
          using (FileStream fs = new FileStream(filePath, FileMode.Open))
          {
            int fsLen = (int)fs.Length; byte[] b = new byte[fsLen]; fs.Read(b, 0, fsLen);
            for (int i = 0; i < fsLen; i++) { sb.Append(b[i].ToString("X2")); }
          }
        }
      }
      catch { }

      return sb.ToString();
    }
    //END FileToHexString

    //BEGIN HexStringToFile
    public bool HexStringToFile(string filePath = "", string hexString = "")
    {
      bool r = false;

      try
      {
        using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
        {
          int hsLen = hexString.Length;

          for (int i = 0; i < (hsLen / 2); i++)
          {
            if ((i * 2 + 1) < hsLen)
            {
              string h = hexString[i * 2].ToString() + hexString[i * 2 + 1].ToString();                       
              int b = int.Parse(h, NumberStyles.HexNumber); //int b = Convert.ToInt32(h, 16);
              fs.WriteByte(Convert.ToByte(b)); 
            }
          }

          r = true;
        }
      }
      catch { }

      return r;
    }
    //END HexStringToFile


    public string CodeGen(int l)
    {
      string code = "";
      string[] arr = { "1","2","3","4","5","6","7","8","9","0",
                       "a","b","c","d","e","f","g","h","i","j","k","l","m","n",
                       "o","p","q","r","s","t","u","v","w","x","y","z" };

      Random rnd = new Random(); //(int) System.DateTime.Now.Ticks & 0x0000FFFF     
      for (int i = 0; i < l; i++) { code += arr[rnd.Next(0, 36)]; }

      return code;
    }

    public string CodeGenCrypt(int l)
    {
      string code = "";
      string[] arr = { "1","2","3","4","5","6","7","8","9","0",
                       "a","b","c","d","e","f","g","h","i","j","k","l","m","n",
                       "o","p","q","r","s","t","u","v","w","x","y","z" };     

      using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
	    {	 
        byte[] data = new byte[4];     
        rng.GetBytes(data);
        Random rnd = new Random(BitConverter.ToInt32(data, 0));
        for (int i = 0; i < l; i++) { code += arr[rnd.Next(0, 36)]; }
	    }

      return code;
    }



    //BEGIN Aes Crypt
    public byte[] AesEncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
    {
      byte[] encrypted;

      using (AesManaged aesAlg = new AesManaged())
      {     
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msEncrypt = new MemoryStream())
        {
          using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
          {
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) { swEncrypt.Write(plainText); }
            encrypted = msEncrypt.ToArray();
          }
        }
      }

      return encrypted;
    }

    public string AesDecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
    {
      string plaintext = null;

      using (AesManaged aesAlg = new AesManaged())
      {
        aesAlg.Key = Key;
        aesAlg.IV = IV;

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        {
          using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
          {
            using (StreamReader srDecrypt = new StreamReader(csDecrypt)) { plaintext = srDecrypt.ReadToEnd(); }
          }
        }

      }

      return plaintext;
    }

    /*
        string original = "Here is some data to encrypt!";

        byte[] Key = Encoding.ASCII.GetBytes("12345678901234561234567890123456");
        byte[] IV = Encoding.ASCII.GetBytes("1234567890123456");

        byte[] encrypted = AesEncryptStringToBytes(original, Key, IV);
        string roundtrip = AesDecryptStringFromBytes(encrypted, Key, IV);

        Console.WriteLine("Original: {0}", original);
        Console.WriteLine("Encrypted: {0}", Encoding.Default.GetString(encrypted));
        Console.WriteLine("Round Trip: {0}", roundtrip);     
     */
    //END Aes Crypt



  }
  //END Common
}
