using Model;
using Model.DTOs;

namespace Repository
{
    public interface IAuditoriaService
    {
        public Task<int> AddAuditoriaAsync(Auditoria user);
    }
}
