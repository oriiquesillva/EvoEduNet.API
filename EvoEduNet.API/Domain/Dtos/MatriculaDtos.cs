using System;
using System.ComponentModel.DataAnnotations;

namespace EvoEduNet.API.Domain.Dtos
{
    public class CriarMatriculaDto
    {
        [Required(ErrorMessage = "O ID do aluno é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID do aluno deve ser maior que zero.")]
        public int AlunoId { get; set; }

        [Required(ErrorMessage = "O ID da turma é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "O ID da turma deve ser maior que zero.")]
        public int TurmaId { get; set; }
    }

    public class MatriculaResponseDto
    {
        public int Id { get; set; }
        public int AlunoId { get; set; }
        public string NomeAluno { get; set; }
        public int TurmaId { get; set; }
        public string NomeTurma { get; set; }
        public DateTime DataMatricula { get; set; }
        public string Mensagem { get; set; }
    }
}
