namespace restaurant_demo_website.Models    
{
    public class Stocks
    {
         public int Id { get; set; }
        public int ProductID { get; set; }
        public virtual Product Product { get; set; }

        public int InitialUnits {get;set;}

        public DateTime PrepDate {get;set;}

        public int RemainingUnits {get;set;}

        public DateTime StockedDate {get;set;}

        public bool IsExpired { get; set; }
        public bool HasWaste { get; set; }
        public bool IsDeleted { get; set; }

        public Guid ApplicationUserID { get; set; }
        public int SoldUnits { get; internal set; }
    }
}