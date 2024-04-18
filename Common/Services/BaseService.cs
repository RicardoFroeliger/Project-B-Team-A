using Common.DAL;

namespace Common.Services
{
    public class BaseService
    {
        public DepotContext Context { get; }

        public BaseService(DepotContext context)
        {
            Context = context;
        }
    }
}
