using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities.Models;
using TurkSoft.Service.Inferfaces;

namespace TurkSoft.Service.Implementations
{
    public class ClCardRepository : IClCardRepository
    {
        private readonly string _connectionString;
        public ClCardRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TigerConnection");
        }
        public async Task<List<ClCard>> GetAllClCardsAsync()
        {
            var cards = new List<ClCard>();
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                    SELECT 
                        CODE, 
                        DEFINITION_ AS NAME
                    FROM 
                        LG_001_01_CLCARD
                    WHERE 
                        ACTIVE = 0 AND CARDTYPE = 1
                    ORDER BY 
                        CODE";

                using (var command = new SqlCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var card = new ClCard
                        {
                            Code = reader["CODE"] != DBNull.Value ? reader["CODE"].ToString() : string.Empty,
                            Name = reader["NAME"] != DBNull.Value ? reader["NAME"].ToString() : string.Empty
                        };
                        cards.Add(card);
                    }
                }

            }
            return cards;
        }

        public async Task<ClCard> GetClCardByCodeAsync(string code)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = @"
                        SELECT
                            CODE,
                            DEFINITION_ AS NAME
                        FROM
                            LG_001_01_CLCARD
                        WHERE
                            CODE=@Code AND ACTIVE=0 AND CARDTYPE=1";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Code", code);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ClCard
                            {
                                Code = reader["CODE"] != DBNull.Value ? reader["CODE"].ToString() : string.Empty,
                                Name = reader["NAME"] != DBNull.Value ? reader["NAME"].ToString() : string.Empty
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<ClCard>> SearchClCardsByNameAsync(string searchTerm)
        {
            var cards = new List<ClCard>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        CODE, 
                        DEFINITION_ AS NAME
                    FROM 
                        LG_001_01_CLCARD
                    WHERE 
                        (DEFINITION_ LIKE @SearchTerm + '%' OR CODE LIKE @SearchTerm + '%')
                        AND ACTIVE = 0 AND CARDTYPE = 1
                    ORDER BY 
                        CODE";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var card = new ClCard
                            {
                                Code = reader["CODE"] != DBNull.Value ? reader["CODE"].ToString() : string.Empty,
                                Name = reader["NAME"] != DBNull.Value ? reader["NAME"].ToString() : string.Empty
                            };
                            cards.Add(card);
                        }
                    }
                }
            }

            return cards;
        }
    }
}
