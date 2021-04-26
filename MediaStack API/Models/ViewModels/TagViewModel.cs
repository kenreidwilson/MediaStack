using System.ComponentModel.DataAnnotations;

namespace MediaStack_API.Models.ViewModels
{
    public class TagViewModel
    {
        #region Properties

        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        #endregion
    }
}