using System.Linq;

namespace Models
{
    public interface IRemoveable
    {
        public bool IsRemoved { get; set; }
    }
    public class RemoveRemovedFilter<T2> : IQuery2<IRemoveable>
    {
        public IQueryable<T2> run<T2>(IQueryable<T2> q) where T2 : IRemoveable
        {
            return q.Where(x => !x.IsRemoved);
        }
    }
}