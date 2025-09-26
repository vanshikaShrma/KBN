using KBN.Models;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace KBN.Controllers
{
    public class DBAliasesController : Controller
    {
        private readonly IDBALiasesRepo _repo;
        public DBAliasesController(IDBALiasesRepo repo) 
        { 
            _repo = repo;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GetTable(string alias_username, string username, string recorded_by, int pageNumber = 0, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(alias_username)) alias_username = null;
            if (string.IsNullOrWhiteSpace(username)) username = null;
            if (string.IsNullOrWhiteSpace(recorded_by)) recorded_by = null;
            var data = _repo.Aliases(alias_username,username,recorded_by,pageNumber,pageSize);
            return PartialView(data);
        }
    }
}
