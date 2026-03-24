namespace QuizGame.Repositories.Interfaces
{
    public interface IRepository<TModel>
    {
        IQueryable<TModel> GetAll();
        TModel? Get(int id);
        void Add(TModel model);
        void Update(TModel model);
        void Delete(int id);
        void Save();
    }
}
