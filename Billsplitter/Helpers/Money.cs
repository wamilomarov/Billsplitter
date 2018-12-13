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

            var purchases = _context.Purchases
                .Where(p => p.PaidByUserId != _userId &&
                            p.GroupId == _groupId &&
                            p.PaidByUserId != null &&
                            p.IsComplete == true &&
                            p.PurchaseMembers.Any(pm => pm.UserId == _userId))
                .GroupBy(g => g.PaidByUserId)
                .Select(i => new GroupMoney(){UserId = i.Key.Value, Value = i.Sum(sum => sum.Price / sum.PurchaseMembers.Count())})
                .ToList();

            var transactions = _context.Transactions
                .Where(t => t.GroupId == _groupId &&
                            t.PayerId == _userId &&
                            t.Group.GroupsUsers.Any(gu => gu.UserId == t.ReceiverId))
                .GroupBy(g => g.ReceiverId)
                .Select(i => new GroupMoney() {UserId = i.Key, Value = i.Sum(sum => sum.Amount)})
                .ToList();
            
            var groupMoney = new List<GroupMoney>();

            foreach (var user in _groupUsers)
            {
                var item = new GroupMoney(){UserId = user.UserId, Value = 0};
                
                if (purchases.Exists(p => p.UserId == user.UserId))
                {
                    item.Value += purchases.FirstOrDefault(p => p.UserId == item.UserId).Value;
                }
                
                if (transactions.Exists(p => p.UserId == user.UserId))
                {
                    item.Value -= transactions.FirstOrDefault(t => t.UserId == item.UserId).Value;
                }

                groupMoney.Add(item);
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
                    Value = owe - transactions
                };
                
                groupMoney.Add(owes);

            }

            return groupMoney;
        }

        
    }
}