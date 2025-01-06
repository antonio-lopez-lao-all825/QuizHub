using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuizHub.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Pregunta> preguntas { get; set; }
        public DbSet<Respuesta> respuestas { get; set; }
        public DbSet<Cuestionario> cuestionarios { get; set; }
        public DbSet<PreguntasCuestionario> preguntasCuestionario { get; set; }
        public DbSet<Asignatura> asignaturas { get; set; }
        public DbSet<AsinaturasUsuario> asignaturasUsuario { get; set; }
        public DbSet<Puntuacion> puntuacion { get; set; }

    }
}
