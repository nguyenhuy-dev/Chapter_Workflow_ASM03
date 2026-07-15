using Grpc.Core;
using MangaWorkflow.APIGrpcService.HuyNQ.Commons;
using MangaWorkflow.APIGrpcService.HuyNQ.Protos;
using MangaWorkflow.Services.HuyNQ;
using MangaWorkflow.Services.HuyNQ.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MangaWorkflow.APIGrpcService.HuyNQ.Services;

public class SystemUserAccountGRPCService : SystemUserAccountGRPC.SystemUserAccountGRPCBase
{
    private readonly IConfiguration _config;
    private readonly ISystemUserAccountService _userAccountsService;
    private readonly ITokenBlacklistService _tokenBlacklist;

    public SystemUserAccountGRPCService(IConfiguration config, ISystemUserAccountService userAccountsService, ITokenBlacklistService tokenBlacklist)
    {
        _config = config;
        _userAccountsService = userAccountsService;
        _tokenBlacklist = tokenBlacklist;
    }

    public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
    {
        var response = await _userAccountsService.GetUserAccount(new GetUserAccountRequest
        {
            UserName = request.UserName,
            Password = request.Password
        });

        if (response == null)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid username or password."));

        var token = GenerateJSONWebToken(response);

        return new LoginReply { Token = token };
    }

    [Authorize]
    public override Task<LogoutReply> Logout(LogoutRequest request, ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (string.IsNullOrEmpty(jti))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Token does not contain a valid identifier."));

        // Revoke the token until its own expiry so it can no longer be used.
        var expiresUtc = GetTokenExpiryUtc(user);
        _tokenBlacklist.Revoke(jti, expiresUtc);

        return Task.FromResult(new LogoutReply
        {
            StatusCode = StatusCodes.Status200OK,
            Message = "Logged out successfully"
        });
    }

    private static DateTime GetTokenExpiryUtc(ClaimsPrincipal user)
    {
        var expClaim = user.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

        if (long.TryParse(expClaim, out var expSeconds))
            return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;

        // Fallback: mirror the token lifetime used at login.
        return DateTime.UtcNow.AddMinutes(120);
    }

    private string GenerateJSONWebToken(GetUserAccountResponse systemUserAccount)
    {
        if (systemUserAccount.RoleId == 3)
        {
            return string.Empty;
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_config["Jwt:Issuer"]
                , _config["Jwt:Audience"]
                , new Claim[]
                {
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Name, systemUserAccount.UserName),
                    new(ClaimTypes.Role, systemUserAccount.RoleId.ToString()),
                },
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
