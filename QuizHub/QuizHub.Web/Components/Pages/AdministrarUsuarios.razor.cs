using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizHub.Web.Data;
using System.Security.Claims;
using QuizHub.Web.Extensions;
using Radzen;

namespace QuizHub.Web.Components.Pages
{
    public partial class AdministrarUsuarios : ComponentBase
    {
        private List<ApplicationUser> alumnos;
        private List<ApplicationUser> alumnosFiltrados;
        private string searchTerm = "";
        private bool showDeleteDialog = false;
        private bool showGradesDialog = false;
        private ApplicationUser selectedAlumno;
        private List<CalificacionDetallada> alumnoCalificaciones = new();
        private string currentUserId;
        private List<string> asignaturasProfesor = new();
        private string filtroAsignatura = "Todas las asignaturas";
        private string searchTermCalificaciones = "";
        private List<CalificacionDetallada> calificacionesFiltradas = new();
        private int paginaActual = 1;
        private const int elementosPorPagina = 3;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            currentUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await LoadAlumnos();
        }

        private async Task LoadAlumnos()
        {
            // Obtener todos los usuarios con rol "Alumno"
            var allUsers = await UserManager.Users.ToListAsync();
            alumnos = new List<ApplicationUser>();

            foreach (var user in allUsers)
            {
                if (await UserManager.IsInRoleAsync(user, "Alumno"))
                {
                    alumnos.Add(user);
                }
            }

            alumnosFiltrados = alumnos;
        }

        private void FiltrarAlumnos()
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                alumnosFiltrados = alumnos;
            }
            else
            {
                var term = searchTerm.ToLower();
                alumnosFiltrados = alumnos
                    .Where(a => a.Email.ToLower().Contains(term))
                    .ToList();
            }
        }

        private int GetCompletedQuizzes(string alumnoId)
        {
            using var context = DbFactory.CreateDbContext();
            return context.puntuacion
                .Count(p => p.IdUsuario == alumnoId);
        }

        private double GetAverageScore(string alumnoId)
        {
            using var context = DbFactory.CreateDbContext();
            var puntuaciones = context.puntuacion
                .Where(p => p.IdUsuario == alumnoId)
                .Select(p => p.puntuacion)
                .ToList();

            return puntuaciones.Any() ? puntuaciones.Average() : 0;
        }

        private async Task VerCalificaciones(ApplicationUser alumno)
        {
            selectedAlumno = alumno;
            using var context = DbFactory.CreateDbContext();

            // Obtener asignaturas del profesor actual
            asignaturasProfesor = await context.asignaturas
                .Select(a => a.Name)
                .Distinct()
                .ToListAsync();

            // Obtener calificaciones
            var calificaciones = await (
                from p in context.puntuacion
                join c in context.cuestionarios on p.IdCuestionario equals c.Id
                where p.IdUsuario == alumno.Id
                select new CalificacionDetallada
                {
                    CuestionarioNombre = c.Name,
                    Puntuacion = p.puntuacion,
                    Asignatura = c.Asignatura
                })
                .OrderByDescending(c => c.Puntuacion)
                .ToListAsync();

            alumnoCalificaciones = calificaciones;
            calificacionesFiltradas = calificaciones;
            showGradesDialog = true;
        }

        private void FiltrarCalificaciones()
        {
            var query = alumnoCalificaciones.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtroAsignatura) && filtroAsignatura != "Todas las asignaturas")
            {
                query = query.Where(c => c.Asignatura == filtroAsignatura);
            }

            if (!string.IsNullOrWhiteSpace(searchTermCalificaciones))
            {
                var term = searchTermCalificaciones.ToLower();
                query = query.Where(c => 
                    c.CuestionarioNombre.ToLower().Contains(term) || 
                    c.Asignatura.ToLower().Contains(term));
            }

            calificacionesFiltradas = query.ToList();
            paginaActual = 1;
        }

        private IEnumerable<CalificacionDetallada> GetCalificacionesPaginadas()
        {
            return calificacionesFiltradas
                .Skip((paginaActual - 1) * elementosPorPagina)
                .Take(elementosPorPagina);
        }

        private async Task OnPageChanged(PagerEventArgs args)
        {
            paginaActual = args.PageIndex + 1;
            await InvokeAsync(StateHasChanged);
        }

        private void EliminarAlumno(ApplicationUser alumno)
        {
            selectedAlumno = alumno;
            showDeleteDialog = true;
        }

        private async Task ConfirmDelete()
        {
            if (selectedAlumno != null)
            {
                await UserManager.DeleteAsync(selectedAlumno);
                await LoadAlumnos();
            }
            showDeleteDialog = false;
        }

        private void CancelDelete()
        {
            showDeleteDialog = false;
        }

        private void CloseGradesDialog()
        {
            showGradesDialog = false;
            alumnoCalificaciones.Clear();
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
            public string CuestionarioNombre { get; set; }
            public double Puntuacion { get; set; }
            public string Asignatura { get; set; }
        }
    }
} 