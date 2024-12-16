using Model;
using Model.DTOs;

namespace Repository
{
    public interface IRegistroService
    {
        public Task<int> AddUserAsync(RegistroModel user);
        public Task<int> UpdateUserAsync(RegistroDto usuario);
        public Task<int> DeleteUserAsync(string id, int tipo);
        public Task<IEnumerable<RegistroModel>> GetUserAsync(RegistroDto usuario);
       
        public Task<IEnumerable<StudioModel>> GetUserTypeAsync(string id, int type, string? email = null, string? fechanacimiento = null);
        public Task<IEnumerable<RegistroModel>> GetUserUpdateAsync(string id,int tipo, string? email);

    }
}
