using System.Numerics;

namespace KBN.Models
{
    public class DIDSubsLink
    {
        public int id {  get; set; }
        public long did {  get; set; }
        public string subscriber { get; set; }

        public DateTime linked_at { get; set; }

        public string linked_by { get; set; }

    }
}
