namespace EvoEduNet.API.Domain.Dtos
{
    public class RelatorioAlunoTurmaDto
    {
        public string NomeTurma { get; set; }
        public string Periodo { get; set; }
        public int TotalAlunosMatriculados { get; set; }
        public int VagasRestantes { get; set; }
    }
}
