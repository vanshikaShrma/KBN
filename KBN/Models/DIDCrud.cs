using System.ComponentModel.DataAnnotations;

namespace KBN.Models
{
    public class DIDCrud
    {
        public int id { get; set; }
        
        public string city { get; set; }

        [Required(ErrorMessage = "Country caanot be empty")]
        public string country { get; set; }

        [Required(ErrorMessage ="DID caanot be empty")]
        public long did { get; set; }

        public bool status { get; set; }
    }
}
