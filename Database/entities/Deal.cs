using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.entities;

[Table("deals")]
public class Deal
{
    [Column("product_id")]
    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public Product Product { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }

    [Required]
    [Column("amount")]
    public int Amount { get; set; }

    [Column("deal_date")]
    public DateOnly Date { get; set; }

    public override string ToString()
    {
        return $"{ProductId} | {CustomerId} | {Amount} | {Date}";
    }
}
