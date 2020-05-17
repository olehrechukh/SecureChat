namespace Chat.Server.Repositories
{
    public interface IRepository
    {
        string GetBarcode(long id);
    }

    public class DummyRepository : IRepository
    {
        public string GetBarcode(long id)
        {
            return id == 39 ? "39 Barcode name" : null;
        }
    }
}