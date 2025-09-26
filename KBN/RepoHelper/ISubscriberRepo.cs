using KBN.Models;

namespace KBN.RepoHelper
{
    public interface ISubscriberRepo
    {
        public (int res, int res2) AddSubcriber(SubsCustom s,string email);
        public int CountAllSubscribers(string username=null, string customer=null);
        public List<Subscriber> GetAllSubscribers();
        public SubscriberViewModel GetAllSubscribersPaged(string username = null, string customer = null, int pageNumber = 0, int pageSize = 10);
        public (int res,string result) UpdateSubcriber(SubsCustom s);
        public SubsCustom GetById(int id);
        public int DeleteSubscriber(int id,string email);
        public int GetIdByName(string username);
    }
}
