namespace QuizHub.Web.Extensions
{
    public static class NumberFormatExtensions
    {
        public static string ToScoreString(this double score)
        {
            if (Math.Round(score, 1) == Math.Floor(score))
                return Math.Floor(score).ToString();
            
            return score.ToString("F1").Replace(",", ".");
        }
    }
} 