using System.IdentityModel.Tokens.Jwt;

namespace Woose.API
{
    public interface ICryptoHandler
    {
        string Base64Encode(string contenxt);

        string Base64Decode(string contenxt);

        string AESEncode(string contenxt);

        string AESDecode(string contenxt);

        string AES128Encode(string contenxt);

        string AES128Decode(string contenxt);

        string AES256Encode(string contenxt);

        string AES256Decode(string contenxt);

        string SHA256Encrypt(string contenxt);

        bool SHA256ValidateCheck(string targetHash, string keyString);

        string SHA512Encrypt(string contenxt);
        bool SHA512ValidateCheck(string targetHash, string keyString);

        JwtToken? GenerateTokens(string userId, string userName);
        RefreshToken? GenerateRefreshToken(string rtoken);
        JwtSecurityToken? ReadToken(string token);
        JwtToken? RefreshAccessToken(string refreshToken);
        User? GetUserFromToken(string token);
    }
}
