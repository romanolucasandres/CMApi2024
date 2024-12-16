using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Model;
using Model.DTOs;
using Microsoft.Extensions.Configuration;

namespace Model.Custom
{
	public class Usefulness
	{
		private readonly IConfiguration _configuration;
		public Usefulness(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		//Encriptacion
		public string encryptSHA256(string input)
		{
			using (SHA256 sha256HASH = SHA256.Create())
			{
				//hacer hash
				byte[] bytes = sha256HASH.ComputeHash(Encoding.UTF8.GetBytes(input));

				// convierto el array de bytes a string

				StringBuilder builder = new StringBuilder();
				for (int i = 0; i < bytes.Length; i++)
				{
					builder.Append(bytes[i].ToString("X2"));
				}

				return builder.ToString();

			}

		}
		public Auditoria CrearAuditoria(string ip, string nombrepc, string detalle, bool resultado, Enums.Protocolo protocolo, RegistroDto registrodto)
		{
			Auditoria auditoria = new Auditoria()
			{
				Id = Guid.NewGuid(),
				IdUsuario = registrodto.IdRegistro,
				TipoUsuario = registrodto.Tipo,
				Descripcion = detalle,
				FechaCreacion = DateTime.Now,
				IpUsuario = ip,
				NombrePCUsuario = nombrepc,
				Protocolo = protocolo.ToString(),
				Resultado = resultado
			};
			return auditoria;	
		}
		//generar JWT
		public string generarJWT(RegistroDto modelo)
		{
			//crear informacion del usuario para el token
			var userClaims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, modelo.IdRegistro)
		
			};

			var securityKEY = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:key"]!));
			var credentials = new SigningCredentials(securityKEY, SecurityAlgorithms.HmacSha256Signature);

			//crear detalle del token

			var jwtConfig = new JwtSecurityToken(
				claims: userClaims,
				expires: DateTime.UtcNow.AddMinutes(15),
				signingCredentials: credentials
				);
			return new JwtSecurityTokenHandler().WriteToken(jwtConfig);
		}

	}
}
