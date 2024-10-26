namespace Domain.Entities
{
    public class Product
    {
        public Product(Guid id, string name, string description, decimal price, int quantity, Guid userId)
        {
            Id = id;
            Name = name;
            Description = description;
            Price = price;
            Quantity = quantity;
            UserId = userId;
        }
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
        public void Update(string name, string description, decimal price, int quantity)
        {
            Name = name;
            Description = description;
            Price = price;
            Quantity = quantity;
        }
    }
}
