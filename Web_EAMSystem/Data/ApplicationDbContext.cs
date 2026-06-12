using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Web_EAMSystem.Models;

namespace Web_EAMSystem.Data
{
    /// <summary>
    /// 系統資料庫上下文，管理與配置資料表對應，並自動攔截處理實體的稽核時間欄位
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// 初始化資料庫上下文
        /// </summary>
        /// <param name="options">資料庫連線與配置參數</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AssetCategory> AssetCategories { get; set; }
        public DbSet<SubAssetCategory> SubAssetCategories { get; set; }
        public DbSet<AssetUnit> AssetUnits { get; set; }
        public DbSet<ItemName> ItemNames { get; set; }
        public DbSet<AssetInfo> AssetInfos { get; set; }
        public DbSet<StoreRoom> StoreRooms { get; set; }
        public DbSet<StorageBin> StorageBins { get; set; }
        public DbSet<Inventory> Inventories { get; set; }

        /// <summary>
        /// 覆寫 EF Core 的同步存檔，於寫入資料庫前自動填入實體稽核時間
        /// </summary>
        /// <returns>受影響的資料筆數</returns>
        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        /// <summary>
        /// 覆寫 EF Core 的非同步存檔，於寫入資料庫前自動填入實體稽核時間
        /// </summary>
        /// <param name="cancellationToken">取消語彙</param>
        /// <returns>受影響的資料筆數任務</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 自動追蹤 ChangeTracker 中的變更，填入 CreatedDate 與 ModifiedDate
        /// </summary>
        private void ApplyAuditInfo()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            var currentTime = DateTime.Now;

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                entity.ModifiedDate = currentTime;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = currentTime;
                }
            }
        }
    }
}
