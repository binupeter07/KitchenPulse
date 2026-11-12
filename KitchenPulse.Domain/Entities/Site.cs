namespace KitchenPulse.Domain.Entities
{
    public class Site
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public Site(string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteName))
            {
                throw new ArgumentException("Site Name cannot be blank", nameof(siteName));
            }

            Id = Guid.NewGuid();
            Name = siteName.Trim();
        }
    }
}
