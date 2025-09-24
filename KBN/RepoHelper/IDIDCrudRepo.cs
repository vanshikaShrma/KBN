using KBN.Models;

namespace KBN.RepoHelper
{
    public interface IDIDCrudRepo
    {
        public List<long> GetRandomNumbers(long start, long end);
        public List<DIDCrud> GetAllDIDs(string did=null, string city=null, string country=null);
        public DIDViewModel GetAllDIDsPaged(string did = null, string city = null, string country = null, int pageNumber = 0, int pageSize = 10);
        public DIDCrud GetDIDById(int id);
        public List<long> GetInvalidDids(List<long> list);
        public int AddDIDs(List<DIDCrud> data,string userEmail);
        public int UpdateDID(DIDCrud data);
        public int DeleteDID(int id);
    }
}
