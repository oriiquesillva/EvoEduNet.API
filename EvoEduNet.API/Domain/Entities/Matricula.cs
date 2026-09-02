using System;

namespace EvoEduNet.API.Domain.Entities
{
    public class Matricula
    {
        public int Id { get; set; }
        public int AlunoId { get; set; }
        public int TurmaId { get; set; }
        public DateTime DataMatricula { get; set; } = DateTime.Now;
    }
}
