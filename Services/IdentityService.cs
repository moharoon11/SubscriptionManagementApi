using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using SubscriptionPlatform.Api.Models;
using SubscriptionPlatform.Api.Repositories;

namespace SubscriptionPlatform.Api.Services;

public sealed class IdentityService(IdentityRepository repository,IConfiguration config,ILogger<IdentityService> logger)
{
    public async Task<string?> IssueTokenAsync(LoginRequest request){
        string role;var claims=new List<Claim>();
        if(!string.IsNullOrEmpty(request.AdminKey)&&Fixed(request.AdminKey,config["Authentication:BootstrapAdminKey"]))role="platform_admin";
        else if(!string.IsNullOrEmpty(request.ApplicationKey)){var app=await repository.FindActiveApplicationAsync(Hash(request.ApplicationKey));if(app is null)return null;role="application";claims.Add(new("app_id",app.Value.ToString()));}
        else return null;
        var key=Encoding.UTF8.GetBytes(config["Authentication:SigningKey"]!);var token=new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(config["Authentication:Issuer"],config["Authentication:Audience"],claims.Append(new Claim("role",role)),DateTime.UtcNow,DateTime.UtcNow.AddHours(1),new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256));return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<(Guid Id,string Key)> RegisterApplicationAsync(string name){try{var raw=Convert.ToBase64String(RandomNumberGenerator.GetBytes(36));return(await repository.CreateApplicationAsync(name,Hash(raw)),raw);}catch(Exception ex){logger.LogError(ex,"Application registration failed");throw;}}
    public Task<IEnumerable<object>> ListApplicationsAsync()=>repository.ListApplicationsAsync();
    public Task<bool> DeactivateApplicationAsync(Guid id)=>repository.DeactivateApplicationAsync(id);
    private static string Hash(string value)=>Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static bool Fixed(string a,string? b)=>!string.IsNullOrEmpty(b)&&CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(a)),SHA256.HashData(Encoding.UTF8.GetBytes(b)));
}
