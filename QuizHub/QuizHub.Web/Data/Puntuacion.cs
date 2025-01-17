namespace QuizHub.Web.Data
{
    public class Puntuacion
    {
        public int Id { get; set; }
        public int IdCuestionario { get; set; }
        public string IdUsuario { get; set; }
        public int puntuacion { get; set; }
    } 
}
