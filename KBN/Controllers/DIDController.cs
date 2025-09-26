using KBN.Models;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Packaging.Signing;
using System.Drawing;
using System.Net;

namespace KBN.Controllers
{
    public class DIDController : Controller
    {
        private readonly IDIDCrudRepo _repo;
        private readonly IDIDSubsMappingRepo _maprepo;
        private readonly ISubscriberRepo _subscriberRepo;
        private readonly IDBALiasesRepo _repoDB;
        public DIDController(IDIDCrudRepo repo, IDIDSubsMappingRepo maprepo, ISubscriberRepo subscriberRepo,  IDBALiasesRepo repoDB)
        {
            _repo = repo;
            _maprepo = maprepo;
            _subscriberRepo = subscriberRepo;
            _repoDB = repoDB;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetAddDIDPartial()
        {
            return PartialView();
        }
        public IActionResult GetRandomDIDs(long start, long end)
        {
            var data = _repo.GetRandomNumbers(start, end);

            return Json(new { success = true, dids = data });
        }
        [HttpPost]
        public IActionResult GetDIDModalTable([FromBody] List<long> list)
        {
            var invalidDIDs = _repo.GetInvalidDids(list);
            ViewBag.InvalidCount = invalidDIDs.Count;
            ViewBag.InvalidDIDs = invalidDIDs;
            return PartialView(list);
        }
        public IActionResult GetDIDTable(string did, string city, string country, int pageNumber = 0, int pageSize = 10)
        {

            if (string.IsNullOrWhiteSpace(did)) did = null;
            if (string.IsNullOrWhiteSpace(city)) city = null;
            if (string.IsNullOrWhiteSpace(country)) country = null;

            var model = _repo.GetAllDIDsPaged(did, city, country, pageNumber, pageSize);
            return PartialView(model);
        }
        [HttpPost]
        public IActionResult AddDIDs([FromBody] List<DIDCrud> data)
        {
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            Console.WriteLine("User Email: " + email);


            var res = _repo.AddDIDs(data, email);
            if (res == 0)
            {
                return Json(new { success = false, message = "DIDs are not Valid!" });
            }
            return Json(new { success = true, message = $"{res} DID Added Successfully" });
        }
        public IActionResult UpdateDID(int id)
        {
            var data = _repo.GetDIDById(id);
            return PartialView(data);
        }
        [HttpPost]
        public IActionResult UpdateDID(DIDCrud data)
        {
            var old = _repo.GetDIDById(data.id);
            var res = _repo.UpdateDID(data);
            if (res == 0)
            {
                return Json(new { success = false, message = "Invalid DID" });
            }
          
            var change = _repoDB.updateByDID(old.did, data.did);
            return Json(new { success = true, message = "Updated DID" });
        }
        [HttpDelete]
        public IActionResult DeleteDID(int id)
        {
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var res = _repo.DeleteDID(id,email);
            return Json(new { success = true, message = "DID deleted Successfully!" });
        }
        public IActionResult DeleteDID()
        {
            return PartialView();
        }
        public IActionResult LinkDID()
        {
            var subscribers = _subscriberRepo.GetAllSubscribers();

            List<SelectListItem> subscriberList = subscribers
                .Select(s => new SelectListItem
                {
                    Text = s.username,
                    Value = s.username
                })
                .ToList();

            // Pass to view
            ViewBag.SubscriberList = subscriberList;
            return PartialView();
        }
        [HttpPost]
        public IActionResult LinkDID(string SelectedSubscriber)
        {
            var id = RouteData.Values["id"]?.ToString();
            int Id = Convert.ToInt32(id);
            Console.WriteLine($"ID mila: {id}");
            var item = _repo.GetLinkedStatus(Id);
            if(item.Linked==0)
            {
                return Json(new { message = "Already Linked" });
            }
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            Console.WriteLine($"Subscriber to aa gya {SelectedSubscriber}");
            var subs_id = _subscriberRepo.GetIdByName(SelectedSubscriber);
            Console.WriteLine($"Subs id is {subs_id}");
            var res = _maprepo.AddMapping(Id, subs_id,email);
            if(res==0)
            {
                return Json(new { message = "Not Done" });
            }
            return Json(new { message = "Done" });
        }
    }
}
