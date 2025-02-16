﻿using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using QuizHub.Web.Data;

namespace QuizHub.Web.Components.Pages
{
    public partial class PreguntasCuestionarioAlumno
    {
        [Parameter]
        public int Id { get; set; }

        private List<Pregunta> preguntas;
        private List<Respuesta> respuestas;
        private Dictionary<int, int> respuestasUsuario = new();
        private int? puntuacionFinal;

        protected override async Task OnInitializedAsync()
        {
            // Cargar preguntas asociadas al cuestionario
            preguntas = await (from pq in DbContext.preguntasCuestionario
                               join p in DbContext.preguntas on pq.IdPregunta equals p.Id
                               where pq.IdCuestionario == Id
                               select p).ToListAsync();

            // Cargar respuestas asociadas a las preguntas del cuestionario
            respuestas = await (from r in DbContext.respuestas
                                where preguntas.Select(p => p.Id).Contains(r.IdPregunta.Value)
                                select r).ToListAsync();
        }

        private void SeleccionarRespuesta(int preguntaId, int respuestaId)
        {
            respuestasUsuario[preguntaId] = respuestaId;
        }

        private async Task CalcularPuntuacion()
        {
            // Verificar cuántas respuestas son correctas
            int correctas = respuestas.Count(r =>
                respuestasUsuario.TryGetValue(r.IdPregunta.Value, out var respuestaId) &&
                r.Id == respuestaId &&
                r.Correcta == true);

            // Escalar puntuación sobre 10
            puntuacionFinal = (correctas * 10) / preguntas.Count;

            // Obtener el usuario autenticado
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/Account/login");
                return;
            }

            // Obtener el ID del usuario usando ClaimTypes.NameIdentifier en lugar del email
            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("Error: No se pudo obtener el ID del usuario");
                return;
            }

            // Guardar la puntuación en la base de datos
            var nuevaPuntuacion = new Puntuacion
            {
                IdCuestionario = Id,
                IdUsuario = userId,  // Ahora aseguramos que siempre sea el ID
                puntuacion = puntuacionFinal.Value
            };

            DbContext.puntuacion.Add(nuevaPuntuacion);
            await DbContext.SaveChangesAsync();
        }

    }
}
