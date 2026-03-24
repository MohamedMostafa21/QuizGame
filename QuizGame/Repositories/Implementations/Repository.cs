using QuizGame.Data;
using QuizGame.Repositories.Interfaces;

namespace QuizGame.Repositories.Implementations
{
    public class Repository<TModel> : IRepository<TModel> where TModel : class
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(TModel model)
        {
            _context.Set<TModel>().Add(model);
        }

        public void Delete(int id)
        {
            TModel? model = Get(id);

            if(model is not null)
                _context.Set<TModel>().Remove(model);
        }

        public TModel? Get(int id)
        {
            return _context.Set<TModel>().Find(id);
        }

        public IQueryable<TModel> GetAll()
        {
            return _context.Set<TModel>();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void Update(TModel model)
        {
            _context.Set<TModel>().Update(model);
        }
    }
}
