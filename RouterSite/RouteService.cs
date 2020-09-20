using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RouterSite
{
    public class RouteService : IDisposable
    {
        private readonly IDbConnection _connection;

        public RouteService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task CreateDbIfMissing()
        {
            if (!(await _connection.ExecuteScalarAsync<bool>("SELECT EXISTS( SELECT name FROM sqlite_master WHERE type='table' AND name='routes' );")))
                await _connection.ExecuteAsync("CREATE TABLE routes (path TEXT PRIMARY KEY, destination TEXT)");
        }

        public Task<IEnumerable<Route>> GetRoutes() =>
            _connection.QueryAsync<Route>("SELECT Path, Destination FROM routes");

        public async Task UpsertRoute(string path, string destination)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            _ = destination ?? throw new ArgumentNullException(nameof(destination));

            await _connection.ExecuteAsync(
                sql: @"INSERT INTO routes(path, destination) VALUES(@Path, @Destination)
                        ON CONFLICT(path) DO UPDATE SET destination = excluded.destination;",
                param: new
                {
                    Path = path,
                    Destination = destination
                });
        }

        public async Task DeleteRoute(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));

            await _connection.ExecuteAsync(
                sql: @"DELETE FROM routes WHERE path = @Path;",
                param: new { Path = path });
        }

        public async Task<(bool success, string destination)> GetDestination(string path)
        {
            var destination = await _connection.ExecuteScalarAsync<string>("SELECT destination FROM routes WHERE path = @Path;", new { Path = path });
            return (!(destination is null), destination);
        }


        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
