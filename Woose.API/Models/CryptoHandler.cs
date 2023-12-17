using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Woose.Core;
using Woose.Data;

namespace Woose.API
{
    public class CryptoHandler : ICryptoHandler
    {
        public CryptoHandler()
        {
            CryptoHelper.AES.Set(WooseSecret.PublicKey, WooseSecret.SecretKey);
            CryptoHelper.AES128.Set(WooseSecret.SecretKey);
            CryptoHelper.AES256.Set(WooseSecret.SecretKey);
        }

        public string AES128Decode(string contenxt)
        {
            return CryptoHelper.AES128.Decrypt(contenxt);
        }

        public string AES128Encode(string contenxt)
        {
            return CryptoHelper.AES128.Encrypt(contenxt);
        }

        public string AES256Decode(string contenxt)
        {
            return CryptoHelper.AES256.Decrypt(contenxt);
        }

        public string AES256Encode(string contenxt)
        {
            return CryptoHelper.AES256.Encrypt(contenxt);
        }

        public string AESDecode(string contenxt)
        {
            return CryptoHelper.AES.Decrypt(contenxt);
        }

        public string AESEncode(string contenxt)
        {
            return CryptoHelper.AES.Encrypt(contenxt);
        }

        public string Base64Decode(string contenxt)
        {
            return CryptoHelper.BASE64.Decrypt(contenxt);
        }

        public string Base64Encode(string contenxt)
        {
            return CryptoHelper.BASE64.Encrypt(contenxt);
        }

        public string SHA256Encrypt(string contenxt)
        {
            return CryptoHelper.SHA256.Encrypt(contenxt);
        }

        public bool SHA256ValidateCheck(string targetHash, string keyString)
        {
            return CryptoHelper.SHA256.ValidateCheck(targetHash, keyString);
        }

        public string SHA512Encrypt(string contenxt)
        {
            return CryptoHelper.SHA512.Encrypt(contenxt);
        }

        public bool SHA512ValidateCheck(string targetHash, string keyString)
        {
            return CryptoHelper.SHA512.ValidateCheck(targetHash, keyString);
        }


        public RefreshToken? GenerateRefreshToken(string userid)
        {
            if (!string.IsNullOrWhiteSpace(userid))
            {
                try
                {
                    return new RefreshToken
                    {
                        Token = CryptoHelper.AES256.Encrypt(userid),
                        Expiration = DateTime.UtcNow.AddMonths(3)
                    };
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public JwtToken? GenerateTokens(string userId, string userName = "", string serverToken = "")
        {
            if (!string.IsNullOrWhiteSpace(userId))
            {
                try
                {

                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(WooseSecret.JwtSecret));
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                    new Claim(ClaimTypes.Name, userId),
                    new Claim("name", userName),
                    new Claim("servertoken", serverToken)
                }),
                        Expires = DateTime.UtcNow.AddDays(1),
                        SigningCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature)
                    };

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var accessToken = tokenHandler.CreateToken(tokenDescriptor);
                    var refreshToken = GenerateRefreshToken(userId);

                    return new JwtToken
                    {
                        AccessToken = tokenHandler.WriteToken(accessToken),
                        AccessTokenExpiration = tokenDescriptor.Expires.Value,
                        RefreshToken = refreshToken.Token
                    };
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public User? GetUserFromToken(string token)
        {
            User result = new User();

            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    var claimsPrincipal = handler.ReadJwtToken(token);

                    // 토큰 해석
                    if (claimsPrincipal != null)
                    {
                        // 사용자 아이디 추출
                        var userId = claimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == "unique_name");
                        var userName = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "name");
                        var serverToken = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == "servertoken");

                        if (userId != null && userName != null)
                        {
                            result.Id = userId.Value;
                            result.Name = userName.Value;
                        }
                    }
                }
                catch
                {
                    result = null;
                }
            }

            return result;
        }

        public JwtSecurityToken? ReadToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    return handler.ReadJwtToken(token);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public JwtToken? RefreshAccessToken(string refreshToken)
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                var refreshTokenHandler = new JwtSecurityTokenHandler();
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(WooseSecret.JwtSecret));

                try
                {
                    var principal = refreshTokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = secretKey,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero // 토큰 만료 시간 검증
                    }, out var validatedToken);

                    if (validatedToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new SecurityTokenException("Invalid token");
                    }

                    string? userId = principal.FindFirst(ClaimTypes.Name)?.Value;
                    string? username = principal.FindFirst("name")?.Value;
                    string? serverToken = principal.FindFirst("servertoken")?.Value;
                    if (!string.IsNullOrWhiteSpace(userId))
                    {
                        var newAccessToken = GenerateTokens(userId, username ?? "");
                        return newAccessToken;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
