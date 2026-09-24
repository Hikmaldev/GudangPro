namespace GudangPro.Domain.Entities;

public class Unit
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public ICollection<Item> Items { get; set; } = [];
}