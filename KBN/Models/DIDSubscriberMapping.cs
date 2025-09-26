namespace KBN.Models
{
    public class DIDSubscriberMapping
    {
        public int id {  get; set; }
        public int did_id { get; set; }
        public int subs_id { get; set; }
        public DateTime linked_at { get; set; }
        public string linked_by { get; set; }
    }
}
