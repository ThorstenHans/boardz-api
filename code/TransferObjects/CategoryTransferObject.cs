using BoardZ.API.Models;

namespace BoardZ.API.TransferObjects
{
    public class CategoryTransferObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int State { get; set; }
        public ModelState ModelState => (ModelState)State;

        public static CategoryTransferObject FromCategory(Category category)
        {
            return new CategoryTransferObject
            {
                Id = category.Id.ToString(),
                Name = category.Name,
                State = (int)ModelState.Clean
            };
        }
    }
}
