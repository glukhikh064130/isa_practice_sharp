using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.entities;

[Table("customers")]
public class Customer
{
    [Key]
    [Column("customer_id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    public List<Product> Products { get; set; } = new List<Product>();
    public List<Deal> Deals { get; set; } = new List<Deal>();

    public override string ToString()
    {
        return $"{Id} | {Name}";
    }
}
