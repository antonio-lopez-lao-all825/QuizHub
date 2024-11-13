namespace QuizHub.Web.Data
{
    public class Cuestionario
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Asignatura { get; set; }
        public int IdPregutasCuestionario { get; set; }
        public int Estado { get; set; }
    }
}
