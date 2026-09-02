using System;

namespace EvoEduNet.API.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Representa violação de regras de negócio (mapeada para HTTP 409 Conflict).
    /// Exemplos: Turma sem vagas, Aluno inativo, Matrícula duplicada.
    /// </summary>
    public class BusinessException : DomainException
    {
        public BusinessException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Representa recurso não encontrado (mapeada para HTTP 404 Not Found).
    /// Exemplos: Aluno não encontrado, Turma não encontrada.
    /// </summary>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Representa dados inválidos enviados na requisição (mapeada para HTTP 400 Bad Request).
    /// </summary>
    public class ValidationException : DomainException
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}
