using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class AuditoriaService : IAuditoriaService
    {
        protected readonly DbContextClass _dbContextClass;
        private readonly Usefulness _usefulness;

        public AuditoriaService(DbContextClass dbContextClass, Usefulness usefulness)
        {
            _dbContextClass = dbContextClass;
            _usefulness = usefulness;
        }
        public async Task<int> AddAuditoriaAsync(Auditoria user)
        {
            var parameter = new List<SqlParameter>
            {
                new SqlParameter("@Id", user.Id),
                new SqlParameter("@IdUsuario", user.IdUsuario),
                new SqlParameter("@IpUsuario", user.IpUsuario),
                new SqlParameter("@NombrePCUsuario", user.NombrePCUsuario),
                new SqlParameter("@TipoUsuario", user.TipoUsuario),
                new SqlParameter("@FechaCreacion", DateTime.Now), // Fecha de creación nunca es nula
                new SqlParameter("@Protocolo", user.Protocolo),
                new SqlParameter("@Descripcion", user.Descripcion),
                new SqlParameter("@Resultado", user.Resultado)
            };
            //_dbContextClass.Update<Auditoria>(user);
            var result = await _dbContextClass.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_crearAuditoria @Id, @IdUsuario, @IpUsuario, @NombrePCUsuario, @TipoUsuario, @FechaCreacion, @Protocolo, @Descripcion, @Resultado",
                parameter.ToArray()
            );

            return result;
        }

    }
}
