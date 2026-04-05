using System.Security.Cryptography;
using System.Text;
using Gci409.Application.Common;

namespace Gci409.Infrastructure.Security;

public sealed class RefreshTokenProtector : IRefreshTokenProtector
{
    public string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
