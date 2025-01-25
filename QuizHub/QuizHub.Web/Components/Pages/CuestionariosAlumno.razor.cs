using Microsoft.EntityFrameworkCore;
using QuizHub.Web.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Radzen;

namespace QuizHub.Web.Components.Pages
{
    public static class NumberFormatExtensions
    {
        public static string ToScoreString(this double score)
        {
            // Si el número es entero (por ejemplo 9.0), mostrar solo el entero (9)
            if (Math.Round(score, 1) == Math.Floor(score))
                return Math.Floor(score).ToString();
            
            // Si tiene decimales, mostrar un decimal
            return score.ToString("F1").Replace(",", ".");
        }
    }

    public partial class CuestionariosAlumno
    {
        [Inject]
        private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        private List<Cuestionario> cuestionarios = new();
        private List<Cuestionario> cuestionariosFiltrados = new();
        private List<Cuestionario> completedCuestionarios = new();
        private string searchTerm = "";
        private bool showAll = true;
        private Dictionary<int, int> completionCounts = new();
        private Dictionary<int, double> averageScores = new();
        private Dictionary<int, double> userScores = new();

        private int paginaActual = 1;
        private int paginaCompletados = 1;
        private const int elementosPorPagina = 4;
        private List<string> asignaturasUnicas = new();
        private string filtroAsignatura = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadCuestionarios();
            asignaturasUnicas = cuestionarios.Select(c => c.Asignatura).Distinct().ToList();
        }

        private async Task LoadCuestionarios()
        {
            try
            {
                using var context = await DbFactory.CreateDbContextAsync();
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Cargar cuestionarios activos
                cuestionarios = await context.cuestionarios
                    .Where(c => c.Estado == 1)
                    .ToListAsync();

                if (userId != null)
                {
                    // Cargar puntuaciones del usuario
                    var userPuntuaciones = await context.puntuacion
                        .Where(p => p.IdUsuario == userId)
                        .ToListAsync();

                    userScores.Clear();
                    foreach (var puntuacion in userPuntuaciones)
                    {
                        userScores[puntuacion.IdCuestionario] = puntuacion.puntuacion;
                    }

                    // Separar cuestionarios completados y pendientes
                    completedCuestionarios = cuestionarios
                        .Where(c => userScores.ContainsKey(c.Id))
                        .ToList();

                    // Inicialmente mostrar solo los pendientes
                    cuestionariosFiltrados = cuestionarios
                        .Where(c => !userScores.ContainsKey(c.Id))
                        .ToList();
                }

                // Cargar estadísticas generales
                foreach (var cuestionario in cuestionarios)
                {
                    // Contar número de usuarios únicos que han completado el cuestionario
                    completionCounts[cuestionario.Id] = await context.puntuacion
                        .Where(p => p.IdCuestionario == cuestionario.Id)
                        .Select(p => p.IdUsuario)
                        .Distinct()
                        .CountAsync();

                    // Calcular la media usando solo la última puntuación de cada usuario
                    var latestScores = await context.puntuacion
                        .Where(p => p.IdCuestionario == cuestionario.Id)
                        .GroupBy(p => p.IdUsuario)
                        .Select(g => g.OrderByDescending(p => p.Id).First().puntuacion)
                        .ToListAsync();

                    averageScores[cuestionario.Id] = latestScores.Any() 
                        ? Math.Round(latestScores.Average(), 1) 
                        : 0;
                }
            }
            catch (Exception ex)
            {
                // Manejar el error de forma apropiada para tu aplicación
                Console.WriteLine($"Error al cargar cuestionarios: {ex.Message}");
            }
        }

        private void FilterCuestionarios()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Si no hay término de búsqueda, mostrar todos o solo pendientes según el filtro
                cuestionariosFiltrados = showAll 
                    ? cuestionarios 
                    : cuestionarios.Where(c => !HasCompletedCuestionario(c.Id)).ToList();
            }
            else
            {
                // Si hay término de búsqueda, aplicar filtro de texto y estado
                var term = searchTerm.ToLower();
                var filtered = cuestionarios
                    .Where(c => c.Name.ToLower().Contains(term) || 
                               c.Asignatura.ToLower().Contains(term));

                // Aplicar filtro de estado si no se muestran todos
                if (!showAll)
                {
                    filtered = filtered.Where(c => !HasCompletedCuestionario(c.Id));
                }

                cuestionariosFiltrados = filtered.ToList();
            }

            StateHasChanged();
        }

        private void FilterByStatus(bool showAllCuestionarios)
        {
            showAll = showAllCuestionarios;
            FilterCuestionarios();
        }

        private bool HasCompletedCuestionario(int cuestionarioId)
        {
            // Verificar si el usuario tiene una puntuación para este cuestionario
            return userScores.ContainsKey(cuestionarioId);
        }

        private int GetCompletionCount(int cuestionarioId)
        {
            return completionCounts.TryGetValue(cuestionarioId, out int count) ? count : 0;
        }

        private string GetAverageScore(int cuestionarioId)
        {
            return averageScores.TryGetValue(cuestionarioId, out double score) 
                ? score.ToScoreString()
                : "0";
        }

        private string GetUserScore(int cuestionarioId)
        {
            return userScores.TryGetValue(cuestionarioId, out double score) 
                ? score.ToScoreString()
                : "0";
        }

        private void NavigateToQuiz(int cuestionarioId)
        {
            if (HasCompletedCuestionario(cuestionarioId))
            {
                NavigationManager.NavigateTo($"/mostrarResultadosCuestionario/{cuestionarioId}");
            }
            else
            {
                NavigationManager.NavigateTo($"/detalleCuestionario/{cuestionarioId}");
            }
        }

        private async Task OnPageChanged(PagerEventArgs args)
        {
            paginaActual = args.PageIndex + 1;
            await InvokeAsync(StateHasChanged);
        }

        private async Task OnCompletedPageChanged(PagerEventArgs args)
        {
            paginaCompletados = args.PageIndex + 1;
            await InvokeAsync(StateHasChanged);
        }

        private IEnumerable<Cuestionario> GetCuestionariosPaginados()
        {
            return cuestionariosFiltrados
                .Skip((paginaActual - 1) * elementosPorPagina)
                .Take(elementosPorPagina);
        }

        private IEnumerable<Cuestionario> GetCompletedCuestionariosPaginados()
        {
            return completedCuestionarios
                .Skip((paginaCompletados - 1) * elementosPorPagina)
                .Take(elementosPorPagina);
        }

        private void FiltrarCuestionarios()
        {
            var query = cuestionarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtroAsignatura) && filtroAsignatura != "Todas las asignaturas")
            {
                query = query.Where(c => c.Asignatura == filtroAsignatura);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(term) || 
                    c.Asignatura.ToLower().Contains(term));
            }

            if (!showAll)
            {
                query = query.Where(c => !HasCompletedCuestionario(c.Id));
            }

            cuestionariosFiltrados = query.ToList();
            paginaActual = 1; // Reset página al filtrar
        }
    }
}