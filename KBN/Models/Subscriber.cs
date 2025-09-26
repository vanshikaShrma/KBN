using System.ComponentModel.DataAnnotations;

namespace KBN.Models
{
    public class Subscriber
    {
        public int id { get; set; }

        [Required(ErrorMessage =  "User Name Can't Be Empty")]
        [Display(Name = "UserName")]
        public string username { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        //[DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }
        public string recorded_by { get; set; }
        public DateTime recorded_at { get; set; }
    }
}
