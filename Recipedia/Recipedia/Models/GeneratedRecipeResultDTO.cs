namespace Recipedia.Models
{
    public class GeneratedRecipeResultDTO
    {
        public string Title { get; set; }
        public List<string> Ingredients { get; set; } = new();
        public List<string> Instructions { get; set; } = new();
    }
}
