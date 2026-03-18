using Microsoft.EntityFrameworkCore;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Data
{
    // 繼承自 EF Core 的 DbContext
    public class ApplicationDbContext : DbContext
    {
        // 建構子：接收外部傳進來的資料庫連線設定
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 宣告 DbSet：這代表我們要在資料庫裡建立一個名為 AssetCategories 的資料表
        // 裡面的欄位就是依照我們剛才寫的 AssetCategory 藍圖來產生
        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<SubAssetCategory> SubAssetCategories { get; set; }
        public DbSet<AssetUnit> AssetUnits { get; set; }
    }
}
