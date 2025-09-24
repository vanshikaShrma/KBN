using Dapper;
using KBN.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;

namespace KBN.RepoHelper
{
    public class DIDCrudRepo:IDIDCrudRepo
    {
        private readonly IDbConnection _conn;
        public DIDCrudRepo(IDbConnection conn) 
        {
            _conn = conn;
        }
        public List<long> GetRandomNumbers(long start, long end)
        {
            List<long> numbers = new List<long>();
            for (long i = start; i <= end; i++)
            {
                numbers.Add(i);
            }
            return numbers;
        }
        public List<DIDCrud> GetAllDIDs(string did=null, string city=null, string country = null)
        {
            string sql = @"Select * from dbo.DIDCrud where (is_void=@is_void)
                           AND (@DID IS NULL OR DID=@DID)
                           AND (@City IS NULL OR City=@City)
                           AND (@Country IS NULL OR Country=@Country)";
            var isvoid = false;
            var data = _conn.Query<DIDCrud>(sql, new {is_void=isvoid,DID=did,City=city,Country=country}).ToList();
            return data; 
        }
        public DIDViewModel GetAllDIDsPaged(string did = null, string city = null, string country = null, int pageNumber = 0, int pageSize = 10)
        {
            long? Did = string.IsNullOrEmpty(did) ? (long?)null : Convert.ToInt64(did);
            var parameters = new
            {
                did = Did,
                city = city,
                country = country,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            using (var multi = _conn.QueryMultiple("GetDIDSPaged", parameters, commandType: CommandType.StoredProcedure))
            {
                int totalCount = multi.Read<int>().Single();

                var DIDs = multi.Read<DIDCrud>().ToList();

                var data = new DIDViewModel
                {
                    DIDs = DIDs,
                    TotalRows = totalCount,
                };
                return data;
            }
        }
        public List<long> GetInvalidDids(List<long> list)
        {
            var invalidDIDs = list.Where(d => d.ToString().Length < 11).ToList();
            return invalidDIDs;
        }
        public DIDCrud GetDIDById(int id)
        {
            string sql = @"Select * from dbo.DIDCrud where Id=@Id And is_void=@is_void";
            var data = _conn.QuerySingleOrDefault<DIDCrud>(sql, new {Id=id,is_void=false});
            return data;
        }
        public int UpdateDID(DIDCrud data)
        {
            var validData = GetValidDIDs(new List<DIDCrud> { data },data.id);
            
            if(validData.Count==0)
            {
                return 0;
            }
            var d = validData[0];
            string sql = @"Update dbo.DIDCrud SET
                           DID=@DID , City=@City, Country=@Country where id=@Id";
            var res = _conn.Execute(sql, new { DID = d.did, City = d.city, Country = d.country, Id = d.id });
            return res;
        }
        public int DeleteDID(int id)
        {
            var time = DateTime.Now;
            string sql = "Update dbo.DIDCrud SET is_void=@is_void , voided_on=@voided_on where Id=@Id";
            var res = _conn.Execute(sql, new { is_void = true, Id = id,voided_on=time });
            return res;
        }

        public List<DIDCrud> GetValidDIDs(List<DIDCrud> data, int? currentId = null)
        {
            var validDIDs = data.Where(d => d.did.ToString().Length >= 11).ToList();
            var existingDIDs = GetAllDIDs();
            if (currentId.HasValue)
            {
                existingDIDs = existingDIDs.Where(e => e.id != currentId.Value).ToList();
            }
            var newValidDIDs = validDIDs.Where(d => !existingDIDs.Any(e => e.did==d.did)).ToList();
            return newValidDIDs;
        }

        public int AddDIDs(List<DIDCrud> data, string userEmail)
        {
            var validInsert = GetValidDIDs(data);
            var records = validInsert.Select(item => new {
                item.did,
                item.city,
                item.country,
                item.status,
                RecordedAt = DateTime.Now,
                RecordedBy = userEmail
            });

            string sql = @"Insert into dbo.DIDCrud(DID,City,Status,Country,RecordedAt,RecordedBy)
                           Values(@DID,@City,@Status,@Country ,@RecordedAt, @RecordedBy)";
            var rows = _conn.Execute(sql, records);
            return rows;
        }
    }
}
