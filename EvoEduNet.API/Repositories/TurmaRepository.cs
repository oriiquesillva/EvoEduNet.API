using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Repositories.Interfaces;

namespace EvoEduNet.API.Repositories
{
    public class TurmaRepository : ITurmaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TurmaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<Turma>> ObterTodasAsync()
        {
            const string sql = @"
                SELECT Id, Nome, Periodo, VagasTotal, VagasDisponiveis
                FROM dbo.Turma
                ORDER BY Nome ASC;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<Turma>(sql);
            }
        }

        public async Task<Turma> ObterPorIdAsync(int id, IDbTransaction transaction = null)
        {
            const string sql = @"
                SELECT Id, Nome, Periodo, VagasTotal, VagasDisponiveis
                FROM dbo.Turma
                WHERE Id = @Id;
            ";

            if (transaction != null)
            {
                return await transaction.Connection.QueryFirstOrDefaultAsync<Turma>(sql, new { Id = id }, transaction: transaction);
            }

            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Turma>(sql, new { Id = id });
            }
        }

        public async Task<bool> DecrementarVagasAsync(int turmaId, IDbTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "A operação de decremento de vagas exige uma transação aberta (IDbTransaction).");
            }

            // O predicado 'AND VagasDisponiveis > 0' impede decremento abaixo de zero em concorrência
            const string sql = @"
                UPDATE dbo.Turma
                SET VagasDisponiveis = VagasDisponiveis - 1
                WHERE Id = @Id AND VagasDisponiveis > 0;
            ";

            var rowsAffected = await transaction.Connection.ExecuteAsync(sql, new { Id = turmaId }, transaction: transaction);
            return rowsAffected > 0;
        }
    }
}
