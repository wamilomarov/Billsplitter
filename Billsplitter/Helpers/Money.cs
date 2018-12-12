using System;
using System.Collections.Generic;
using System.Linq;
using Billsplitter.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using MySql.Data.MySqlClient;

namespace Billsplitter.Models
{
    public class Money
    {
        private readonly billsplitterContext _context;
        private int _groupId { get; set; }
        private int _userId { get; set; }
        private List<GroupsUsers> _groupUsers { get; set; }

        public Money(billsplitterContext context, int groupId, int userId)
        {
            _context = context;
            _groupId = groupId;
            _userId = userId;
            _groupUsers = _context.GroupsUsers.Where(gu => gu.GroupId == groupId && gu.UserId != userId).ToList();
        }
        
        public List<GroupMoney> Owe()
        {
            

            const string query = @"SELECT
                        p.PaidByUserId AS UserId,
                        
                            ( COALESCE(
                                 SUM(p.Price /(SELECT COALESCE(COUNT(1), 0) 
                                                FROM purchase_members 
                                                WHERE purchase_members.PurchaseId = p.Id)
                                   ), 0
                                      ) -
                                 (SELECT COALESCE(SUM(Amount), 0) 
                                  FROM transactions 
                                  WHERE PayerId = @userId AND ReceiverId = p.PaidByUserId AND GroupId = @groupId
                                 )
                            )
                             AS `Value`
                        FROM
                            purchases p
                            INNER JOIN purchase_members pm ON pm.PurchaseId = p.Id AND pm.UserId = @userId
                        WHERE
                            p.GroupId = @groupId AND p.PaidByUSerId != @userId AND p.IsComplete = 1 AND p.PaidByUserId IS NOT NULL
                        GROUP BY
                            p.PaidByUserId";
            
            var userId = new MySqlParameter("@userId", _userId);
            var groupId = new MySqlParameter("@groupId", _groupId);

            var groupMoney = _context.GroupMoney.FromSql(query, groupId, userId).ToList();

            foreach (var user in _groupUsers)
            {
                if (!groupMoney.Exists(gm => gm.UserId == user.UserId))
                {
                    groupMoney.Add(new GroupMoney(){UserId = user.UserId, Value = 0});
                }
            }

            return groupMoney;
        }


        public List<GroupMoney> Paid()
        {
            var groupMoney = new List<GroupMoney>();
            foreach (var user in _groupUsers)
            {
                decimal owe = 0;
                var purchases = _context.Purchases
                    .Where(p => p.GroupId == _groupId &&
                                p.IsComplete == true &&
                                p.PaidByUserId == _userId &&
                                p.PurchaseMembers.Any(pm => pm.UserId == user.UserId))
                    .Include(i => i.PurchaseMembers)
                    .ToList();
                foreach (var purchase in purchases)
                {
                    owe += purchase.Price / purchase.PurchaseMembers.Count();
                }
                
                var transactions = _context.Transactions
                    .Where(p => p.GroupId == _groupId &&
                                p.PayerId == user.UserId &&
                                p.ReceiverId == _userId)
                    .GroupBy(g => g.PayerId)
                    .Select(i => i.Sum(s => s.Amount)).FirstOr(0);

                var owes = new GroupMoney()
                {
                    UserId = user.UserId,
                    Value = transactions
                };
                
                groupMoney.Add(owes);

            }

            return groupMoney;
        }

        
    }
}