namespace faturamento_api.Models;

public class NotaFiscal
{
    public int Id { get; set; }

    public int Numero { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public DateTime Data { get; set; }

    public string Status { get; set; } = "Aberta";

    public List<ItemNota> Itens { get; set; } = new();
}