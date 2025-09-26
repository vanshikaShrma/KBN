using System.ComponentModel.DataAnnotations;

namespace KBN.Models
{
    public class DBAliases
    {
        public int id {  get; set; }
      
        public string username { get; set; }

        public string alias_username { get; set; }
        public string recorded_by { get; set; }
        public DateTime recorded_at { get; set; }
    }
}
