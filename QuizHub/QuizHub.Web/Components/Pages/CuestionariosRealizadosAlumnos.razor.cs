using Microsoft.EntityFrameworkCore;

namespace QuizHub.Web.Components.Pages
{
    public partial class CuestionariosRealizadosAlumnos
    {
        private List<CuestionarioRealizado> puntuaciones;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity.IsAuthenticated)
            {
                puntuaciones = new List<CuestionarioRealizado>();
                return;
            }

            var userName = user.Identity.Name; // Usar el UserName directamente.

            puntuaciones = await (from p in DbContext.puntuacion
                                  join c in DbContext.cuestionarios on p.IdCuestionario equals c.Id
                                  where p.IdUsuario == userName // Filtrar por UserName.
                                  select new CuestionarioRealizado
                                  {
                                      CuestionarioName = c.Name,
                                      Puntuacion = p.puntuacion
                                  }).ToListAsync();
        }

        private string GetPuntuacionColorClass(int puntuacion)
        {
            return puntuacion switch
            {
                < 5 => "puntuacion-rojo",
                < 7 => "puntuacion-naranja",
                < 8 => "puntuacion-amarillo",
                < 9 => "puntuacion-amarillo-verdoso",
                _ => "puntuacion-verde"
            };
        }

        private class CuestionarioRealizado
        {
            public string CuestionarioName { get; set; }
            public int Puntuacion { get; set; }
        }
    }
}
