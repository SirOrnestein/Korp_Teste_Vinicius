namespace faturamento_api.Models;

public class ItemNota
{
    public int Id { get; set; }

    public string CodigoProduto { get; set; } = string.Empty;

    public int Quantidade { get; set; }
}