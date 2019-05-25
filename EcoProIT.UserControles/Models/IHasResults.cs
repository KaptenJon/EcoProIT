using EcoProIT.DataLayer;

namespace EcoProIT.UserControles.Models
{
    public interface IHasResults
    {
        IResults Result { get; }
        bool ShowResults { get; set; }
    }
}