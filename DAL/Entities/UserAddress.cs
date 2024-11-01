namespace DAL.Entities
{
    public class UserAddress
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AddressDelivery { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
