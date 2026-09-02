namespace EvoEduNet.API.Domain.Dtos
{
    public class TurmaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Periodo { get; set; }
        public int VagasTotal { get; set; }
        public int VagasDisponiveis { get; set; }
    }
}
