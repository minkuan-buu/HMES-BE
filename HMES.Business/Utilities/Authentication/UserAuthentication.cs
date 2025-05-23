using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HMES.Data.DTO.ResponseModel;
using HMES.Data.Entities;
using Microsoft.IdentityModel.Tokens;
using RNGCryptoServiceProvider = System.Security.Cryptography.RNGCryptoServiceProvider;

namespace HMES.Business.Utilities.Authentication;

public class Authentication
{
    private static string Key = "TestingIssuerSigningKeyPTEducationMS@123";
    private static string Issuser = "TestingJWTIssuerSigningPTEducationMS@123";

    public Authentication()
    {
    }

    static string GenerateSalt()
    {
        int SaltLength = 16;

        byte[] Salt = new byte[SaltLength];

        RandomNumberGenerator.Fill(Salt);

        return BitConverter.ToString(Salt).Replace("-", "");
    }

    public static CreateHashPasswordModel CreateHashPassword(string Password)
    {
        string SaltString = GenerateSalt();
        byte[] Salt = Encoding.UTF8.GetBytes(SaltString);
        byte[] PasswordByte = Encoding.UTF8.GetBytes(Password);
        byte[] CombinedBytes = CombineBytes(PasswordByte, Salt);
        byte[] HashedPassword = HashingPassword(CombinedBytes);
        return new CreateHashPasswordModel()
        {
            Salt = Encoding.UTF8.GetBytes(SaltString),
            HashedPassword = HashedPassword
        };
    }

    public static bool VerifyPasswordHashed(string Password, byte[] Salt, byte[] PasswordStored)
    {
        byte[] PasswordByte = Encoding.UTF8.GetBytes(Password);
        byte[] CombinedBytes = CombineBytes(PasswordByte, Salt);
        byte[] NewHash = HashingPassword(CombinedBytes);
        return PasswordStored.SequenceEqual(NewHash);
    }

    static byte[] HashingPassword(byte[] PasswordCombined)
    {
        using (SHA256 SHA256 = SHA256.Create())
        {
            byte[] HashBytes = SHA256.ComputeHash(PasswordCombined);
            return HashBytes;
        }
    }

    static byte[] CombineBytes(byte[] First, byte[] Second)
    {
        byte[] Combined = new byte[First.Length + Second.Length];
        Buffer.BlockCopy(First, 0, Combined, 0, First.Length);
        Buffer.BlockCopy(Second, 0, Combined, First.Length, Second.Length);
        return Combined;
    }

    public static string GenerateJWT(User User)
    {
        var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var Credential = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        List<Claim> Claims = new()
        {
            new Claim(ClaimsIdentity.DefaultRoleClaimType, User.Role),
            new Claim("userid", User.Id.ToString()),
            new Claim("email", User.Email),
        };

        var Token = new JwtSecurityToken(
            issuer: Issuser,
            audience: Issuser,
            claims: Claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: Credential
            );
        var Encodetoken = new JwtSecurityTokenHandler().WriteToken(Token);
        return Encodetoken;
    }

    public static string GenerateTempJWT(string Email)
    {
        var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
        var Credential = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
        List<Claim> Claims = new()
        {
            new Claim("type", "reset"),
            new Claim("email", Email),
        };

        var Token = new JwtSecurityToken(
            issuer: Issuser,
            audience: Issuser,
            claims: Claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: Credential
            );
        var Encodetoken = new JwtSecurityTokenHandler().WriteToken(Token);
        return Encodetoken;
    }

    public static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public static string GenerateRandomPassword()
    {
        int length = 12;
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        byte[] data = new byte[length];
        byte[] buffer = new byte[sizeof(int)];
        StringBuilder result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            RandomNumberGenerator.Fill(buffer);
            int randomNumber = BitConverter.ToInt32(buffer, 0);
            randomNumber = Math.Abs(randomNumber);
            int index = randomNumber % chars.Length;
            result.Append(chars[index]);
        }
        return result.ToString();
    }

    public static string DecodeToken(string jwtToken, string nameClaim)
    {
        var _tokenHandler = new JwtSecurityTokenHandler();
        Claim? claim = _tokenHandler.ReadJwtToken(jwtToken).Claims.FirstOrDefault(selector => selector.Type.ToString().Equals(nameClaim));
        return claim != null ? claim.Value : "Error!!!";
    }

    public static string GenerateRandomSerial(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder(length);
        using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
        {
            byte[] data = new byte[length];
            crypto.GetBytes(data);
            foreach (byte b in data)
            {
                result.Append(chars[b % chars.Length]);
            }
        }
        return result.ToString();
    }

    public static string CreateIoTToken(Guid deviceId, string serialNumber, string userId)
    {
        string deviceIdString = deviceId.ToString();
        byte[] deviceIdBytes = Encoding.UTF8.GetBytes(deviceIdString);
        byte[] serialNumberBytes = Encoding.UTF8.GetBytes(serialNumber);
        byte[] userIdBytes = Encoding.UTF8.GetBytes(userId);
        byte[] combinedBytes = new byte[deviceIdBytes.Length + serialNumberBytes.Length + userIdBytes.Length];
        Buffer.BlockCopy(deviceIdBytes, 0, combinedBytes, 0, deviceIdBytes.Length);
        Buffer.BlockCopy(serialNumberBytes, 0, combinedBytes, deviceIdBytes.Length, serialNumberBytes.Length);
        Buffer.BlockCopy(userIdBytes, 0, combinedBytes, deviceIdBytes.Length + serialNumberBytes.Length, userIdBytes.Length);
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(combinedBytes);
            string token = Convert.ToBase64String(hashBytes);
            return token;
        }
    }

    public static bool VerifyIoTToken(string token, Guid deviceId, string serialNumber, string userId)
    {
        string deviceIdString = deviceId.ToString();
        byte[] deviceIdBytes = Encoding.UTF8.GetBytes(deviceIdString);
        byte[] serialNumberBytes = Encoding.UTF8.GetBytes(serialNumber);
        byte[] userIdBytes = Encoding.UTF8.GetBytes(userId);

        byte[] combinedBytes = new byte[deviceIdBytes.Length + serialNumberBytes.Length + userIdBytes.Length];

        Buffer.BlockCopy(deviceIdBytes, 0, combinedBytes, 0, deviceIdBytes.Length);
        Buffer.BlockCopy(serialNumberBytes, 0, combinedBytes, deviceIdBytes.Length, serialNumberBytes.Length);
        Buffer.BlockCopy(userIdBytes, 0, combinedBytes, deviceIdBytes.Length + serialNumberBytes.Length, userIdBytes.Length);

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(combinedBytes);
            string expectedToken = Convert.ToBase64String(hashBytes);

            return expectedToken == token;
        }
    }
}