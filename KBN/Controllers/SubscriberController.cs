using KBN.Models;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Security.Cryptography;

namespace KBN.Controllers
{
    public class SubscriberController : Controller
    {
        private readonly ISubscriberRepo _repo;
            private readonly IDBALiasesRepo _repoDB;
        public SubscriberController(ISubscriberRepo repo, IDBALiasesRepo repoDB) 
        {
            _repo = repo;
            _repoDB = repoDB;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetSubsTable(string username, string customer, int pageNumber = 0, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(username)) username = null;
            if (string.IsNullOrWhiteSpace(customer)) customer = null;
            if (customer=="-") customer = null;
            var data = _repo.GetAllSubscribersPaged(username, customer,pageNumber,pageSize);
            return PartialView(data);
        }
        public IActionResult AddSubscriber()
        {
            return PartialView(new SubsCustom());
        }
        [HttpPost]
        public IActionResult AddSubscriber(SubsCustom s)
        {
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var result = _repo.AddSubcriber(s,email);
            if(result.res==1&&result.res2==1)
            {
                return Json(new { success1 = true,success2=true, message = "Subscriber Added Successfully" });
            }
            if (result.res == 1 && result.res2 == 0)
            {
                return Json(new { success1 = true, success2=false,message = "Cannot add customer" });
            }
            return Json(new { success1 = false, success2 = false, message = "Error" });
        }
        public IActionResult UpdateSubscriber(int id)
        {
            var s = _repo.GetById(id);
            return PartialView(s);
        }
        [HttpPost]
        public IActionResult UpdateSubscriber(SubsCustom s)
        {
            var old = _repo.GetById(s.id);
            
            var result = _repo.UpdateSubcriber(s);
            Console.WriteLine(result.res);
            Console.WriteLine(result.result);
            if (result.res == 1 && result.result == "New Customer Inserted")
            {
                var change = _repoDB.updateBySubscriber(old.username, s.username);
                return Json(new { message = "Subscriber Updated Successfully" });
            }
            if (result.res == 1 && result.result == "Customer Can't be Updated")
            {
                var change = _repoDB.updateBySubscriber(old.username, s.username);
                return Json(new {message = "Cannot Update customer" });
            }
            if (result.res == 1 && result.result == "Customer Updated Successfully")
            {
                var change = _repoDB.updateBySubscriber(old.username, s.username);
                return Json(new { message = "Update Done" });
            }
            if (result.res == 0 && result.result == "No Changes")
            {
                return Json(new { message = "Subscriber is not unique!" });
            }
            if (result.res == 1 && result.result == "No Changes")
            {
                var change = _repoDB.updateBySubscriber(old.username, s.username);
                return Json(new {  message = "Subs Updated!" });
            }
            return Json(new {  message = "Error" });
        }
        public IActionResult DeleteSubscriber()
        {
            return PartialView();
        }
        [HttpDelete]
        public IActionResult DeleteSubscriber(int id)
        {
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var res = _repo.DeleteSubscriber(id,email);
            return Json(new { success = true });
        }
    }
}
