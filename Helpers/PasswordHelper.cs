using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    public static byte[] HashPassword(string password)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}
