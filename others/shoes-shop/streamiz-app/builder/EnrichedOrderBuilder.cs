using Streamiz.Kafka.Net.Crosscutting;

 static class EnrichedOrderBuilder {
    public static EnrichedOrder Build(CustomerOrder customerOrder, Product product){
        EnrichedOrder enrichedOrder = new EnrichedOrder();
        enrichedOrder.OrderId = customerOrder.Order.order_id;
        enrichedOrder.OrderTime = customerOrder.Order.ts.FromMilliseconds();
        enrichedOrder.CustomerId = customerOrder.Customer.id;
        enrichedOrder.CustomerEmail = customerOrder.Customer.email;
        enrichedOrder.CustomerFirstName = customerOrder.Customer.first_name;
        enrichedOrder.CustomerLastName = customerOrder.Customer.last_name;
        enrichedOrder.CustomerPhone = customerOrder.Customer.phone;
        enrichedOrder.CustomerAddress = $"{customerOrder.Customer.street_address} {customerOrder.Customer.zip_code} {customerOrder.Customer.state} - {customerOrder.Customer.country}";
        enrichedOrder.ProductId = product.id;
        enrichedOrder.ProductBrand = product.brand;
        enrichedOrder.ProductPrice = product.sale_price;
        enrichedOrder.ProductName = product.name;
        return enrichedOrder;
    }
}