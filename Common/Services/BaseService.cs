using Common.DAL;
using Common.DAL.Interfaces;

namespace Common.Services
{
    public class BaseService
    {
        public IDepotContext Context { get; }

        public BaseService(IDepotContext context)
        {
            Context = context;
        }
    }
}
