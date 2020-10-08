using DigWex.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
 
namespace DigWex.Helpers
{
  public static class CryptoHelper
  {
    public static string Md5(Stream input)
    {
      byte[] hash;
      using (MD5 md5 = MD5.Create()) {
        hash = md5.ComputeHash(input);
      }
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hash.Length; i++) {
        sb.Append(hash[i].ToString("x2"));
      }
      Console.WriteLine(sb);
      return sb.ToString();
    }

    public static string Md5File(string file)
    {
      byte[] hash;
      using (var md5 = MD5.Create()) {
        using (var stream = File.OpenRead(file)) {
          hash = md5.ComputeHash(stream);
        }
      }

      StringBuilder sb = new StringBuilder();
      foreach (byte item in hash) {
        sb.Append(item.ToString("x2"));
      }
      return sb.ToString();
    }

    public static string Md5(string input)
    {
      byte[] bytes = input.ToByte();
      return Md5(bytes);
    }

  
    public static string Md5(byte[] input)
    {
      using (MD5 md5 = MD5.Create()) {
        byte[] hash = md5.ComputeHash(input);
        StringBuilder sb = new StringBuilder();
        foreach (byte item in hash) {
          sb.Append(item.ToString("x2"));
        }
        return sb.ToString();
      }

    }

    public static bool VerifyMd5Hash(string input, string hash)
    {
      //Console.WriteLine(input);
      string hashOfInput = Md5(input);

      //Console.WriteLine(new { hash, hashOfInput });

      //Console.WriteLine("hash data: {0}", hashOfInput);
      //Console.WriteLine(hashOfInput);
      StringComparer comparer = StringComparer.OrdinalIgnoreCase;
      return comparer.Compare(hashOfInput, hash) == 0;
    }

    public static bool VerifyMd5HashDataSpecial(string input, string hash)
    {
      //Console.WriteLine(input);
      byte[] dataInput = Convert.FromBase64String(input);
      byte[] dataHash = Convert.FromBase64String(hash);

      for (int i = 10; i < 32; i++) {
        if (dataInput[i] != dataHash[i])
          return false;
      }

      return true;
    }

    public static bool VerifyMd5HashSpecial(string input, string hash)
    {
      //Console.WriteLine(input);
      string hashOfInput = Md5(input);

      //Console.WriteLine(new { hash, hashOfInput });

      //Console.WriteLine("hash data: {0}", hashOfInput);
      //Console.WriteLine(hashOfInput);
      StringComparer comparer = StringComparer.OrdinalIgnoreCase;
      return comparer.Compare(hashOfInput, hash) == 0;
    }

    public static byte[] SignData(byte[] data)
    {
      var csp = new CspParameters {
        Flags = CspProviderFlags.UseDefaultKeyContainer
      };

      byte[] sig;

      try {
        var rsa = new RSACryptoServiceProvider(csp);
        sig = rsa.SignData(data, "SHA1");
      }
      catch {
        sig = new byte[] { 0, 1, 2 };
      }

      return sig;
    }
  }
}
