using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
	public interface IStudioService
	{
		public Task<List<StudioModel>> GetStudioListAsync(string Id);
		public Task<List<StudioModel>> GetStudiosProfesionalListAsync(string idProfesional, string IdPaciente);
		public Task<List<StudioModel>> GetStudiosEntidadListAsync(string idEntidad, string IdPaciente);


    }
}
