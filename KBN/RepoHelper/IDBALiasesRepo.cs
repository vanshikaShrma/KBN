using KBN.Models;

namespace KBN.RepoHelper
{
    public interface IDBALiasesRepo
    {
        public DBAliasesViewModel Aliases(string alias_username=null, string username=null, string recorded_by=null, int pageNumber = 0, int pageSize = 10);
        public int updateBySubscriber(string old, string newone);
        public int updateByDID(long old, long newone);
    }
}
