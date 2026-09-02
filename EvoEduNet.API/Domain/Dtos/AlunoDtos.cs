using System;
using System.ComponentModel.DataAnnotations;

namespace EvoEduNet.API.Domain.Dtos
{
    public class CriarAlunoDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
        [StringLength(120, ErrorMessage = "O e-mail deve ter no máximo 120 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        public DateTime? DataNascimento { get; set; }
    }

    public class AtualizarAlunoDto
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado é inválido.")]
        [StringLength(120, ErrorMessage = "O e-mail deve ter no máximo 120 caracteres.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        public DateTime? DataNascimento { get; set; }

        public bool? Ativo { get; set; }
    }

    public class AlunoResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
