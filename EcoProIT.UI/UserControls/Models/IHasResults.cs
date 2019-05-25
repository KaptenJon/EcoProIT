namespace EcoProIT.UI.UserControls.Models
{
    public interface IHasResults
    {
        IResults Result { get; }
        bool ShowResults { get; set; }
    }
}