using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using EvoEduNet.API.Domain.Entities;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Repositories.Interfaces;

namespace EvoEduNet.API.Repositories
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AlunoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<Tuple<IEnumerable<Aluno>, int>> ObterPaginadoAsync(string nome, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var offset = (page - 1) * pageSize;

            const string sql = @"
                SELECT COUNT(1) 
                FROM dbo.Aluno
                WHERE (@Nome IS NULL OR @Nome = '' OR Nome LIKE '%' + @Nome + '%');

                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM dbo.Aluno
                WHERE (@Nome IS NULL OR @Nome = '' OR Nome LIKE '%' + @Nome + '%')
                ORDER BY Nome ASC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(sql, new 
                { 
                    Nome = string.IsNullOrWhiteSpace(nome) ? null : nome.Trim(),
                    Offset = offset,
                    PageSize = pageSize 
                }))
                {
                    var total = await multi.ReadFirstAsync<int>();
                    var alunos = await multi.ReadAsync<Aluno>();

                    return Tuple.Create(alunos, total);
                }
            }
        }

        public async Task<Aluno> ObterPorIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM dbo.Aluno
                WHERE Id = @Id;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Aluno>(sql, new { Id = id });
            }
        }

        public async Task<Aluno> ObterPorEmailAsync(string email)
        {
            const string sql = @"
                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM dbo.Aluno
                WHERE Email = @Email;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<Aluno>(sql, new { Email = email.Trim() });
            }
        }

        public async Task<int> InserirAsync(Aluno aluno)
        {
            const string sql = @"
                INSERT INTO dbo.Aluno (Nome, Email, DataNascimento, Ativo, DataCadastro)
                VALUES (@Nome, @Email, @DataNascimento, @Ativo, @DataCadastro);
                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.ExecuteScalarAsync<int>(sql, aluno);
            }
        }

        public async Task<bool> AtualizarAsync(Aluno aluno)
        {
            const string sql = @"
                UPDATE dbo.Aluno
                SET Nome = @Nome,
                    Email = @Email,
                    DataNascimento = @DataNascimento,
                    Ativo = @Ativo
                WHERE Id = @Id;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(sql, aluno);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> ExcluirLogicoAsync(int id)
        {
            const string sql = @"
                UPDATE dbo.Aluno
                SET Ativo = 0
                WHERE Id = @Id;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
                return rowsAffected > 0;
            }
        }
    }
}
