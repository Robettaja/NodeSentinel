using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace web_server.Managers;

public class PasswordHasher
{
    public static byte[] Hash(string password, byte[] salt)
    {
        using var hasher = new Argon2id(Encoding.UTF8.GetBytes(password));
        hasher.Salt = salt;
        hasher.DegreeOfParallelism = 8;
        hasher.MemorySize = 65536;
        hasher.Iterations = 5;
        return hasher.GetBytes(32);
    }
    public static byte[] Salt(int length)
    {
        byte[] salt = new byte[length];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
    public static bool Verify(string password, byte[] salt, byte[] storedHash)
    {
        using var hasher = new Argon2id(Encoding.UTF8.GetBytes(password));
        hasher.Salt = salt;
        hasher.DegreeOfParallelism = 8;
        hasher.MemorySize = 65536;
        hasher.Iterations = 5;
        byte[] newHash = hasher.GetBytes(32);
        return newHash.SequenceEqual(storedHash);
    }

}
