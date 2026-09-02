using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Infrastructure.Data;
using EvoEduNet.API.Repositories.Interfaces;

namespace EvoEduNet.API.Repositories
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RelatorioRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<IEnumerable<RelatorioAlunoTurmaDto>> ObterAlunosPorTurmaAsync()
        {
            // Consulta SQL agregada nativa com LEFT JOIN e GROUP BY no banco de dados (sem LINQ em memória)
            const string sql = @"
                SELECT 
                    t.Nome AS NomeTurma,
                    t.Periodo AS Periodo,
                    COUNT(m.Id) AS TotalAlunosMatriculados,
                    t.VagasDisponiveis AS VagasRestantes
                FROM dbo.Turma t
                LEFT JOIN dbo.Matricula m ON t.Id = m.TurmaId
                GROUP BY t.Id, t.Nome, t.Periodo, t.VagasDisponiveis
                ORDER BY t.Nome ASC;
            ";

            using (var connection = _connectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<RelatorioAlunoTurmaDto>(sql);
            }
        }
    }
}
