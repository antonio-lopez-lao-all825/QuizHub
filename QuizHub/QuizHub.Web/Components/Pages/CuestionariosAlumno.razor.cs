using Microsoft.EntityFrameworkCore;
using QuizHub.Web.Data;


namespace QuizHub.Web.Components.Pages
{
    public partial class CuestionariosAlumno
    {

        private List<Cuestionario> cuestionarios;
        protected override async Task OnInitializedAsync()
        {
            //Llamada a la BD
            cuestionarios = await DbContext.cuestionarios.ToListAsync();

            // Simulación de datos.
           // cuestionarios = await ObtenerCuestionarios();
        }
        /*
        private Task<List<Cuestionario>> ObtenerCuestionarios()
        {
            // Simulacion de datos para la vista
            return Task.FromResult(new List<Cuestionario>
        {
            new Cuestionario { Id = 1, Name = "Cuestionario 1", Asignatura = "Matemáticas", Estado = 1 },
            new Cuestionario { Id = 2, Name = "Cuestionario 2", Asignatura = "Historia", Estado = 0 },
            new Cuestionario { Id = 3, Name = "Cuestionario 3", Asignatura = "Ciencias", Estado = 1 }
        });
        }*/

        private void NavegarACuestionario(int id)
        {
            NavigationManager.NavigateTo($"/detalleCuestionario/{id}");
        }
    }
}
