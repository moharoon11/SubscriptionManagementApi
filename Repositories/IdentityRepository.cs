using Dapper;
using SubscriptionPlatform.Api.Data;

namespace SubscriptionPlatform.Api.Repositories;

public sealed class IdentityRepository(ConnectionFactory connections)
{
    public async Task<Guid?> FindActiveApplicationAsync(string keyHash){await using var db=connections.Create();return await db.QuerySingleOrDefaultAsync<Guid?>("SELECT id FROM registered_applications WHERE key_hash=@keyHash AND active=1",new{keyHash});}
    public async Task<Guid> CreateApplicationAsync(string name,string hash){var id=Guid.NewGuid();await using var db=connections.Create();await db.ExecuteAsync("INSERT INTO registered_applications(id,name,key_hash,active,created_utc) VALUES(@id,@name,@hash,1,UTC_TIMESTAMP(6))",new{id,name,hash});return id;}
    public async Task<IEnumerable<object>> ListApplicationsAsync(){await using var db=connections.Create();return await db.QueryAsync("SELECT id,name,active,created_utc FROM registered_applications");}
    public async Task<bool> DeactivateApplicationAsync(Guid id){await using var db=connections.Create();return await db.ExecuteAsync("UPDATE registered_applications SET active=0 WHERE id=@id",new{id})>0;}
}
