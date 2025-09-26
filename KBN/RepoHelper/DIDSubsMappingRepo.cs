using Dapper;
using KBN.Models;
using System.Data;

namespace KBN.RepoHelper
{
    public class DIDSubsMappingRepo:IDIDSubsMappingRepo
    {
        private readonly IDbConnection _conn;
        private readonly IDIDCrudRepo _didrepo;
        private readonly ISubscriberRepo _subsrepo;
        
        public DIDSubsMappingRepo(IDbConnection conn, IDIDCrudRepo didrepo, ISubscriberRepo subsrepo)
        {
            _conn = conn;
            _didrepo = didrepo;
            _subsrepo = subsrepo;
        }
        public int AddMapping(int did_id, int subs_id, string email)
        {
            var linked_at = DateTime.Now;
            var DID = _didrepo.GetDIDById(did_id);
            var Subscriber = _subsrepo.GetById(subs_id);

            string insertSql = @"INSERT INTO dbo.DIDSubscriberMapping (did_id, subs_id, linked_at, linked_by)
                             VALUES (@did_id, @subs_id, @linked_at, @linked_by)";
            var res = _conn.Execute(insertSql, new { did_id, subs_id, linked_at, linked_by = email });

            

            var sqlDBALiases = @"Insert into dbo.DBALiases(
                                    username,alias_username,recorded_at,recorded_by)
                                    Values(@subscriber,@did,@recorded_at,@recorded_by)";
            var res1 = _conn.Execute(sqlDBALiases, new { subscriber = Subscriber.username, did = DID.did, recorded_at = linked_at, recorded_by = email });


            return res;
        }
        public int GetAllLinkedDIDsCount(string did = null, string subscriber = null, string linked_by = null)
        {
            long? Did = string.IsNullOrEmpty(did) ? (long?)null : Convert.ToInt64(did);
            string sql = @"Select COUNT(*) from dbo.DIDSubscriberMapping ds inner join dbo.DIDCrud d on ds.did_id=d.id inner join dbo.Subscriber s on ds.subs_id= s.id 
                            where ds.is_void=@is_void
                            AND (@did IS NULL OR d.did=@did)
                            AND (@subscriber IS NULL OR s.username=@subscriber)
                            AND (@linked_by IS NULL OR ds.linked_by=@linked_by);";
            var count = _conn.ExecuteScalar<int>(sql, new { is_void = false, did = Did, subscriber = subscriber, linked_by = linked_by });
            

            return count;
        }

        public DIDSubsLinkViewModel GetAllLinkedDIDs(string did=null,string subscriber=null,string linked_by=null, int pageNumber = 0, int pageSize = 10)
        {
            long? Did = string.IsNullOrEmpty(did) ? (long?)null : Convert.ToInt64(did);
            int Skip = pageNumber * pageSize;
            string sql = @"Select ds.*,d.did,s.username AS subscriber from dbo.DIDSubscriberMapping ds inner join dbo.DIDCrud d on ds.did_id=d.id inner join dbo.Subscriber s on ds.subs_id= s.id 
                            where ds.is_void=@is_void
                            AND (@did IS NULL OR d.did=@did)
                            AND (@subscriber IS NULL OR s.username=@subscriber)
                            AND (@linked_by IS NULL OR ds.linked_by=@linked_by)
                            Order By ds.id
                            OFFSET @Skip Rows Fetch Next @PageSize ROWS ONLY;";
            var data = _conn.Query<DIDSubsLink>(sql, new { is_void = false, did=Did ,subscriber=subscriber,linked_by=linked_by,Skip,PageSize=pageSize }).ToList();
            var model = new DIDSubsLinkViewModel
            {
                dIDSubsLinks = data,
                TotalRows = GetAllLinkedDIDsCount(),
            };

            return model;
        }
        public DIDSubscriberMapping GetById(int id)
        {
            var sql = "Select * from dbo.DIDSubscriberMapping where id=@id AND is_void=@is_void";
            var res = _conn.QuerySingleOrDefault<DIDSubscriberMapping>(sql, new { id, is_void = false });
            Console.WriteLine("Request Done");
            Console.WriteLine($"Response id is : {res.did_id}");
            return res;
        }
        public int RemoveMapping(int id, string email)
        {
            var row = GetById(id);
            string sql = @"Update dbo.DIDSubscriberMapping
                            SET is_void=@is_void,
                            voided_at=@voided_at,
                            voided_by=@voided_by
                             Where id=@id";

            var voided_at = DateTime.Now;
            var voided_by = email;
            var is_void = true;
            var res = _conn.Execute(sql, new { is_void, voided_at, voided_by, id });


            Console.WriteLine(row.did_id);

            var DID = _didrepo.GetDIDById(row.did_id);

            //remove from DBAliases
            string sqlRemoveDBA = @"Update dbo.DBAliases
                            SET is_void=@is_void,
                            voided_at=@voided_at
                             Where alias_username=@username";


            var resRemoveDBA = _conn.Execute(sqlRemoveDBA, new { is_void = true, voided_at, username = DID.did });
            return res;
        }
    }
}
