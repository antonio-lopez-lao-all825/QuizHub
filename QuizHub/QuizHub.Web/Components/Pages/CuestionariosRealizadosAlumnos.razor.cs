using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using QuizHub.Web.Extensions;
using Radzen;

namespace QuizHub.Web.Components.Pages
{
    public partial class CuestionariosRealizadosAlumnos
    {
        private List<CalificacionDetallada> puntuaciones;
        private List<CalificacionDetallada> puntuacionesFiltradas;
        private List<string> asignaturasUnicas = new();
        private string filtroAsignatura = "";
        private string searchTerm = "";
        private int paginaActual = 1;
        private const int elementosPorPagina = 3;
        private double promedioGeneral;
        private List<DataPoint> puntuacionesPorFecha;
        private List<AsignaturaPromedio> promediosPorAsignatura;

        protected override async Task OnInitializedAsync()
        {
            try 
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    puntuaciones = new List<CalificacionDetallada>();
                    return;
                }

                // Cargar todas las calificaciones con detalles
                puntuaciones = await (from p in DbContext.puntuacion
                                    join c in DbContext.cuestionarios on p.IdCuestionario equals c.Id
                                    where p.IdUsuario == userId
                                    orderby p.Id descending
                                    select new CalificacionDetallada
                                    {
                                        Id = p.Id,
                                        CuestionarioId = c.Id,
                                        CuestionarioName = c.Name,
                                        Asignatura = c.Asignatura,
                                        Puntuacion = (double)p.puntuacion,
                                        Fecha = DateTime.Now // Por ahora usamos la fecha actual
                                    }).ToListAsync();

                if (puntuaciones.Any())
                {
                    // Inicializar datos derivados
                    asignaturasUnicas = puntuaciones.Select(p => p.Asignatura).Distinct().ToList();
                    promedioGeneral = puntuaciones.Average(p => p.Puntuacion);
                    
                    // Preparar datos para gráficos
                    puntuacionesPorFecha = puntuaciones
                        .OrderBy(p => p.Fecha)
                        .Select(p => new DataPoint { 
                            Fecha = p.Fecha, 
                            Puntuacion = p.Puntuacion 
                        })
                        .ToList();

                    promediosPorAsignatura = puntuaciones
                        .GroupBy(p => p.Asignatura)
                        .Select(g => new AsignaturaPromedio 
                        { 
                            Asignatura = g.Key, 
                            Promedio = Math.Round(g.Average(p => p.Puntuacion), 1)
                        })
                        .ToList();

                    // Inicializar lista filtrada
                    puntuacionesFiltradas = puntuaciones;
                }
                else
                {
                    puntuacionesFiltradas = new List<CalificacionDetallada>();
                    puntuacionesPorFecha = new List<DataPoint>();
                    promediosPorAsignatura = new List<AsignaturaPromedio>();
                    promedioGeneral = 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos: {ex.Message}");
                puntuaciones = new List<CalificacionDetallada>();
            }
        }

        private void FiltrarCalificaciones()
        {
            var query = puntuaciones.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtroAsignatura) && filtroAsignatura != "Todas las asignaturas")
            {
                query = query.Where(p => p.Asignatura == filtroAsignatura);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p => 
                    p.CuestionarioName.ToLower().Contains(term) || 
                    p.Asignatura.ToLower().Contains(term));
            }

            puntuacionesFiltradas = query.ToList();
            paginaActual = 1; // Reset página al filtrar
        }

        private string GetScoreClass(double score)
        {
            return score switch
            {
                >= 9 => "excellent",
                >= 7 => "good",
                >= 5 => "average",
                _ => "needs-improvement"
            };
        }

        private class CalificacionDetallada
        {
            public int Id { get; set; }
            public int CuestionarioId { get; set; }
            public string CuestionarioName { get; set; }
            public string Asignatura { get; set; }
            public double Puntuacion { get; set; }
            public DateTime Fecha { get; set; }
        }

        private class DataPoint
        {
            public DateTime Fecha { get; set; }
            public double Puntuacion { get; set; }
        }

        private class AsignaturaPromedio
        {
            public string Asignatura { get; set; }
            public double Promedio { get; set; }
        }

        private async Task OnPageChanged(PagerEventArgs args)
        {
            paginaActual = args.PageIndex + 1;
            await InvokeAsync(StateHasChanged);
        }

        private IEnumerable<IGrouping<string, CalificacionDetallada>> GetPuntuacionesPaginadas()
        {
            return puntuacionesFiltradas
                .Skip((paginaActual - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .GroupBy(p => p.Asignatura);
        }
    }
}
