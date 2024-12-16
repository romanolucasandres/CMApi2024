namespace Model
{
    public class Enums
    {

        public enum Tipos
        {
            Paciente = 1,
            Profesional = 2,
            Entidad = 3
        }
       
        public enum Protocolo
        {
            Generico = 0,
            Alta_Usuario = 1,
            Cambiar_Contraseña = 2,            
            Recuperar_Contraseña = 3,
            Login=4,
            Buscar_Paciente=5,
            Abrir_Link=6,
            Envio_Mail=7,
            Intento_Recuperar_Contraseña=8,
            Intento_Inicio_Sesion=9,
            Intento_Alta_Usuario=10,
            
        }
    }
}
