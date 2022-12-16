public class EnrichedOrder
{
    public int OrderId { get; set; }
    public String ProductId {get; set;}
    public String ProductName { get; set; }
    public String ProductBrand { get; set; }
    public int ProductPrice { get; set; }
    
    public String CustomerId{get; set;}
    public DateTime OrderTime { get; set; }
    public String CustomerFirstName { get; set; }
    public String CustomerLastName { get; set; }
    public String CustomerEmail { get; set; }
    public String CustomerPhone { get; set; }
    public String CustomerAddress { get; set; }
    
}