using KBN.Models;

namespace KBN.RepoHelper
{
    public interface IDIDSubsMappingRepo
    {
        public int AddMapping(int did_id, int subs_id,string email);

        public int RemoveMapping(int id, string email);
        public int GetAllLinkedDIDsCount(string did = null, string subscriber = null, string linked_by = null);
        public DIDSubsLinkViewModel GetAllLinkedDIDs(string did = null, string subscriber = null, string linked_by = null, int pageNumber = 0, int pageSize = 10);
    }
}
