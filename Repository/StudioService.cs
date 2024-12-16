using Microsoft.EntityFrameworkCore;
using Model;

namespace Repository
{
	public class StudioService : IStudioService
	{
		protected readonly DbContextClass _dbContextClass;

		public StudioService(DbContextClass dbContextClass)
		{
			_dbContextClass = dbContextClass;
		}

        public async Task<List<StudioModel>> GetStudioListAsync(string Id)
        {
            return await _dbContextClass.Set<StudioModel>()
                .FromSqlInterpolated($"EXECUTE dbo.sp_listaPacientePorId {Id}")
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<List<StudioModel>> GetStudiosProfesionalListAsync(string idProfesional,string IdPaciente )
        {
            return await _dbContextClass.Set<StudioModel>()
                                .FromSqlRaw($"EXECUTE dbo.sp_listaProfesionalPorId {idProfesional}, {IdPaciente}")
                                .AsNoTracking()
                                .ToListAsync();
        }

        public async Task<List<StudioModel>> GetStudiosEntidadListAsync(string idEntidad, string IdPaciente)
        {
            return await _dbContextClass.Set<StudioModel>()
                                .FromSqlRaw($"EXECUTE dbo.sp_listaEntidadPorId  {idEntidad},{IdPaciente}")
                                .AsNoTracking()
                                .ToListAsync();
        }
    }
}
