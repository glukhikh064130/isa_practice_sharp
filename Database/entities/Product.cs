using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.entities;

[Table("products")]
public class Product
{
    [Key]
    [Column("product_id")]
    public int Id { get; set; }

    [Required]
    [Column("good")]
    public string Good { get; set; }

    [Required]
    [Column("price")]
    public double Price { get; set; }

    [Required]
    [Column("category_name", TypeName="text")]
    public string Category { get; set; }

    public List<Customer> Customers { get; set; } = new List<Customer>();
    public List<Deal> Deals { get; set; } = new List<Deal>();

    public override string ToString()
    {
        return $"{Id} | {Good} | {Price} | {Category}";
    }
}
