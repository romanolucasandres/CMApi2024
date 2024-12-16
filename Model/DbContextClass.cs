using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace Model
{
	public class DbContextClass : DbContext
	{
		protected readonly  IConfiguration Configuration;

        public DbContextClass(IConfiguration configuration)
        {
            Configuration = configuration;
        }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer(Configuration.GetConnectionString("CadenaSQL"));
		}

		public virtual DbSet<RegistroModel> Registros { get; set; }
		public virtual DbSet<StudioModel> Studios { get; set; }
		//public virtual DbSet<PacienteModel> PacienteStudios { get; set; }
	}
}
