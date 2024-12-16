using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Custom;
using Model.DTOs;
using static Model.Enums;

namespace Repository
{
    public class RegistroService : IRegistroService
    {
        protected readonly DbContextClass _dbContextClass;
        private readonly Usefulness _usefulness;

        public RegistroService(DbContextClass dbContextClass, Usefulness usefulness)
        {
            _dbContextClass = dbContextClass;
            _usefulness = usefulness;
        }

        public async Task<int> AddUserAsync(RegistroModel user)
        {
            var parameter = new List<SqlParameter>
                {
                    new SqlParameter("@IdRegistro", user.IdRegistro),
                    new SqlParameter("@Correo", user.Correo),
                    new SqlParameter("@Clave", _usefulness.encryptSHA256(user.Clave)),
                    new SqlParameter("@Tipo", user.Tipo),
                    new SqlParameter("@FechaCreacion", DateTime.Now),
                    new SqlParameter("@FechaModificacion", DateTime.Now),
                    new SqlParameter("@DebeCambiarPassword", user.DebeCambiarPassword),
                };

            try
            {
                //await _dbContextClass.Registros.AddAsync(user);
                //var result = await _dbContextClass.SaveChangesAsync();

                var result = await _dbContextClass.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.sp_crearUser @IdRegistro, @Correo, @Clave, @Tipo, @FechaCreacion, @FechaModificacion,@DebeCambiarPassword", parameter.ToArray());

                return result;
            }
            catch (DbUpdateException ex)
            {
                // Manejo específico de excepciones de actualización de base de datos
                throw new Exception("Error al actualizar la base de datos: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Manejo genérico de excepciones
                throw new Exception("Error al agregar el usuario: " + ex.Message);
            }
        }

        public async Task<int> DeleteUserAsync(string id, int tipo)
        {
            var parameter = new List<SqlParameter>
        {
            new SqlParameter("@IdRegistro",id),           
            new SqlParameter("@Tipo", tipo)
        };

            var result = await _dbContextClass.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_eliminarUser @IdRegistro, @Tipo", parameter.ToArray());

            return result;
        }

        public async Task<int> UpdateUserAsync(RegistroDto user)
        {
            var parameter = new List<SqlParameter>
        {
            new SqlParameter("@IdRegistro", user.IdRegistro),
            new SqlParameter("@Correo", user.Correo),
            new SqlParameter("@Clave", _usefulness.encryptSHA256(user.Clave)),
            new SqlParameter("@DebeCambiarPassword", user.DebeCambiarPassword),
            new SqlParameter("@FechaModificacion", DateTime.Now),
            new SqlParameter("@Tipo", user.Tipo)
        };

            var result = await _dbContextClass.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_editarUser @IdRegistro, @Correo, @Clave, @DebeCambiarPassword, @FechaModificacion,@Tipo ",
                parameter.ToArray()
            );

            return result;
        }


        public async Task<IEnumerable<RegistroModel>> GetUserAsync(RegistroDto user)
        {
            var parameter = new List<SqlParameter>();
            parameter.Add(new SqlParameter("@IdRegistro", user.IdRegistro));
            parameter.Add(new SqlParameter("@Clave", _usefulness.encryptSHA256(user.Clave)));            
            parameter.Add(new SqlParameter("@Tipo", user.Tipo));
            

            var access = await _dbContextClass.Registros
                                      .FromSqlRaw("exec dbo.sp_acceso @IdRegistro, @Clave, @Tipo", parameter.ToArray())
                                      .ToListAsync();
            return access;


        }

        public async Task<IEnumerable<StudioModel>> GetUserTypeAsync(string id, int type, string? email = null, string? FechaNac = null)
        {

            if (type.Equals(Enums.Tipos.Paciente.GetHashCode()))
            {
                var parameters = new[]
                {
                    new SqlParameter("@IdPaciente", id),
                    new SqlParameter("@EMAIL", email ?? (object)DBNull.Value),  // Convertir a DBNull si es null
                    new SqlParameter("@FechaNac",  FechaNac ?? (object)DBNull.Value)
                };
                return await _dbContextClass.Set<StudioModel>()
                .FromSqlRaw("EXECUTE dbo.sp_getPacientePorId @IdPaciente, @EMAIL, @FechaNac", parameters)
                .ToListAsync();
                //return await _dbContextClass.Set<StudioModel>()
                //                .FromSqlRaw($"EXECUTE dbo.sp_getPacientePorId {id}")
                //                .ToListAsync();
            }
            else if (type.Equals(Enums.Tipos.Profesional.GetHashCode()))
            {
                var parameters = new[]
                {
                    new SqlParameter("@IdProfesional", id),
                    new SqlParameter("@EMAIL", email ?? (object)DBNull.Value),  // Convertir a DBNull si es null
                   
                };
                return await _dbContextClass.Set<StudioModel>()
                                .FromSqlRaw($"EXECUTE dbo.sp_ProfesionalPorId @IdProfesional, @EMAIL",parameters)
                                .ToListAsync();
            }
                
            else if (type.Equals(Enums.Tipos.Entidad.GetHashCode()))
            {
                var parameters = new[]
               {
                    new SqlParameter("@IdEntidad", id),
                    new SqlParameter("@EMAIL", email ?? (object)DBNull.Value),  // Convertir a DBNull si es null
                   
                };
                return await _dbContextClass.Set<StudioModel>()
                                .FromSqlRaw($"EXECUTE sp_getEntidadPorId @IdEntidad, @EMAIL", parameters)
                                .ToListAsync();
            }
                
            else
                return null;

        }

        public async Task<IEnumerable<RegistroModel>> GetUserUpdateAsync(string id, int tipo, string? email)
        {
            var parameters = new[]
            {
        new SqlParameter("@IdRegistro", id),
        new SqlParameter("@Correo", email ?? (object)DBNull.Value),  // Convertir a DBNull si es null
        new SqlParameter("@Tipo", tipo)
    };
            
            return await _dbContextClass.Set<RegistroModel>()
                       .FromSqlRaw("EXECUTE dbo.sp_getUserUpdate @IdRegistro, @Correo, @Tipo", parameters)
                       .ToListAsync();
        }
    }
}
