using KBN.Models;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Packaging.Signing;
using System.Net;

namespace KBN.Controllers
{
    public class DIDController : Controller
    {
        private readonly IDIDCrudRepo _repo;
        public DIDController(IDIDCrudRepo repo) 
        {
            _repo = repo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetAddDIDPartial()
        {
            return PartialView();
        }
        public IActionResult GetRandomDIDs(long start , long end)
        {
            var data = _repo.GetRandomNumbers(start, end);
            
            return Json(new { success = true , dids=data});
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
            
            var model = _repo.GetAllDIDsPaged(did,city,country,pageNumber,pageSize);
            return PartialView(model);
        }
        [HttpPost]
        public IActionResult AddDIDs([FromBody]List<DIDCrud> data)
        {
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            Console.WriteLine("User Email: " + email);


            var res = _repo.AddDIDs(data,email);
            if(res==0)
            {
                return Json(new { success = false, message = "DIDs are not Valid!" });
            }
            return Json(new { success = true, message = $"{res } DID Added Successfully" });
        }
        public IActionResult UpdateDID(int id)
        {
            var data = _repo.GetDIDById(id);
            return PartialView(data);
        }
        [HttpPost]
        public IActionResult UpdateDID(DIDCrud data)
        {
            var res = _repo.UpdateDID(data);
            if (res == 0)
            {
                return Json(new { success = false, message = "Invalid DID" });
            }
            return Json(new { success = true, message = "Updated DID" });
        }
        [HttpDelete]
        public IActionResult DeleteDID(int id)
        {
            var res = _repo.DeleteDID(id);
            return Json(new { success = true, message = "DID deleted Successfully!" });
        }
        public IActionResult DeleteDID()
        {
            return PartialView();
        }
    }
}
