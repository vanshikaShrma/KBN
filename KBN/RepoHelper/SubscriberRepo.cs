using Dapper;
using KBN.Models;
using KBN.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Protocol;
using System.Data;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace KBN.RepoHelper
{
    public class SubscriberRepo : ISubscriberRepo
    {
        private readonly IDbConnection _conn;
        private readonly IAesEncryption _en;
        public SubscriberRepo(IDbConnection conn, IAesEncryption en)
        {
            _conn = conn;
            _en = en;
        }
        public int GetIdByName(string username)
        {
            string sql = "Select id from dbo.Subscriber where username=@username";
            var res = _conn.ExecuteScalar<int>(sql, new {username});
            return res;
        }

        public (int res, int res2) AddSubcriber(SubsCustom s, string email)
        {
            int res;
            if (checkSubsPresence(s.username) == 0)
            {
                string sql1 = @"Insert INTO dbo.Subscriber(username,
                            password,recorded_at,recorded_by)
                            Values(
                            @username,@password,@recorded_at,@recorded_by)";
                 s.recorded_at = DateTime.Now;
                s.recorded_by = email;
                s.password = _en.Encrypt(s.password);
                Console.WriteLine($"Encrypted Password is: {s.password}");
                res = _conn.Execute(sql1, new { s.username, s.password, s.recorded_at, s.recorded_by });
            }
            else
            {
                return (0, 0);
            }
            string sqlId = "Select id from dbo.Subscriber where username=@username ";
            int subsid = _conn.ExecuteScalar<int>(sqlId, new { s.username });
            int res2 = AddCustomer(s, subsid);
            return (res, res2);
        }
        public SubsCustom GetById(int id)
        {
            string sql = @"Select s.*,c.name AS customer_name from dbo.Subscriber s 
                           left join dbo.CustomerData c 
                           on s.id=c.sub1 OR s.Id=c.sub2
                           where is_void=@is_void AND s.id=@id;";
            var data = _conn.QuerySingleOrDefault<SubsCustom>(sql, new { is_void = false, id = id });
            data.password = _en.Decrypt(data.password);
            Console.WriteLine($"password sending in update is: {data.password}");
            return data;
        }

        public (int res, string result) UpdateSubcriber(SubsCustom s)
        {
            Console.WriteLine($"Updated Password came is: {s.password}");
            s.password = _en.Encrypt(s.password);
            
            int res = 0;
            if(checkSubsPresence(s.username,s.id)==0)
            {
                string sql = @"Update dbo.Subscriber
                           SET username=@username, password=@password where id=@id";
                res = _conn.Execute(sql, new { s.username, s.password, s.id });
               
            }
            
            string result = UpdateCustomer(s, s.id);
            return (res, result);
        }

        public int DeleteSubscriber(int id, string email)
        {
            
            string sql = @"UPDATE dbo.Subscriber
                   SET is_void=@is_void, voided_at=@voided_at
                   WHERE id=@id";
            var voided_at = DateTime.Now;
            var is_void = true;
            var res = _conn.Execute(sql, new { is_void, voided_at, id });

            var subscriber = GetById(id);

            //removing mapping
            string sqlRemove = @"Update dbo.DIDSubscriberMapping
                            SET is_void=@is_void,
                            voided_at=@voided_at,
                            voided_by=@voided_by
                             Where subs_id=@id";

            
            var voided_by = email;
            
            var resRemove = _conn.Execute(sqlRemove, new { is_void, voided_at, voided_by, id });

            //remove from DBAliases
            string sqlRemoveDBA = @"Update dbo.DBAliases
                            SET is_void=@is_void,
                            voided_at=@voided_at,
                             Where username=@username";




            var resRemoveDBA = _conn.Execute(sqlRemoveDBA, new { is_void = true, voided_at, username = subscriber.username });


            string sqlGetC = "SELECT * FROM dbo.CustomerData WHERE sub1=@id OR sub2=@id";
            var c = _conn.QuerySingleOrDefault(sqlGetC, new { id });

            if (c != null)
            { 

                bool updated = false;

                
                if (c.sub1 == id)
                {
                    c.sub1 = null; 
                    updated = true;
                }
                
                if (c.sub2 == id)
                {
                    c.sub2 = null; 
                    updated = true;
                }

                if (c.sub1 == null && c.sub2 != null)
                {
                    c.sub1 = c.sub2;
                    c.sub2 = null;
                    updated = true;
                }

                if (updated)
                {
                    string sqlUpdate = @"UPDATE dbo.CustomerData
                                 SET sub1=@sub1, sub2=@sub2
                                 WHERE id=@id";
                    _conn.Execute(sqlUpdate, new { sub1 = c.sub1, sub2 = c.sub2, id = c.id });
                }
            }

            return res;
        }

        public int CountAllSubscribers(string username = null, string customer = null)
        {
            string sql = @"Select COUNT(*) from dbo.Subscriber s left join dbo.CustomerData c
                          on s.id=c.sub1 OR s.Id=c.sub2 
                          where (is_void=@is_void)
                          AND (@username IS NULL OR s.username=@username)
                           AND (@customer IS NULL OR c.name=@customer);";

            var data = _conn.ExecuteScalar<int>(sql, new { is_void = false,username,customer });
            return data;
        }
        public List<Subscriber> GetAllSubscribers()
        {
            string sql = @"SELECT * FROM dbo.Subscriber s
                           WHERE s.id NOT IN (SELECT d.subs_id FROM dbo.DIDSubscriberMapping d WHERE subs_id IS NOT NULL And d.is_void=@is_void)
                            AND s.is_void=@is_void;";

            var data = _conn.Query<Subscriber>(sql, new { is_void = false}).ToList();
            return data;
        }
        public SubscriberViewModel GetAllSubscribersPaged(string username = null, string customer = null, int pageNumber = 0, int pageSize = 10)
        {
            string sql = @"Select s.*,c.name AS customer_name from dbo.Subscriber s left join dbo.CustomerData c
                          on s.id=c.sub1 OR s.Id=c.sub2 
                          where (is_void=@is_void)
                          AND (@username IS NULL OR s.username=@username)
                           AND (@customer IS NULL OR c.name=@customer)
                            Order By s.username
                            OFFSET @Skip ROWS Fetch NEXT @PageSize ROWS ONlY;";
            var Skip = pageNumber * pageSize;

            var data = _conn.Query<SubsCustom>(sql, new { is_void = false, username, customer,Skip,PageSize=pageSize }).ToList();
            foreach(var d in data)
            {
                d.password = _en.Decrypt(d.password);
                Console.WriteLine($"Password of user: {d.username} is : {d.password}");
            }
            return new SubscriberViewModel
            {
                Subscribers = data,
                TotalRows= CountAllSubscribers(username,customer)
            };
        }

        public int checkCustomerPresence(string name)
        {
            string sql = "Select COUNT(*) from dbo.CustomerData where name=@name";
            var res = _conn.ExecuteScalar<int>(sql, new { name });
            return res;
        }

        public int checkSubsPresence(string username , int? id=null)
        {
            string sql = "Select COUNT(*) from dbo.Subscriber where username=@username AND (@id is NULL OR id!=@id)";
            var res = _conn.ExecuteScalar<int>(sql, new { username,id });
            return res;
        }

        public int checkCustomerSub1(string name)
        {
            string sql = "Select COUNT(*) from dbo.CustomerData where name=@name AND sub1 IS NOT NULL";
            var res = _conn.ExecuteScalar<int>(sql, new { name });
            return res;
        }

        public int checkCustomerSub2(string name)
        {
            string sql = "Select COUNT(*) from dbo.CustomerData where name=@name AND sub2 IS NOT NULL";
            var res = _conn.ExecuteScalar<int>(sql, new { name });
            return res;
        }

        public int AddCustomer(SubsCustom s, int subsid)
        {
            string sql2;
            int res2;
            if (checkCustomerPresence(s.customer_name) == 0)
            {
                sql2 = @"Insert INTO dbo.CustomerData(name,sub1)
                         Values(@name,@sub1)";
                var res1 = _conn.Execute(sql2, new { name = s.customer_name, sub1 = subsid });
                res2 = res1;
            }
            else if (checkCustomerSub1(s.customer_name) == 0)
            {
                sql2 = @"Update dbo.CustomerData
                         SET sub1=@sub1 where name=@name";
                var res1 = _conn.Execute(sql2, new { name = s.customer_name, sub1 = subsid });
                res2 = res1;
            }
            else if (checkCustomerSub2(s.customer_name) == 0)
            {
                sql2 = @"Update dbo.CustomerData
                         SET sub2=@sub2 where name=@name";
                var res1 = _conn.Execute(sql2, new { name = s.customer_name, sub2 = subsid });
                res2 = res1;
            }
            else
            {
                res2 = 0;
            }
            return res2;
        }
        public string UpdateCustomer(SubsCustom s, int subsid)
        {
            string result="";
            int res2;
            if (checkCustomerPresence(s.customer_name) == 0)
            {
                string sqlClear = @"
                                UPDATE dbo.CustomerData
                                SET sub1 = NULL
                                WHERE sub1 = @subsid;

                                UPDATE dbo.CustomerData
                                SET sub2 = NULL
                                WHERE sub2 = @subsid;";
                _conn.Execute(sqlClear, new { subsid });

               string sql2 = @"Insert INTO dbo.CustomerData(name,sub1)
                         Values(@name,@sub1)";
                var res1 = _conn.Execute(sql2, new { name = s.customer_name, sub1 = subsid });
                res2 = res1;
                if(res2==1)
                {
                    result = "New Customer Inserted";
                }
            }
            else if (checkCustomerSub1(s.customer_name) == 0)
            {
                string sqlClear = @"
                                UPDATE dbo.CustomerData
                                SET sub1 = NULL
                                WHERE sub1 = @subsid AND name <> @name;

                                UPDATE dbo.CustomerData
                                SET sub2 = NULL
                                WHERE sub2 = @subsid AND name <> @name;";
                _conn.Execute(sqlClear, new { name = s.customer_name, subsid });

                
                string sqlSet = @"UPDATE dbo.CustomerData
                  SET sub1 = @subsid
                  WHERE name = @name
                  AND (sub2 IS NULL OR sub2 <> @subsid);";
                var res1 = _conn.Execute(sqlSet, new { name = s.customer_name, subsid });
                res2 = res1;
                if(res2==1)
                {
                    result = "Customer Updated Successfully";
                }
                else
                {
                    result = "No Changes";
                }
            }
            else if (checkCustomerSub2(s.customer_name) == 0)
            {
                string sqlClear = @"
                                UPDATE dbo.CustomerData
                                SET sub1 = NULL
                                WHERE sub1 = @subsid AND name <> @name;

                                UPDATE dbo.CustomerData
                                SET sub2 = NULL
                                WHERE sub2 = @subsid AND name <> @name;";
                _conn.Execute(sqlClear, new { name = s.customer_name,subsid });


                string sqlSet = @"UPDATE dbo.CustomerData
                  SET sub2 = @subsid
                  WHERE name = @name
                  AND (sub1 IS NULL OR sub1 <> @subsid);";
                var res1 = _conn.Execute(sqlSet, new { name = s.customer_name, subsid });
                res2 = res1;
                if (res2 == 1)
                {
                    result = "Customer Updated Successfully";
                }
                else
                {
                    result = "No Changes";
                }
            }
            else
            {
                string sql = "Select Count(*) from dbo.CustomerData where name=@name And (sub1=@subsid or sub2=@subsid)";
                int res = _conn.ExecuteScalar<int>(sql, new { name = s.customer_name, subsid });
                if(res>0)
                {
                    result = "No Changes";
                }
                else
                {
                    result = "Customer Can't be Updated";
                }
            }

            return result;
        }
    }
}
