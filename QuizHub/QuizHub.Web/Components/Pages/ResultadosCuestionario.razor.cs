using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using QuizHub.Web.Data;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizHub.Web.Components.Pages
{
    public partial class ResultadosCuestionario : ComponentBase
    {
        [Parameter]
        public int Id { get; set; }

        private List<Pregunta> preguntas;
        private List<Respuesta> respuestas;
        private double puntuacionUsuario;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // Cargar preguntas y respuestas
                preguntas = await (from pq in DbContext.preguntasCuestionario
                                 join p in DbContext.preguntas on pq.IdPregunta equals p.Id
                                 where pq.IdCuestionario == Id
                                 select p).ToListAsync();

                respuestas = await (from r in DbContext.respuestas
                                  where preguntas.Select(p => p.Id).Contains(r.IdPregunta.Value)
                                  select r).ToListAsync();

                // Obtener la puntuación del usuario
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var userId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (userId != null)
                {
                    var puntuacion = await DbContext.puntuacion
                        .Where(p => p.IdCuestionario == Id && p.IdUsuario == userId)
                        .FirstOrDefaultAsync();

                    if (puntuacion != null)
                    {
                        puntuacionUsuario = puntuacion.puntuacion;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar resultados: {ex.Message}");
                // Aquí podrías manejar el error de una manera más apropiada
            }
        }

        private void NavigateBack()
        {
            NavigationManager.NavigateTo("/preguntasAlumno");
        }
    }
} 