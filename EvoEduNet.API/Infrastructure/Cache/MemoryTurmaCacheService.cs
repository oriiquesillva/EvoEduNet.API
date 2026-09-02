using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EvoEduNet.API.Domain.Dtos;
using EvoEduNet.API.Services.Interfaces;

namespace EvoEduNet.API.Infrastructure.Cache
{
    /// <summary>
    /// Implementação de cache em memória para a listagem de turmas.
    /// Segue a interface ITurmaCacheService para permitir fácil substituição por Redis.
    /// </summary>
    public class MemoryTurmaCacheService : ITurmaCacheService
    {
        private static IEnumerable<TurmaResponseDto> _cache;
        private static DateTime? _cacheExpiration;
        private static readonly object _lock = new object();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public Task<IEnumerable<TurmaResponseDto>> ObterTurmasAsync()
        {
            lock (_lock)
            {
                if (_cache != null && _cacheExpiration.HasValue && DateTime.UtcNow < _cacheExpiration.Value)
                {
                    return Task.FromResult(_cache);
                }

                return Task.FromResult<IEnumerable<TurmaResponseDto>>(null);
            }
        }

        public Task InserirAsync(IEnumerable<TurmaResponseDto> turmas)
        {
            lock (_lock)
            {
                _cache = turmas;
                _cacheExpiration = DateTime.UtcNow.Add(CacheDuration);
            }

            return Task.CompletedTask;
        }

        public Task InvalidarAsync()
        {
            lock (_lock)
            {
                _cache = null;
                _cacheExpiration = null;
            }

            return Task.CompletedTask;
        }
    }
}
