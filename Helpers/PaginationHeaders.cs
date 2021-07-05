namespace ZawagProject.Helpers
{
    public class PaginationHeaders
    {
       
        public int CurrentPage { get; set; }

        public int ItemsPerPage { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }
         public PaginationHeaders(int currnetPage,int itemsPerPage,int totalItems,int totalPages)
        {
          this.CurrentPage  =currnetPage;
          this.ItemsPerPage=itemsPerPage;
          this.TotalItems=totalItems;
          this.TotalPages=totalPages;

        }
        
    }
}