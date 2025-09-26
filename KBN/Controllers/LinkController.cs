using KBN.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace KBN.Controllers
{
    public class LinkController : Controller
    {
        private readonly IDIDSubsMappingRepo _repo;
        public LinkController(IDIDSubsMappingRepo repo)
        {
            _repo = repo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetLinkedDIDstable(string did, string subscriber, string linked_by,int pageNumber = 0, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(did)) did = null;
            if (string.IsNullOrWhiteSpace(subscriber)) subscriber = null;
            if (string.IsNullOrWhiteSpace(linked_by)) linked_by = null;
            var data = _repo.GetAllLinkedDIDs(did,subscriber,linked_by,pageNumber,pageSize);
            return PartialView(data);
        }
        public IActionResult RemoveMapping()
        {
            return PartialView();
        }
        [HttpDelete]
        public IActionResult RemoveMapping(int id)
        {
            var email = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var res = _repo.RemoveMapping(id,email);
            return Json(new { message = "Done" });
        }
    }
}
