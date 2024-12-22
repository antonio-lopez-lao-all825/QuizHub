namespace QuizHub.Web.Data
{
    public class Respuesta
    {
        public int? Id { get; set; }
        public int? IdPregunta { get; set; }
        public string? Texto { get; set; }
        public bool? Correcta { get; set; }
        public int? Estado { get; set; }
    }
}
