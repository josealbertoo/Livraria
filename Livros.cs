using System.Text.Json.Serialization;


namespace Livraria
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Generos
    {
        Anima��o,
        Aventura,
        A��o,
        Com�dia,
        Drama,
        Fic��o,
        Mist�rio,
        Policial,
        Romance,
        Terror
    }
    public class Livros(
                      DateOnly date,
                      string titulo="",
                      string autor="",
                      Generos genero= Generos.Anima��o,
                      double preco=0,
                      double quantidade=0,
                      int id = 0) 
    {
        public int Id { get; set; } = id;
        public string Titulo { get; set; } = titulo;
        public string Autor { get; set; } = autor;
        public Generos Genero { get; set; } = genero;
        public double Preco { get; set; } = preco;
        public double Quantidade { get; set; } = quantidade;
        public DateOnly Date { get; set; } = date;

    }
}
