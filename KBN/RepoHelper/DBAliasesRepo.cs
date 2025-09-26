using Dapper;
using KBN.Models;
using System.Data;
using System.Drawing.Printing;
using System.Security.Cryptography;

namespace KBN.RepoHelper
{
    public class DBAliasesRepo:IDBALiasesRepo
    {
        private readonly IDbConnection _conn;
        public DBAliasesRepo(IDbConnection conn)
        {
            _conn = conn;
        }
        public int CountOfAll(string alias_username = null, string username = null, string recorded_by = null)
        {
            long? AU = string.IsNullOrEmpty(alias_username) ? (long?)null : Convert.ToInt64(alias_username);
            
            string sql = @"Select COUNT(*) from dbo.DBAliases where is_void=@is_void
                            AND (@alias_username IS NULL OR alias_username=@alias_username)
                            AND (@username IS NULL OR username=@username)
                            AND (@recorded_by IS NULL OR recorded_by=@recorded_by) ";
            var res = _conn.ExecuteScalar<int>(sql, new { is_void = false, alias_username = AU, username, recorded_by });
            return res;
        }
        public DBAliasesViewModel Aliases(string alias_username = null, string username = null, string recorded_by = null, int pageNumber = 0, int pageSize = 10)
        {
            long? AU = string.IsNullOrEmpty(alias_username) ? (long?)null : Convert.ToInt64(alias_username);
            int Skip = pageSize * pageNumber;
            string sql = @"Select * from dbo.DBAliases where is_void=@is_void
                            AND (@alias_username IS NULL OR alias_username=@alias_username)
                            AND (@username IS NULL OR username=@username)
                            AND (@recorded_by IS NULL OR recorded_by=@recorded_by)
                            Order By username
                            OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY ";
            var res = _conn.Query<DBAliases>(sql,new { is_void=false,alias_username=AU,username,recorded_by,Skip,PageSize=pageSize}).ToList();
            var model = new DBAliasesViewModel
            {
                AllData = res,
                TotalRows = CountOfAll(alias_username, username, recorded_by)
            };
            return model;
        }
        public int updateBySubscriber(string old,string newone)
        {
            string sqlDBA = @"Update dbo.DBAliases SET username=@new_username where username=@old_username";
            var resDBA = _conn.Execute(sqlDBA, new { new_username = newone,old_username=old });
            return resDBA;
        }
        public int updateByDID(long old, long newone)
        {
            string sqlDBA = @"Update dbo.DBAliases SET alias_username=@newone where alias_username=@old";
            var resDBA = _conn.Execute(sqlDBA, new { newone,  old });
            return resDBA;
        }
    }
}
