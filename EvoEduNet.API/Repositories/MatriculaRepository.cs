using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Repositories.Interfaces;

namespace EvoEduNet.API.Repositories
{
    public class MatriculaRepository : IMatriculaRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public MatriculaRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<int> InserirAsync(Matricula matricula, IDbTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "A operação de matrícula exige uma transação aberta (IDbTransaction).");
            }

            const string sql = @"
                INSERT INTO dbo.Matricula (AlunoId, TurmaId, DataMatricula)
                VALUES (@AlunoId, @TurmaId, @DataMatricula);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            return await transaction.Connection.ExecuteScalarAsync<int>(sql, matricula, transaction: transaction);
        }

        public async Task<bool> ExisteMatriculaAsync(int alunoId, int turmaId, IDbTransaction transaction = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 
                    FROM dbo.Matricula 
                    WHERE AlunoId = @AlunoId AND TurmaId = @TurmaId
                ) THEN 1 ELSE 0 END;
            ";

            if (transaction != null)
            {
                var existeNaTransacao = await transaction.Connection.ExecuteScalarAsync<int>(
                    sql, 
                    new { AlunoId = alunoId, TurmaId = turmaId }, 
                    transaction: transaction
                );
                return existeNaTransacao == 1;
            }

            using (var connection = _connectionFactory.CreateConnection())
            {
                var existe = await connection.ExecuteScalarAsync<int>(
                    sql, 
                    new { AlunoId = alunoId, TurmaId = turmaId }
                );
                return existe == 1;
            }
        }
    }
}
