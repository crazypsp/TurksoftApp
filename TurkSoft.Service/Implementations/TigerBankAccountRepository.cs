using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;

namespace TurkSoft.Service.Implementations
{
    public sealed class TigerBankAccountRepository : ITigerBankAccountRepository
    {
        private readonly string _connectionString;

        public TigerBankAccountRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TigerConnection");
        }

        public async Task<List<TigerBankAccount>> SearchAsync(string searchTerm, int take = 30)
        {
            var list = new List<TigerBankAccount>();
            searchTerm = (searchTerm ?? "").Trim();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT TOP (@Take)
                    CODE,
                    DEFINITION_ AS NAME
                FROM LG_001_01_BANKACC
                WHERE ACTIVE = 0
                  AND (
                        CODE LIKE @CodeLike
                        OR DEFINITION_ LIKE @NameLike
                  )
                ORDER BY CODE";
            cmd.Parameters.AddWithValue("@Take", take <= 0 ? 30 : take);
            cmd.Parameters.AddWithValue("@CodeLike", searchTerm + "%");
            cmd.Parameters.AddWithValue("@NameLike", "%" + searchTerm + "%");

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TigerBankAccount
                {
                    Code = reader["CODE"]?.ToString() ?? "",
                    Name = reader["NAME"]?.ToString() ?? ""
                });
            }

            return list;
        }
    }
}
