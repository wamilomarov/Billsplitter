using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Billsplitter.Models
{
    public class Pagination<T>
    {
        private IQueryable<T> _query;
        
        private int _maxPageSize = 20;

        private int _pageNumber { get; set; } = 1;

        private int _pageSize { get; set; } = 10;

        private int pageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > _maxPageSize) ? _maxPageSize : value; }
        }

        public List<T> data;
        public JObject pagination;

        private int _totalCount;
        private int _totalPagesCount;

        public Pagination(IQueryable<T> query, int page, int size)
        {
            _query = query;
            _pageNumber = page;
            _pageSize = size;
            _totalCount = query.Count();
            _totalPagesCount = (int) Math.Ceiling(_totalCount / (double) _pageSize);
        }


        public JObject GetPagination()
        {
            data = _query.Skip((_pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var result = new JObject();
            pagination = new JObject();
            pagination["hasNextPage"] = _totalPagesCount > (_pageNumber + 1);
            pagination["hasPrevPage"] = _pageNumber > 1;
            pagination["nextPageNumber"] = _totalPagesCount > (_pageNumber + 1) ? _pageNumber + 1 : 0;
            pagination["prevPageNumber"] = _pageNumber > 1 ? _pageNumber - 1 : 0;
            result["data"] = JToken.FromObject(data);
            result["pagination"] = pagination;
            return result;
        }
    }
}