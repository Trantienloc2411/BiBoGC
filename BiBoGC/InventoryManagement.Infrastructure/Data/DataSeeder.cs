using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Data;

/// <summary>
/// Seeder class for initializing sample data in the database
/// Data represents a typical Vietnamese grocery store (Cửa hàng tạp hoá)
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(InventoryDbContext context, ILogger logger)
    {
        try
        {
            // Only seed if database is empty
            if (await context.Categories.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping seed.");
                return;
            }

            logger.LogInformation("Starting database seeding...");

            // Seed in order of dependencies
            var categories = await SeedCategoriesAsync(context, logger);
            var suppliers = await SeedSuppliersAsync(context, logger);
            var products = await SeedProductsAsync(context, categories, logger);
            var batches = await SeedProductBatchesAsync(context, products, logger);
            await SeedStockTransactionsAsync(context, products, batches, suppliers, logger);

            logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private static async Task<List<Category>> SeedCategoriesAsync(InventoryDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding categories...");

        var categories = new List<Category>();

        // ===== PARENT CATEGORIES =====
        var doUong = new Category("Đồ uống", "Các loại nước uống, nước giải khát") { DisplayOrder = 1 };
        var thucPham = new Category("Thực phẩm", "Thực phẩm đóng gói và chế biến sẵn") { DisplayOrder = 2 };
        var banhKeo = new Category("Bánh kẹo", "Các loại bánh, kẹo, snack") { DisplayOrder = 3 };
        var giaDung = new Category("Gia dụng", "Đồ dùng gia đình") { DisplayOrder = 4 };
        var veSinh = new Category("Vệ sinh cá nhân", "Sản phẩm chăm sóc cá nhân") { DisplayOrder = 5 };

        context.Categories.AddRange(doUong, thucPham, banhKeo, giaDung, veSinh);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { doUong, thucPham, banhKeo, giaDung, veSinh });

        // ===== CHILD CATEGORIES - Đồ uống =====
        var nuocNgot = new Category("Nước ngọt", "Nước ngọt có gas và không gas", doUong.Id) { DisplayOrder = 1 };
        var nuocSuoi = new Category("Nước suối", "Nước khoáng, nước tinh khiết", doUong.Id) { DisplayOrder = 2 };
        var sua = new Category("Sữa", "Sữa tươi, sữa đặc, sữa chua", doUong.Id) { DisplayOrder = 3 };
        var traCaPhe = new Category("Trà & Cà phê", "Trà, cà phê đóng gói", doUong.Id) { DisplayOrder = 4 };
        var nuocTraiCay = new Category("Nước trái cây", "Nước ép trái cây các loại", doUong.Id) { DisplayOrder = 5 };

        context.Categories.AddRange(nuocNgot, nuocSuoi, sua, traCaPhe, nuocTraiCay);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { nuocNgot, nuocSuoi, sua, traCaPhe, nuocTraiCay });

        // ===== CHILD CATEGORIES - Thực phẩm =====
        var miGoi = new Category("Mì gói", "Mì ăn liền các loại", thucPham.Id) { DisplayOrder = 1 };
        var dauAnGia = new Category("Dầu ăn & Gia vị", "Dầu ăn, nước mắm, gia vị", thucPham.Id) { DisplayOrder = 2 };
        var doHop = new Category("Đồ hộp", "Thực phẩm đóng hộp", thucPham.Id) { DisplayOrder = 3 };
        var gaoNep = new Category("Gạo & Nếp", "Gạo, nếp, bột các loại", thucPham.Id) { DisplayOrder = 4 };

        context.Categories.AddRange(miGoi, dauAnGia, doHop, gaoNep);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { miGoi, dauAnGia, doHop, gaoNep });

        // ===== CHILD CATEGORIES - Bánh kẹo =====
        var banhQuy = new Category("Bánh quy", "Bánh quy các loại", banhKeo.Id) { DisplayOrder = 1 };
        var keoMut = new Category("Kẹo & Mứt", "Kẹo, mứt các loại", banhKeo.Id) { DisplayOrder = 2 };
        var snack = new Category("Snack", "Bim bim, snack", banhKeo.Id) { DisplayOrder = 3 };

        context.Categories.AddRange(banhQuy, keoMut, snack);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { banhQuy, keoMut, snack });

        logger.LogInformation("Seeded {Count} categories.", categories.Count);
        return categories;
    }

    private static async Task<List<Supplier>> SeedSuppliersAsync(InventoryDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding suppliers...");

        var suppliers = new List<Supplier>
        {
            new Supplier("Công ty TNHH Coca-Cola Việt Nam", "Nguyễn Văn An", "0283456789", "KCN Biên Hòa, Đồng Nai"),
            new Supplier("Tập đoàn Vinamilk", "Trần Thị Bình", "0287654321", "10 Tân Trào, Quận 7, TP.HCM"),
            new Supplier("Công ty CP Acecook Việt Nam", "Lê Văn Cường", "0283334455", "KCN Tân Bình, TP.HCM"),
            new Supplier("Công ty CP Masan Consumer", "Phạm Thị Dung", "0246789012", "Số 1 Hồ Mễ Trì, Hà Nội"),
            new Supplier("Công ty TNHH Nestlé Việt Nam", "Hoàng Văn Em", "0284445566", "KCN Amata, Đồng Nai"),
            new Supplier("Công ty CP Kinh Đô", "Ngô Thị Phương", "0285556677", "141 Nguyễn Du, Quận 1, TP.HCM"),
            new Supplier("Công ty CP Unilever Việt Nam", "Đỗ Văn Giang", "0286667788", "156 Nguyễn Lương Bằng, Quận 7, TP.HCM"),
            new Supplier("Công ty TNHH La Vie", "Vũ Thị Hương", "0287778899", "KCN Long Thành, Đồng Nai"),
            new Supplier("Công ty CP Bibica", "Bùi Văn Khoa", "0288889900", "443 Lý Thường Kiệt, Quận 11, TP.HCM"),
            new Supplier("Tổng Công ty Lương thực Miền Nam", "Mai Thị Lan", "0289990011", "62 Trần Cao Vân, Quận 3, TP.HCM"),
            new Supplier("Công ty CP TH True Milk", "Trịnh Văn Minh", "0243334455", "Nghĩa Đàn, Nghệ An"),
            new Supplier("Công ty CP Tân Hiệp Phát", "Cao Thị Ngọc", "0650123456", "219 Điện Biên Phủ, Bình Định")
        };

        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} suppliers.", suppliers.Count);
        return suppliers;
    }

    private static async Task<List<Product>> SeedProductsAsync(InventoryDbContext context, List<Category> categories, ILogger logger)
    {
        logger.LogInformation("Seeding products...");

        // Get category references
        var nuocNgot = categories.First(c => c.Name == "Nước ngọt");
        var nuocSuoi = categories.First(c => c.Name == "Nước suối");
        var sua = categories.First(c => c.Name == "Sữa");
        var traCaPhe = categories.First(c => c.Name == "Trà & Cà phê");
        var miGoi = categories.First(c => c.Name == "Mì gói");
        var dauAnGia = categories.First(c => c.Name == "Dầu ăn & Gia vị");
        var banhQuy = categories.First(c => c.Name == "Bánh quy");
        var snack = categories.First(c => c.Name == "Snack");
        var gaoNep = categories.First(c => c.Name == "Gạo & Nếp");

        var products = new List<Product>();

        // ===== NƯỚC NGỌT =====
        var coca330 = new Product("Coca Cola lon 330ml", new Sku("COCA-330"), new Money(12000), "Nước ngọt có gas Coca Cola lon 330ml", ProductStatuses.Active, true);
        coca330.SetCategory(nuocNgot.Id);

        var pepsi330 = new Product("Pepsi lon 330ml", new Sku("PEPSI-330"), new Money(11000), "Nước ngọt có gas Pepsi lon 330ml", ProductStatuses.Active, true);
        pepsi330.SetCategory(nuocNgot.Id);

        var sevenUp330 = new Product("7Up lon 330ml", new Sku("7UP-330"), new Money(11000), "Nước ngọt có gas 7Up lon 330ml", ProductStatuses.Active, true);
        sevenUp330.SetCategory(nuocNgot.Id);

        var fanta330 = new Product("Fanta cam lon 330ml", new Sku("FANTA-330"), new Money(11000), "Nước ngọt có gas Fanta cam lon 330ml", ProductStatuses.Active, true);
        fanta330.SetCategory(nuocNgot.Id);

        // ===== NƯỚC SUỐI =====
        var lavie500 = new Product("Lavie 500ml", new Sku("LAVIE-500"), new Money(5000), "Nước khoáng Lavie chai 500ml", ProductStatuses.Active, true);
        lavie500.SetCategory(nuocSuoi.Id);

        var aqua500 = new Product("Aquafina 500ml", new Sku("AQUA-500"), new Money(5000), "Nước tinh khiết Aquafina chai 500ml", ProductStatuses.Active, true);
        aqua500.SetCategory(nuocSuoi.Id);

        // ===== SỮA =====
        var vinamilk180 = new Product("Sữa tươi Vinamilk 180ml", new Sku("VINA-180"), new Money(7000), "Sữa tươi tiệt trùng Vinamilk 180ml", ProductStatuses.Active, true);
        vinamilk180.SetCategory(sua.Id);

        var thTrue180 = new Product("Sữa TH True Milk 180ml", new Sku("TH-180"), new Money(7500), "Sữa tươi sạch TH True Milk 180ml", ProductStatuses.Active, true);
        thTrue180.SetCategory(sua.Id);

        var suaDacOngTho = new Product("Sữa đặc Ông Thọ 380g", new Sku("ONGTHO-380"), new Money(22000), "Sữa đặc có đường Ông Thọ lon 380g", ProductStatuses.Active, true);
        suaDacOngTho.SetCategory(sua.Id);

        // ===== TRÀ & CÀ PHÊ =====
        var nescafe = new Product("Cà phê Nescafé 3in1", new Sku("NESCAFE-3IN1"), new Money(85000), "Cà phê hòa tan Nescafé 3in1 hộp 20 gói", ProductStatuses.Active, true);
        nescafe.SetCategory(traCaPhe.Id);

        var tranhDao = new Product("Trà Ô Long không độ 500ml", new Sku("OLONG-500"), new Money(10000), "Trà Ô Long không độ chai 500ml", ProductStatuses.Active, true);
        tranhDao.SetCategory(traCaPhe.Id);

        // ===== MÌ GÓI =====
        var haoHao = new Product("Mì Hảo Hảo tôm chua cay", new Sku("HAOHAO-TCC"), new Money(4000), "Mì ăn liền Hảo Hảo vị tôm chua cay", ProductStatuses.Active, true);
        haoHao.SetCategory(miGoi.Id);

        var omachi = new Product("Mì Omachi xốt bò hầm", new Sku("OMACHI-BO"), new Money(6500), "Mì khoai tây Omachi vị xốt bò hầm", ProductStatuses.Active, true);
        omachi.SetCategory(miGoi.Id);

        var kokomi = new Product("Mì Kokomi đại gà quay", new Sku("KOKOMI-GA"), new Money(3500), "Mì ăn liền Kokomi vị gà quay", ProductStatuses.Active, true);
        kokomi.SetCategory(miGoi.Id);

        // ===== DẦU ĂN & GIA VỊ =====
        var dauNeptune = new Product("Dầu ăn Neptune 1L", new Sku("NEPTUNE-1L"), new Money(42000), "Dầu ăn Neptune chai 1 lít", ProductStatuses.Active, true);
        dauNeptune.SetCategory(dauAnGia.Id);

        var nuocMamChinSu = new Product("Nước mắm Chin-su 500ml", new Sku("CHINSU-500"), new Money(28000), "Nước mắm Chin-su chai 500ml", ProductStatuses.Active, true);
        nuocMamChinSu.SetCategory(dauAnGia.Id);

        // ===== BÁNH QUY =====
        var oreo = new Product("Bánh Oreo 137g", new Sku("OREO-137"), new Money(18000), "Bánh quy Oreo nhân kem vani 137g", ProductStatuses.Active, true);
        oreo.SetCategory(banhQuy.Id);

        var cosy = new Product("Bánh Cosy 294g", new Sku("COSY-294"), new Money(32000), "Bánh quy Cosy bơ sữa 294g", ProductStatuses.Active, true);
        cosy.SetCategory(banhQuy.Id);

        // ===== SNACK =====
        var ostar = new Product("Snack Ostar 40g", new Sku("OSTAR-40"), new Money(7000), "Snack khoai tây Ostar 40g", ProductStatuses.Active, true);
        ostar.SetCategory(snack.Id);

        var poca = new Product("Bim bim Poca 52g", new Sku("POCA-52"), new Money(8000), "Bim bim Poca gói 52g", ProductStatuses.Active, true);
        poca.SetCategory(snack.Id);

        // ===== GẠO =====
        var gaoST25 = new Product("Gạo ST25 5kg", new Sku("GAO-ST25-5KG"), new Money(135000), "Gạo ST25 gạo ngon nhất thế giới 5kg", ProductStatuses.Active, false);
        gaoST25.SetCategory(gaoNep.Id);

        products.AddRange(new[]
        {
            coca330, pepsi330, sevenUp330, fanta330,
            lavie500, aqua500,
            vinamilk180, thTrue180, suaDacOngTho,
            nescafe, tranhDao,
            haoHao, omachi, kokomi,
            dauNeptune, nuocMamChinSu,
            oreo, cosy,
            ostar, poca,
            gaoST25
        });

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} products.", products.Count);
        return products;
    }

    private static async Task<List<ProductBatch>> SeedProductBatchesAsync(InventoryDbContext context, List<Product> products, ILogger logger)
    {
        logger.LogInformation("Seeding product batches...");

        var batches = new List<ProductBatch>();
        var random = new Random(42); // Fixed seed for reproducibility

        foreach (var product in products.Where(p => p.RequiresBatchTracking))
        {
            // Create 2-3 batches per product
            var batchCount = random.Next(2, 4);

            for (int i = 1; i <= batchCount; i++)
            {
                var manufacturingDate = DateTime.UtcNow.AddDays(-random.Next(30, 180));
                var expiryDate = manufacturingDate.AddMonths(random.Next(6, 24));
                var quantity = random.Next(50, 500);
                var costPrice = product.Price.Value * 0.7m; // 70% of selling price

                var batch = new ProductBatch(
                    product.Id,
                    $"BATCH-{product.Sku.Value}-{i:D3}",
                    quantity,
                    manufacturingDate,
                    expiryDate,
                    costPrice
                );

                batches.Add(batch);
            }
        }

        context.ProductBatches.AddRange(batches);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} product batches.", batches.Count);
        return batches;
    }

    private static async Task SeedStockTransactionsAsync(
        InventoryDbContext context,
        List<Product> products,
        List<ProductBatch> batches,
        List<Supplier> suppliers,
        ILogger logger)
    {
        logger.LogInformation("Seeding stock transactions...");

        var transactions = new List<StockTransaction>();
        var random = new Random(42);

        // Group batches by product
        var batchesByProduct = batches.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var product in products.Where(p => p.RequiresBatchTracking))
        {
            if (!batchesByProduct.TryGetValue(product.Id, out var productBatches))
                continue;

            foreach (var batch in productBatches)
            {
                var supplier = suppliers[random.Next(suppliers.Count)];

                // 1. Purchase transaction (nhập hàng)
                var purchaseTransaction = new StockTransaction(
                    product.Id,
                    batch.Id,
                    supplier.Id,
                    StockTransactionType.Purchase,
                    batch.Quantity,
                    batch.CostPrice,
                    batch.ManufacturingDate.AddDays(random.Next(1, 7)),
                    $"Nhập hàng từ {supplier.Name}"
                );
                transactions.Add(purchaseTransaction);

                // 2. Some sales transactions (bán hàng)
                var salesCount = random.Next(1, 4);
                var remainingQty = batch.Quantity;

                for (int i = 0; i < salesCount && remainingQty > 10; i++)
                {
                    var saleQty = random.Next(5, Math.Min(50, remainingQty / 2));
                    var saleDate = batch.ManufacturingDate.AddDays(random.Next(7, 60));

                    var saleTransaction = new StockTransaction(
                        product.Id,
                        batch.Id,
                        supplier.Id, // Using supplier as reference, in real app would be different
                        StockTransactionType.Sale,
                        saleQty,
                        product.Price.Value,
                        saleDate,
                        "Bán lẻ tại cửa hàng"
                    );
                    transactions.Add(saleTransaction);
                    remainingQty -= saleQty;
                }
            }
        }

        // Add some adjustment transactions
        var adjustmentProducts = products.Where(p => p.RequiresBatchTracking).Take(3).ToList();
        foreach (var product in adjustmentProducts)
        {
            if (!batchesByProduct.TryGetValue(product.Id, out var productBatches))
                continue;

            var batch = productBatches.First();
            var supplier = suppliers.First();

            var adjustmentIn = new StockTransaction(
                product.Id,
                batch.Id,
                supplier.Id,
                StockTransactionType.AdjustmentIn,
                10,
                batch.CostPrice,
                DateTime.UtcNow.AddDays(-5),
                "Kiểm kê phát hiện thừa hàng"
            );
            transactions.Add(adjustmentIn);

            var damage = new StockTransaction(
                product.Id,
                batch.Id,
                supplier.Id,
                StockTransactionType.Damage,
                5,
                batch.CostPrice,
                DateTime.UtcNow.AddDays(-3),
                "Hàng hỏng do vận chuyển"
            );
            transactions.Add(damage);
        }

        context.StockTransactions.AddRange(transactions);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} stock transactions.", transactions.Count);
    }
}
