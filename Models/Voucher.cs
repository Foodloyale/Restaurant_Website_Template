namespace restaurant_demo_website.Models
{
      public class Voucher
    {
        public int VoucherId { get; set; }

        public string VoucherName { get; set; }
        public string Keyword {get;set;}
        public string VoucherNumber { get; set; }
        public DateTime VoucherCreationDate { get; set; }
        public decimal VoucherCreditAmount { get; set; }
        public DateTime VoucherExpirationDate { get; set; }

        public Status VoucherStatus { get; set; }
        public VoucherValueType ValueType {get;set;}
        public bool isDeleted { get; set; }
        public bool VoucherActivation { get; set; } = false;
        public int Units {get;set;}

        public int OrderID {get;set;}

        public Guid ApplicationUserId { get; set; }
    }

    public enum Status
    {
        Active,
        Expired
    }

    public enum VoucherValueType 
    {
        Percentage,
        ActualValue
    }
}