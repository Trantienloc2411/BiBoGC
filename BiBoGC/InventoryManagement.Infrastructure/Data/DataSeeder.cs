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
            var productVariant = await SeedProductVariantAsync(context, products, logger);
            await SeedStockTransactionsAsync(context, products, batches, suppliers, productVariant, logger);

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
        var biaRuou = new Category("Bia & Rượu", "Bia, rượu các loại", doUong.Id) { DisplayOrder = 6 };

        context.Categories.AddRange(nuocNgot, nuocSuoi, sua, traCaPhe, nuocTraiCay, biaRuou);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { nuocNgot, nuocSuoi, sua, traCaPhe, nuocTraiCay, biaRuou });

        // ===== CHILD CATEGORIES - Thực phẩm =====
        var miGoi = new Category("Mì gói", "Mì ăn liền các loại", thucPham.Id) { DisplayOrder = 1 };
        var dauAnGia = new Category("Dầu ăn & Gia vị", "Dầu ăn, nước mắm, gia vị", thucPham.Id) { DisplayOrder = 2 };
        var doHop = new Category("Đồ hộp", "Thực phẩm đóng hộp", thucPham.Id) { DisplayOrder = 3 };
        var gaoNep = new Category("Gạo & Nếp", "Gạo, nếp, bột các loại", thucPham.Id) { DisplayOrder = 4 };
        var dongLanh = new Category("Đông lạnh", "Thực phẩm đông lạnh", thucPham.Id) { DisplayOrder = 5 };

        context.Categories.AddRange(miGoi, dauAnGia, doHop, gaoNep, dongLanh);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { miGoi, dauAnGia, doHop, gaoNep, dongLanh });

        // ===== CHILD CATEGORIES - Bánh kẹo =====
        var banhQuy = new Category("Bánh quy", "Bánh quy các loại", banhKeo.Id) { DisplayOrder = 1 };
        var keoMut = new Category("Kẹo & Mứt", "Kẹo, mứt các loại", banhKeo.Id) { DisplayOrder = 2 };
        var snack = new Category("Snack", "Bim bim, snack", banhKeo.Id) { DisplayOrder = 3 };

        context.Categories.AddRange(banhQuy, keoMut, snack);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { banhQuy, keoMut, snack });

        // ===== CHILD CATEGORIES - Gia dụng =====
        var giayVeSinh = new Category("Giấy vệ sinh", "Giấy vệ sinh, khăn giấy", giaDung.Id) { DisplayOrder = 1 };
        var chatTayRua = new Category("Chất tẩy rửa", "Nước rửa chén, nước giặt", giaDung.Id) { DisplayOrder = 2 };

        context.Categories.AddRange(giayVeSinh, chatTayRua);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { giayVeSinh, chatTayRua });

        // ===== CHILD CATEGORIES - Vệ sinh cá nhân =====
        var dauGoi = new Category("Dầu gội & Sữa tắm", "Dầu gội, sữa tắm", veSinh.Id) { DisplayOrder = 1 };
        var kemDanhRang = new Category("Kem đánh răng", "Kem đánh răng, bàn chải", veSinh.Id) { DisplayOrder = 2 };

        context.Categories.AddRange(dauGoi, kemDanhRang);
        await context.SaveChangesAsync();

        categories.AddRange(new[] { dauGoi, kemDanhRang });

        logger.LogInformation("Seeded {Count} categories.", categories.Count);
        return categories;
    }

    private static async Task<List<Supplier>> SeedSuppliersAsync(InventoryDbContext context, ILogger logger)
    {
        logger.LogInformation("Seeding suppliers...");

        var suppliers = new List<Supplier>
        {
            new("Công ty TNHH Coca-Cola Việt Nam", "Nguyễn Văn An", "0283456789", "KCN Biên Hòa, Đồng Nai"),
            new("Tập đoàn Vinamilk", "Trần Thị Bình", "0287654321", "10 Tân Trào, Quận 7, TP.HCM"),
            new("Công ty CP Acecook Việt Nam", "Lê Văn Cường", "0283334455", "KCN Tân Bình, TP.HCM"),
            new("Công ty CP Masan Consumer", "Phạm Thị Dung", "0246789012", "Số 1 Hồ Mễ Trì, Hà Nội"),
            new("Công ty TNHH Nestlé Việt Nam", "Hoàng Văn Em", "0284445566", "KCN Amata, Đồng Nai"),
            new("Công ty CP Kinh Đô", "Ngô Thị Phương", "0285556677", "141 Nguyễn Du, Quận 1, TP.HCM"),
            new("Công ty CP Unilever Việt Nam", "Đỗ Văn Giang", "0286667788", "156 Nguyễn Lương Bằng, Quận 7, TP.HCM"),
            new("Công ty TNHH La Vie", "Vũ Thị Hương", "0287778899", "KCN Long Thành, Đồng Nai"),
            new("Công ty CP Bibica", "Bùi Văn Khoa", "0288889900", "443 Lý Thường Kiệt, Quận 11, TP.HCM"),
            new("Tổng Công ty Lương thực Miền Nam", "Mai Thị Lan", "0289990011", "62 Trần Cao Vân, Quận 3, TP.HCM"),
            new("Công ty CP TH True Milk", "Trịnh Văn Minh", "0243334455", "Nghĩa Đàn, Nghệ An"),
            new("Công ty CP Tân Hiệp Phát", "Cao Thị Ngọc", "0650123456", "219 Điện Biên Phủ, Bình Định"),
            new("Công ty CP Sabeco", "Lý Văn Phát", "0281234567", "28-30 Nguyễn Văn Trỗi, Phú Nhuận, TP.HCM"),
            new("Công ty CP Habeco", "Đinh Thị Quỳnh", "0242345678", "10 Trần Nhật Duật, Hoàn Kiếm, Hà Nội"),
            new("Công ty TNHH Diana", "Võ Văn Sơn", "0283456780", "KCN Long Hậu, Long An"),
            new("Công ty CP P&G Việt Nam", "Nguyễn Thị Tâm", "0287890123", "Lầu 10, Tòa Metropole, TP.HCM")
        };

        context.Suppliers.AddRange(suppliers);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} suppliers.", suppliers.Count);
        return suppliers;
    }

    private static async Task<List<Product>> SeedProductsAsync(InventoryDbContext context, List<Category> categories,
        ILogger logger)
    {
        logger.LogInformation("Seeding products...");

        // Get category references
        var nuocNgot = categories.First(c => c.Name == "Nước ngọt");
        var nuocSuoi = categories.First(c => c.Name == "Nước suối");
        var sua = categories.First(c => c.Name == "Sữa");
        var traCaPhe = categories.First(c => c.Name == "Trà & Cà phê");
        var nuocTraiCay = categories.First(c => c.Name == "Nước trái cây");
        var biaRuou = categories.First(c => c.Name == "Bia & Rượu");
        var miGoi = categories.First(c => c.Name == "Mì gói");
        var dauAnGia = categories.First(c => c.Name == "Dầu ăn & Gia vị");
        var banhQuy = categories.First(c => c.Name == "Bánh quy");
        var snack = categories.First(c => c.Name == "Snack");
        var gaoNep = categories.First(c => c.Name == "Gạo & Nếp");
        var dongLanh = categories.First(c => c.Name == "Đông lạnh");
        var doHop = categories.First(c => c.Name == "Đồ hộp");
        var keoMut = categories.First(c => c.Name == "Kẹo & Mứt");
        var giayVeSinh = categories.First(c => c.Name == "Giấy vệ sinh");
        var chatTayRua = categories.First(c => c.Name == "Chất tẩy rửa");
        var dauGoi = categories.First(c => c.Name == "Dầu gội & Sữa tắm");
        var kemDanhRang = categories.First(c => c.Name == "Kem đánh răng");

        var products = new List<Product>();

        // ===== NƯỚC NGỌT =====
        var coca = new Product("Coca-Cola Lon", "Nước ngọt có gas Coca-cola lon 330ml.", ProductStatuses.Active,
            new Sku("COCA-330-LON"), true, Units.Lon, 100);
        coca.SetCategory(nuocNgot.Id);

        var pepsi = new Product("Pepsi lon 330ml", "Nước ngọt có gas Pepsi lon 330ml", ProductStatuses.Active,
            new Sku("PEPSI-330-LON"), true, Units.Lon, 100);
        pepsi.SetCategory(nuocNgot.Id);

        var sevenUp = new Product("7Up Lon 330ml", "Nước ngọt có gas 7Up lon 330ml", ProductStatuses.Active,
            new Sku("7UP-330-LON"), true, Units.Lon, 100);
        sevenUp.SetCategory(nuocNgot.Id);

        var fanta = new Product("Fanta cam lon 330ml", "Nước ngọt có gas Fanta cam lon 330ml", ProductStatuses.Active,
            new Sku("FANTA-CAM-330-LON"), true, Units.Lon, 100);
        fanta.SetCategory(nuocNgot.Id);

        var sprite = new Product("Sprite Lon 330ml", "Nước ngọt có gas Sprite lon 330ml", ProductStatuses.Active,
            new Sku("SPRITE-330-LON"), true, Units.Lon, 100);
        sprite.SetCategory(nuocNgot.Id);

        var sting = new Product("Sting Dâu 330ml", "Nước tăng lực Sting vị dâu lon 330ml", ProductStatuses.Active,
            new Sku("STING-DAU-330"), true, Units.Lon, 80);
        sting.SetCategory(nuocNgot.Id);

        var redbull = new Product("Red Bull 250ml", "Nước tăng lực Red Bull lon 250ml", ProductStatuses.Active,
            new Sku("REDBULL-250"), true, Units.Lon, 60);
        redbull.SetCategory(nuocNgot.Id);

        var number1 = new Product("Number 1 Chanh Muối 455ml", "Nước giải khát Number 1 chanh muối chai 455ml",
            ProductStatuses.Active, new Sku("NUMBER1-CHANHMUOI-455"), true, Units.Chai, 80);
        number1.SetCategory(nuocNgot.Id);

        // ===== NƯỚC SUỐI =====
        var lavie = new Product("Lavie 500ml", "Nước suối lavie tinh khiết 500ml", ProductStatuses.Active,
            new Sku("LAVIE-500"), true, Units.Chai, 100);
        lavie.SetCategory(nuocSuoi.Id);

        var aqua = new Product("Aquafina 500ml", "Nước suối Aquafina tinh khiết chai 500ml", ProductStatuses.Active,
            new Sku("AQUAFINA-500"), true, Units.Chai, 100);
        aqua.SetCategory(nuocSuoi.Id);

        var dasani = new Product("Dasani 500ml", "Nước khoáng Dasani chai 500ml", ProductStatuses.Active,
            new Sku("DASANI-500"), true, Units.Chai, 100);
        dasani.SetCategory(nuocSuoi.Id);

        var vica = new Product("Vica 500ml", "Nước khoáng Vica chai 500ml", ProductStatuses.Active, new Sku("VICA-500"),
            true, Units.Chai, 80);
        vica.SetCategory(nuocSuoi.Id);

        // ===== SỮA =====
        var vinamilk100percent = new Product("Sữa tươi tiệt trùng Vinamilk 180ml",
            "Sữa tươi hộp tiệt trùng 100% Vinamilk 180ml", ProductStatuses.Active, new Sku("VINAMILK-100%-180"), true,
            Units.Hop, 100);
        vinamilk100percent.SetCategory(sua.Id);

        var thTrue = new Product("Sữa TH TrueMilk Có đường 180ml", "Sữa tiệt trùng TH TrueMilk Có đường hộp 180ml",
            ProductStatuses.Active, new Sku("THTRUEMILK-CODUONG-180"), true, Units.Hop, 100);
        thTrue.SetCategory(sua.Id);

        var adnVinamilk = new Product("Sữa ADN Vinamilk 180ml", "Sữa tươi tiệt trùng ADN hộp 180ml",
            ProductStatuses.Discontinued, new Sku("VINAMILK-ADN-180"), true, Units.Hop, 100);
        adnVinamilk.SetCategory(sua.Id);

        var suaDacOngTho = new Product("Sữa đặc Ông Thọ 380g", "Sữa đặc có đường Ông Thọ lon 380g",
            ProductStatuses.Active, new Sku("VINAMILK-ONG-THO-380"), true, Units.Lon, 100);
        suaDacOngTho.SetCategory(sua.Id);

        var dutchLady = new Product("Sữa Dutch Lady Dâu 180ml", "Sữa tiệt trùng Dutch Lady vị dâu hộp 180ml",
            ProductStatuses.Active, new Sku("DUTCHLADY-DAU-180"), true, Units.Hop, 100);
        dutchLady.SetCategory(sua.Id);

        var suaChuaVinamilk = new Product("Sữa chua uống Vinamilk Dâu 180ml",
            "Sữa chua uống Vinamilk vị dâu chai 180ml", ProductStatuses.Active, new Sku("VINAMILK-SUACHUA-DAU-180"),
            true, Units.Chai, 100);
        suaChuaVinamilk.SetCategory(sua.Id);

        var yakult = new Product("Yakult 5 chai", "Sữa chua uống men sống Yakult lốc 5 chai", ProductStatuses.Active,
            new Sku("YAKULT-LOC5"), true, Units.Loc, 80);
        yakult.SetCategory(sua.Id);

        // ===== TRÀ & CÀ PHÊ =====
        var nescafe = new Product("Cà phê NescafÉ 3in1", "Cà phê hòa tan NescafÉ 3in1", ProductStatuses.Active,
            new Sku("CAFE-NESCAFE"), true, Units.Goi, 100);
        nescafe.SetCategory(traCaPhe.Id);

        var tranhDao = new Product("Trà Ô Long không độ 500ml", "Trà Ô Long không độ chai 500ml",
            ProductStatuses.Active, new Sku("OLONGKHONGDO-500"), true, Units.Chai, 100);
        tranhDao.SetCategory(traCaPhe.Id);

        var g7 = new Product("Cà phê G7 3in1 Hòa tan", "Cà phê hòa tan G7 3in1 hộp 21 gói", ProductStatuses.Active,
            new Sku("CAFE-G7-3IN1"), true, Units.Hop, 80);
        g7.SetCategory(traCaPhe.Id);

        var vinacafe = new Product("Cà phê Vinacafe 3in1", "Cà phê hòa tan Vinacafe 3in1 hộp 20 gói",
            ProductStatuses.Active, new Sku("CAFE-VINACAFE-3IN1"), true, Units.Hop, 80);
        vinacafe.SetCategory(traCaPhe.Id);

        var traThaiXanh = new Product("Trà Thái Xanh 350ml", "Trà xanh Thái Nguyên không độ chai 350ml",
            ProductStatuses.Active, new Sku("TRA-THAIXANH-350"), true, Units.Chai, 100);
        traThaiXanh.SetCategory(traCaPhe.Id);

        var liptonLipton = new Product("Trà Lipton Chanh 320ml", "Trà đen Lipton vị chanh lon 320ml",
            ProductStatuses.Active, new Sku("LIPTON-CHANH-320"), true, Units.Lon, 80);
        liptonLipton.SetCategory(traCaPhe.Id);

        // ===== NƯỚC TRÁI CÂY =====
        var twister = new Product("Twister Cam 280ml", "Nước ép trái cây Twister cam chai 280ml",
            ProductStatuses.Active, new Sku("TWISTER-CAM-280"), true, Units.Chai, 80);
        twister.SetCategory(nuocTraiCay.Id);

        var nutriboost = new Product("Nutriboost Dâu 297ml", "Sữa trái cây Nutriboost dâu chai 297ml",
            ProductStatuses.Active, new Sku("NUTRIBOOST-DAU-297"), true, Units.Chai, 80);
        nutriboost.SetCategory(nuocTraiCay.Id);

        var teppy = new Product("TH True Juice Cam 330ml", "Nước ép cam nguyên chất TH True Juice chai 330ml",
            ProductStatuses.Active, new Sku("THTRUEJUICE-CAM-330"), true, Units.Chai, 60);
        teppy.SetCategory(nuocTraiCay.Id);

        var minute = new Product("Minute Maid Pulpy Cam 350ml", "Nước cam có tép Minute Maid Pulpy chai 350ml",
            ProductStatuses.Active, new Sku("MINUTEMAID-CAM-350"), true, Units.Chai, 70);
        minute.SetCategory(nuocTraiCay.Id);

        // ===== BIA & RƯỢU =====
        var saigonDo = new Product("Bia Sài Gòn Đỏ 330ml", "Bia Sài Gòn Đỏ lon 330ml", ProductStatuses.Active,
            new Sku("BIA-SAIGONDO-330"), true, Units.Lon, 100);
        saigonDo.SetCategory(biaRuou.Id);

        var heineken = new Product("Bia Heineken 330ml", "Bia Heineken lon 330ml", ProductStatuses.Active,
            new Sku("BIA-HEINEKEN-330"), true, Units.Lon, 80);
        heineken.SetCategory(biaRuou.Id);

        var tiger = new Product("Bia Tiger 330ml", "Bia Tiger lon 330ml", ProductStatuses.Active,
            new Sku("BIA-TIGER-330"), true, Units.Lon, 100);
        tiger.SetCategory(biaRuou.Id);

        var bivina = new Product("Bia Bivina 330ml", "Bia Bivina lon 330ml", ProductStatuses.Active,
            new Sku("BIA-BIVINA-330"), true, Units.Lon, 100);
        bivina.SetCategory(biaRuou.Id);

        // ===== MÌ GÓI =====
        var haoHao = new Product("Mì Hảo Hảo tôm chua cay", "Mì ăn liền Hảo Hảo vị tôm chua cay",
            ProductStatuses.Active, new Sku("HAOHAO-75"), true, Units.Goi, 100);
        haoHao.SetCategory(miGoi.Id);

        var omachi = new Product("Mì gói Omachi xốt bò hầm", "Mì khoai tây Omachi gói vị xốt bò hầm",
            ProductStatuses.Active, new Sku("OMACHI-BO-GOI"), true, Units.Goi, 100);
        omachi.SetCategory(miGoi.Id);

        var kokomi = new Product("Mì gói Kokomi đại gà quay", "Mì ăn liền Kokomi vị gà quay", ProductStatuses.Active,
            new Sku("KOKOMI-GA"), true, Units.Goi, 100);
        kokomi.SetCategory(miGoi.Id);

        var migoreng = new Product("Mì Mì Goreng xào khô", "Mì xào khô Indomie Mì Goreng", ProductStatuses.Active,
            new Sku("MIGORENG-GOI"), true, Units.Goi, 80);
        migoreng.SetCategory(miGoi.Id);

        var bamien = new Product("Mì 3 Miền tôm chua cay", "Mì ăn liền 3 Miền vị tôm chua cay", ProductStatuses.Active,
            new Sku("3MIEN-TOM"), true, Units.Goi, 100);
        bamien.SetCategory(miGoi.Id);

        var mivina = new Product("Mì Vifon gà tỏi phi", "Mì ăn liền Vifon vị gà tỏi phi", ProductStatuses.Active,
            new Sku("VIFON-GA"), true, Units.Goi, 100);
        mivina.SetCategory(miGoi.Id);

        var haoHaoBo = new Product("Mì Hảo Hảo bò", "Mì ăn liền Hảo Hảo vị bò", ProductStatuses.Active,
            new Sku("HAOHAO-BO-75"), true, Units.Goi, 100);
        haoHaoBo.SetCategory(miGoi.Id);

        // ===== DẦU ĂN & GIA VỊ =====
        var dauNeptune = new Product("Dầu ăn Neptune 1L", "Dầu ăn Neptune chai 1 lít", ProductStatuses.OutOfStock,
            new Sku("NEPTUNE-1L"), true, Units.Chai, 50);
        dauNeptune.SetCategory(dauAnGia.Id);

        var dauTuongAn = new Product("Dầu ăn Tường An 1L", "Dầu ăn Tường An chai 1 lít", ProductStatuses.Active,
            new Sku("TUONGAN-1L"), true, Units.Chai, 50);
        dauTuongAn.SetCategory(dauAnGia.Id);

        var nuocMamChinSu = new Product("Nước mắm Chin-su 500ml", "Nước mắm Chin-su chai 500ml", ProductStatuses.Active,
            new Sku("CHINSU-NUOCMAM-500"), true, Units.Chai, 100);
        nuocMamChinSu.SetCategory(dauAnGia.Id);

        var dauDau = new Product("Dầu ăn Simply 1L", "Dầu đậu nành Simply chai 1 lít", ProductStatuses.Active,
            new Sku("SIMPLY-1L"), true, Units.Chai, 50);
        dauDau.SetCategory(dauAnGia.Id);

        var namChinSu = new Product("Nam ngư Chin-su 250ml", "Nước tương Chin-su nam ngư chai 250ml",
            ProductStatuses.Active, new Sku("CHINSU-NAMINGU-250"), true, Units.Chai, 80);
        namChinSu.SetCategory(dauAnGia.Id);
        var muoiOt = new Product("Muối ớt Tây Ninh 50g", "Muối ớt xanh Tây Ninh gói 50g", ProductStatuses.Active,
            new Sku("MUOIOT-50"), true, Units.Goi, 100);
        muoiOt.SetCategory(dauAnGia.Id);

        var maggiBlock = new Product("Maggi Hạt nêm 400g", "Hạt nêm Maggi gói 400g", ProductStatuses.Active,
            new Sku("MAGGI-HATNEM-400"), true, Units.Goi, 80);
        maggiBlock.SetCategory(dauAnGia.Id);

        var knorr = new Product("Knorr Hạt nêm 400g", "Hạt nêm Knorr gói 400g", ProductStatuses.Active,
            new Sku("KNORR-HATNEM-400"), true, Units.Goi, 80);
        knorr.SetCategory(dauAnGia.Id);

        // ===== ĐỒ HỘP =====
        var caDinh = new Product("Cá mòi sốt cà Vissan 155g", "Cá mòi sốt cà Vissan hộp 155g", ProductStatuses.Active,
            new Sku("VISSAN-CAMOI-155"), true, Units.Hop, 60);
        caDinh.SetCategory(doHop.Id);

        var pateMasan = new Product("Pate Masan 150g", "Pate heo Masan hộp 150g", ProductStatuses.Active,
            new Sku("MASAN-PATE-150"), true, Units.Hop, 60);
        pateMasan.SetCategory(doHop.Id);

        var thitBo = new Product("Thịt hộp Spam 340g", "Thịt hộp Spam lon 340g", ProductStatuses.Active,
            new Sku("SPAM-340"), true, Units.Lon, 40);
        thitBo.SetCategory(doHop.Id);

        var ngaoNgot = new Product("Ngao sốt me Thái Lan 140g", "Ngao sốt me Thái Lan hộp 140g", ProductStatuses.Active,
            new Sku("NGAO-ME-140"), true, Units.Hop, 50);
        ngaoNgot.SetCategory(doHop.Id);

        // ===== BÁNH QUY =====
        var oreo = new Product("Bánh Oreo 137g", "Bánh quy Oreo nhân kem vani 137g", ProductStatuses.Active,
            new Sku("OREO-137"), true, Units.Thanh, 10);
        oreo.SetCategory(banhQuy.Id);

        var cosy = new Product("Bánh Cosy 294g", "Bánh quy Cosy bơ sữa 294g", ProductStatuses.Active,
            new Sku("COSYBOSUA-294"), true, Units.Hop, 10);
        cosy.SetCategory(banhQuy.Id);

        var royco = new Product("Bánh Gấu Royco 120g", "Bánh quy Gấu Royco gói 120g", ProductStatuses.Active,
            new Sku("ROYCO-GAU-120"), true, Units.Goi, 80);
        royco.SetCategory(banhQuy.Id);

        var nabisco = new Product("Bánh Ritz Nabisco 200g", "Bánh quy mặn Ritz Nabisco hộp 200g",
            ProductStatuses.Active, new Sku("RITZ-200"), true, Units.Hop, 50);
        nabisco.SetCategory(banhQuy.Id);

        var orea = new Product("Bánh Cream-O 85g", "Bánh quy Cream-O nhân kem gói 85g", ProductStatuses.Active,
            new Sku("CREAMO-85"), true, Units.Goi, 100);
        orea.SetCategory(banhQuy.Id);

        // ===== SNACK =====
        var ostar = new Product("Snack Ostar 40g", "Snack khoai tây Ostar 40g", ProductStatuses.Active,
            new Sku("OSTAR-40"), true, Units.Goi, 20);
        ostar.SetCategory(snack.Id);

        var poca = new Product("Bim bim Poca 52g", "Bim bim Poca gói 52g", ProductStatuses.Active, new Sku("POCA-52"),
            true, Units.Goi, 20);
        poca.SetCategory(snack.Id);

        var lays = new Product("Snack Lay's 52g", "Snack khoai tây Lay's vị kem chua hành tây 52g",
            ProductStatuses.Active, new Sku("LAYS-52"), true, Units.Goi, 80);
        lays.SetCategory(snack.Id);

        var swing = new Product("Snack Swing 60g", "Snack khoai tây Swing gói 60g", ProductStatuses.Active,
            new Sku("SWING-60"), true, Units.Goi, 80);
        swing.SetCategory(snack.Id);

        var onion = new Product("Snack Onion Rings 40g", "Snack hành tây Onion Rings gói 40g", ProductStatuses.Active,
            new Sku("ONIONRINGS-40"), true, Units.Goi, 100);
        onion.SetCategory(snack.Id);

        // ===== KẸO & MỨT =====
        var mentos = new Product("Kẹo Mentos 37.5g", "Kẹo nhai Mentos hương trái cây cuộn 37.5g",
            ProductStatuses.Active, new Sku("MENTOS-37"), true, Units.Cuon, 80);
        mentos.SetCategory(keoMut.Id);

        var alpenlibe = new Product("Kẹo Alpenliebe 120 viên", "Kẹo ngậm Alpenliebe vị kem sữa gói 120 viên",
            ProductStatuses.Active, new Sku("ALPENLIEBE-120"), true, Units.Goi, 60);
        alpenlibe.SetCategory(keoMut.Id);

        var kopiko = new Product("Kẹo Kopiko Cappuccino 150g", "Kẹo cà phê Kopiko Cappuccino gói 150g",
            ProductStatuses.Active, new Sku("KOPIKO-CAPP-150"), true, Units.Goi, 70);
        kopiko.SetCategory(keoMut.Id);

        var dynamite = new Product("Kẹo Dynamite 120g", "Kẹo chua ngọt Dynamite gói 120g", ProductStatuses.Active,
            new Sku("DYNAMITE-120"), true, Units.Goi, 80);
        dynamite.SetCategory(keoMut.Id);

        // ===== GẠO =====
        var gaoST25 = new Product("Gạo ST25 5kg", "Gạo ST25 gạo ngon nhất thế giới 5kg", ProductStatuses.Active,
            new Sku("ST25-5"), false, Units.Tui, 20);
        gaoST25.SetCategory(gaoNep.Id);

        var gaoThom = new Product("Gạo Thơm Jasmine 5kg", "Gạo thơm Jasmine Việt Nam túi 5kg", ProductStatuses.Active,
            new Sku("JASMINE-5"), false, Units.Tui, 30);
        gaoThom.SetCategory(gaoNep.Id);

        var gaoTamThom = new Product("Gạo Tám Thơm 5kg", "Gạo Tám Thơm túi 5kg", ProductStatuses.Active,
            new Sku("TAMTHOM-5"), false, Units.Tui, 25);
        gaoTamThom.SetCategory(gaoNep.Id);

        var gaoNang = new Product("Gạo Nàng Hoa 5kg", "Gạo Nàng Hoa túi 5kg", ProductStatuses.Active,
            new Sku("NANGHOA-5"), false, Units.Tui, 25);
        gaoNang.SetCategory(gaoNep.Id);

        // ===== ĐÔNG LẠNH =====
        var xucXich = new Product("Xúc xích Đức Việt 500g", "Xúc xích heo Đức Việt gói 500g", ProductStatuses.Active,
            new Sku("XUCXICH-DUCVIET-500"), true, Units.Goi, 40);
        xucXich.SetCategory(dongLanh.Id);

        var chaTom = new Product("Chả tôm Bá Kiến 200g", "Chả tôm Bá Kiến gói 200g", ProductStatuses.Active,
            new Sku("CHATOM-BAKIEN-200"), true, Units.Goi, 35);
        chaTom.SetCategory(dongLanh.Id);

        var nem = new Product("Nem rán CJ 300g", "Nem rán nhân thịt CJ gói 300g", ProductStatuses.Active,
            new Sku("NEMRAN-CJ-300"), true, Units.Goi, 30);
        nem.SetCategory(dongLanh.Id);

        // ===== GIẤY VỆ SINH =====
        var giayRoll = new Product("Giấy vệ sinh Pulppy 10 cuộn", "Giấy vệ sinh Pulppy 2 lớp lốc 10 cuộn",
            ProductStatuses.Active, new Sku("PULPPY-10"), false, Units.Loc, 50);
        giayRoll.SetCategory(giayVeSinh.Id);

        var khanGiay = new Product("Khăn giấy Kleenex 3 lớp", "Khăn giấy rút Kleenex 3 lớp hộp 100 tờ",
            ProductStatuses.Active, new Sku("KLEENEX-100"), false, Units.Hop, 60);
        khanGiay.SetCategory(giayVeSinh.Id);

        var giayBella = new Product("Giấy vệ sinh Bella 12 cuộn", "Giấy vệ sinh Bella 3 lớp lốc 12 cuộn",
            ProductStatuses.Active, new Sku("BELLA-12"), false, Units.Loc, 40);
        giayBella.SetCategory(giayVeSinh.Id);

        // ===== CHẤT TẨY RỬA =====
        var sunlight = new Product("Nước rửa chén Sunlight 750ml", "Nước rửa chén Sunlight chanh chai 750ml",
            ProductStatuses.Active, new Sku("SUNLIGHT-750"), false, Units.Chai, 50);
        sunlight.SetCategory(chatTayRua.Id);

        var vixNuocGiat = new Product("Nước giặt OMO Matic 3.8kg", "Nước giặt OMO cho máy giặt túi 3.8kg",
            ProductStatuses.Active, new Sku("OMO-MATIC-3.8"), false, Units.Tui, 30);
        vixNuocGiat.SetCategory(chatTayRua.Id);

        var botGiat = new Product("Bột giặt Tide 720g", "Bột giặt Tide trắng sáng gói 720g", ProductStatuses.Active,
            new Sku("TIDE-720"), false, Units.Goi, 50);
        botGiat.SetCategory(chatTayRua.Id);

        var nuocLau = new Product("Nước lau sàn Sunlight 3.6kg", "Nước lau sàn Sunlight can 3.6kg",
            ProductStatuses.Active, new Sku("SUNLIGHT-LAUSAN-3.6"), false, Units.Can, 35);
        nuocLau.SetCategory(chatTayRua.Id);

        // ===== DẦU GỘI & SỮA TẮM =====
        var dauGoiClear = new Product("Dầu gội Clear 630ml", "Dầu gội Clear men sạch gàu chai 630ml",
            ProductStatuses.Active, new Sku("CLEAR-630"), false, Units.Chai, 40);
        dauGoiClear.SetCategory(dauGoi.Id);

        var dauGoiDove = new Product("Dầu gội Dove 650ml", "Dầu gội Dove phục hồi hư tổn chai 650ml",
            ProductStatuses.Active, new Sku("DOVE-DAUGOI-650"), false, Units.Chai, 40);
        dauGoiDove.SetCategory(dauGoi.Id);

        var suaTamLifebuoy = new Product("Sữa tắm Lifebuoy 850ml", "Sữa tắm Lifebuoy bảo vệ vượt trội chai 850ml",
            ProductStatuses.Active, new Sku("LIFEBUOY-850"), false, Units.Chai, 45);
        suaTamLifebuoy.SetCategory(dauGoi.Id);

        var suaTamDove = new Product("Sữa tắm Dove 530ml", "Sữa tắm Dove dưỡng ẩm sâu chai 530ml",
            ProductStatuses.Active, new Sku("DOVE-SUATAM-530"), false, Units.Chai, 40);
        suaTamDove.SetCategory(dauGoi.Id);

        // ===== KEM ĐÁNH RĂNG =====
        var psColgate = new Product("Kem đánh răng Colgate 200g", "Kem đánh răng Colgate MaxFresh tuýp 200g",
            ProductStatuses.Active, new Sku("COLGATE-200"), false, Units.Hop, 60);
        psColgate.SetCategory(kemDanhRang.Id);

        var psPs = new Product("Kem đánh răng P/S 230g", "Kem đánh răng P/S chăm sóc nướu tuýp 230g",
            ProductStatuses.Active, new Sku("PS-230"), false, Units.Hop, 60);
        psPs.SetCategory(kemDanhRang.Id);

        var closeup = new Product("Kem đánh răng Close Up 160g", "Kem đánh răng Close Up bạc hà tuýp 160g",
            ProductStatuses.Active, new Sku("CLOSEUP-160"), false, Units.Hop, 65);
        closeup.SetCategory(kemDanhRang.Id);

        products.AddRange(new[]
        {
            // Nước ngọt
            coca, pepsi, sevenUp, fanta, sprite, sting, redbull, number1,
            // Nước suối
            lavie, aqua, dasani, vica,
            // Sữa
            vinamilk100percent, thTrue, suaDacOngTho, adnVinamilk, dutchLady, suaChuaVinamilk, yakult,
            // Trà & Cà phê
            nescafe, tranhDao, g7, vinacafe, traThaiXanh, liptonLipton,
            // Nước trái cây
            twister, nutriboost, teppy, minute,
            // Bia & Rượu
            saigonDo, heineken, tiger, bivina,
            // Mì gói
            haoHao, omachi, kokomi, migoreng, bamien, mivina, haoHaoBo,
            // Dầu ăn & Gia vị
            dauNeptune, nuocMamChinSu, dauTuongAn, dauDau, namChinSu, muoiOt, maggiBlock, knorr,
            // Đồ hộp
            caDinh, pateMasan, thitBo, ngaoNgot,
            // Bánh quy
            oreo, cosy, royco, nabisco, orea,
            // Snack
            ostar, poca, lays, swing, onion,
            // Kẹo & Mứt
            mentos, alpenlibe, kopiko, dynamite,
            // Gạo
            gaoST25, gaoThom, gaoTamThom, gaoNang,
            // Đông lạnh
            xucXich, chaTom, nem,
            // Giấy vệ sinh
            giayRoll, khanGiay, giayBella,
            // Chất tẩy rửa
            sunlight, vixNuocGiat, botGiat, nuocLau,
            // Dầu gội & Sữa tắm
            dauGoiClear, dauGoiDove, suaTamLifebuoy, suaTamDove,
            // Kem đánh răng
            psColgate, psPs, closeup
        });

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} products.", products.Count);
        return products;
    }

    private static async Task<List<ProductVariant>> SeedProductVariantAsync(InventoryDbContext context,
        List<Product> products, ILogger logger)
    {
        logger.LogInformation("Seeding product variants...");

        var productVariants = new List<ProductVariant>();

        // ===== COCA-COLA VARIANTS =====
        var coca = products.First(p => p.SkuGeneral.Value == "COCA-330-LON");
        var cocaLoc6 = coca.AddProductVariant("COCA-330-LON-6", coca.Id, "Lốc 6 lon", Units.Loc, 6, new Money(42000), 1,
            null, new Money(38000));
        var cocaThung24 = coca.AddProductVariant("COCA-330-LON-24", coca.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(165000), 2, null, new Money(150000));
        productVariants.AddRange(new[] { cocaLoc6, cocaThung24 });

        // ===== PEPSI VARIANTS =====
        var pepsi = products.First(p => p.SkuGeneral.Value == "PEPSI-330-LON");
        var pepsiLoc6 = pepsi.AddProductVariant("PEPSI-330-LON-6", pepsi.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(42000), 1, null, new Money(38000));
        var pepsiThung24 = pepsi.AddProductVariant("PEPSI-330-LON-24", pepsi.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(165000), 2, null, new Money(150000));
        productVariants.AddRange(new[] { pepsiLoc6, pepsiThung24 });

        // ===== 7UP VARIANTS =====
        var sevenUp = products.First(p => p.SkuGeneral.Value == "7UP-330-LON");
        var sevenUpLoc6 = sevenUp.AddProductVariant("7UP-330-LON-6", sevenUp.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(45000), 1, null, new Money(40000));
        var sevenUpThung24 = sevenUp.AddProductVariant("7UP-330-LON-24", sevenUp.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(175000), 2, null, new Money(158000));
        productVariants.AddRange(new[] { sevenUpLoc6, sevenUpThung24 });

        // ===== SPRITE VARIANTS =====
        var sprite = products.First(p => p.SkuGeneral.Value == "SPRITE-330-LON");
        var spriteLoc6 = sprite.AddProductVariant("SPRITE-330-LON-6", sprite.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(42000), 1, null, new Money(38000));
        var spriteThung24 = sprite.AddProductVariant("SPRITE-330-LON-24", sprite.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(165000), 2, null, new Money(150000));
        productVariants.AddRange(new[] { spriteLoc6, spriteThung24 });

        // ===== STING VARIANTS =====
        var sting = products.First(p => p.SkuGeneral.Value == "STING-DAU-330");
        var stingLoc6 = sting.AddProductVariant("STING-DAU-330-6", sting.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(58000), 1, null, new Money(52000));
        var stingThung24 = sting.AddProductVariant("STING-DAU-330-24", sting.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(225000), 2, null, new Money(205000));
        productVariants.AddRange(new[] { stingLoc6, stingThung24 });

        // ===== RED BULL VARIANTS =====
        var redbull = products.First(p => p.SkuGeneral.Value == "REDBULL-250");
        var redbullLoc4 = redbull.AddProductVariant("REDBULL-250-4", redbull.Id, "Lốc 4 lon", Units.Loc, 4,
            new Money(58000), 1, null, new Money(52000));
        var redbullThung24 = redbull.AddProductVariant("REDBULL-250-24", redbull.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(340000), 2, null, new Money(310000));
        productVariants.AddRange(new[] { redbullLoc4, redbullThung24 });

        // ===== LAVIE VARIANTS =====
        var lavie = products.First(p => p.SkuGeneral.Value == "LAVIE-500");
        var lavieLoc12 = lavie.AddProductVariant("LAVIE-500-12", lavie.Id, "Lốc 12 chai", Units.Loc, 12,
            new Money(52000), 1, null, new Money(47000));
        var lavieThung24 = lavie.AddProductVariant("LAVIE-500-24", lavie.Id, "Thùng 24 chai", Units.Thung, 24,
            new Money(100000), 2, null, new Money(92000));
        productVariants.AddRange(new[] { lavieLoc12, lavieThung24 });

        // ===== AQUAFINA VARIANTS =====
        var aqua = products.First(p => p.SkuGeneral.Value == "AQUAFINA-500");
        var aquaLoc12 = aqua.AddProductVariant("AQUAFINA-500-12", aqua.Id, "Lốc 12 chai", Units.Loc, 12,
            new Money(52000), 1, null, new Money(47000));
        var aquaThung24 = aqua.AddProductVariant("AQUAFINA-500-24", aqua.Id, "Thùng 24 chai", Units.Thung, 24,
            new Money(100000), 2, null, new Money(92000));
        productVariants.AddRange(new[] { aquaLoc12, aquaThung24 });

        // ===== VINAMILK 100% VARIANTS =====
        var vinamilk = products.First(p => p.SkuGeneral.Value == "VINAMILK-100%-180");
        var vinamilkThung48 = vinamilk.AddProductVariant("VINAMILK-100%-180-48", vinamilk.Id, "Thùng 48 hộp",
            Units.Thung, 48, new Money(280000), 1, null, new Money(255000));
        productVariants.Add(vinamilkThung48);

        // ===== TH TRUE MILK VARIANTS =====
        var thTrue = products.First(p => p.SkuGeneral.Value == "THTRUEMILK-CODUONG-180");
        var thTrueThung48 = thTrue.AddProductVariant("THTRUEMILK-CODUONG-180-48", thTrue.Id, "Thùng 48 hộp",
            Units.Thung, 48, new Money(395000), 1, null, new Money(360000));
        productVariants.Add(thTrueThung48);

        // ===== DUTCH LADY VARIANTS =====
        var dutchLady = products.First(p => p.SkuGeneral.Value == "DUTCHLADY-DAU-180");
        var dutchLadyThung48 = dutchLady.AddProductVariant("DUTCHLADY-DAU-180-48", dutchLady.Id, "Thùng 48 hộp",
            Units.Thung, 48, new Money(325000), 1, null, new Money(295000));
        productVariants.Add(dutchLadyThung48);

        // ===== YAKULT VARIANTS =====
        var yakult = products.First(p => p.SkuGeneral.Value == "YAKULT-LOC5");
        var yakultThung10 = yakult.AddProductVariant("YAKULT-LOC5-10", yakult.Id, "Thùng 10 lốc", Units.Thung, 10,
            new Money(175000), 1, null, new Money(160000));
        productVariants.Add(yakultThung10);

        // ===== HẢO HẢO VARIANTS =====
        var haoHao = products.First(p => p.SkuGeneral.Value == "HAOHAO-75");
        var haoHaoThung30 = haoHao.AddProductVariant("HAOHAO-75-30", haoHao.Id, "Thùng 30 gói", Units.Thung, 30,
            new Money(145000), 1, null, new Money(132000));
        productVariants.Add(haoHaoThung30);

        // ===== OMACHI VARIANTS =====
        var omachi = products.First(p => p.SkuGeneral.Value == "OMACHI-BO-GOI");
        var omachiThung30 = omachi.AddProductVariant("OMACHI-BO-GOI-30", omachi.Id, "Thùng 30 gói", Units.Thung, 30,
            new Money(205000), 1, null, new Money(187000));
        productVariants.Add(omachiThung30);

        // ===== KOKOMI VARIANTS =====
        var kokomi = products.First(p => p.SkuGeneral.Value == "KOKOMI-GA");
        var kokomiThung30 = kokomi.AddProductVariant("KOKOMI-GA-30", kokomi.Id, "Thùng 30 gói", Units.Thung, 30,
            new Money(102000), 1, null, new Money(93000));
        productVariants.Add(kokomiThung30);

        // ===== MI GORENG VARIANTS =====
        var migoreng = products.First(p => p.SkuGeneral.Value == "MIGORENG-GOI");
        var migorengLoc5 = migoreng.AddProductVariant("MIGORENG-GOI-5", migoreng.Id, "Lốc 5 gói", Units.Loc, 5,
            new Money(31000), 1, null, new Money(28000));
        var migorengThung40 = migoreng.AddProductVariant("MIGORENG-GOI-40", migoreng.Id, "Thùng 40 gói", Units.Thung,
            40, new Money(245000), 2, null, new Money(223000));
        productVariants.AddRange(new[] { migorengLoc5, migorengThung40 });

        // ===== 3 MIỀN VARIANTS =====
        var mien3 = products.First(p => p.SkuGeneral.Value == "3MIEN-TOM");
        var mien3Thung30 = mien3.AddProductVariant("3MIEN-TOM-30", mien3.Id, "Thùng 30 gói", Units.Thung, 30,
            new Money(132000), 1, null, new Money(120000));
        productVariants.Add(mien3Thung30);

        // ===== OREO VARIANTS =====
        var oreo = products.First(p => p.SkuGeneral.Value == "OREO-137");
        var oreoHop12 = oreo.AddProductVariant("OREO-137-12", oreo.Id, "Hộp 12 thanh", Units.Hop, 12, new Money(210000),
            1, null, new Money(192000));
        productVariants.Add(oreoHop12);

        // ===== OSTAR VARIANTS =====
        var ostar = products.First(p => p.SkuGeneral.Value == "OSTAR-40");
        var ostarLoc10 = ostar.AddProductVariant("OSTAR-40-10", ostar.Id, "Lốc 10 gói", Units.Loc, 10, new Money(68000),
            1, null, new Money(62000));
        productVariants.Add(ostarLoc10);
        // ===== POCA VARIANTS =====
        var poca = products.First(p => p.SkuGeneral.Value == "POCA-52");
        var pocaLoc10 = poca.AddProductVariant("POCA-52-10", poca.Id, "Lốc 10 gói", Units.Loc, 10, new Money(78000), 1,
            null, new Money(71000));
        productVariants.Add(pocaLoc10);

        // ===== LAY'S VARIANTS =====
        var lays = products.First(p => p.SkuGeneral.Value == "LAYS-52");
        var laysLoc8 = lays.AddProductVariant("LAYS-52-8", lays.Id, "Lốc 8 gói", Units.Loc, 8, new Money(78000), 1,
            null, new Money(71000));
        productVariants.Add(laysLoc8);

        // ===== HEINEKEN VARIANTS =====
        var heineken = products.First(p => p.SkuGeneral.Value == "BIA-HEINEKEN-330");
        var heinekenLoc6 = heineken.AddProductVariant("BIA-HEINEKEN-330-6", heineken.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(105000), 1, null, new Money(96000));
        var heinekenThung24 = heineken.AddProductVariant("BIA-HEINEKEN-330-24", heineken.Id, "Thùng 24 lon",
            Units.Thung, 24, new Money(410000), 2, null, new Money(374000));
        productVariants.AddRange(new[] { heinekenLoc6, heinekenThung24 });

        // ===== TIGER VARIANTS =====
        var tiger = products.First(p => p.SkuGeneral.Value == "BIA-TIGER-330");
        var tigerLoc6 = tiger.AddProductVariant("BIA-TIGER-330-6", tiger.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(82000), 1, null, new Money(75000));
        var tigerThung24 = tiger.AddProductVariant("BIA-TIGER-330-24", tiger.Id, "Thùng 24 lon", Units.Thung, 24,
            new Money(320000), 2, null, new Money(292000));
        productVariants.AddRange(new[] { tigerLoc6, tigerThung24 });

        // ===== SÀI GÒN ĐỎ VARIANTS =====
        var saigonDo = products.First(p => p.SkuGeneral.Value == "BIA-SAIGONDO-330");
        var saigonDoLoc6 = saigonDo.AddProductVariant("BIA-SAIGONDO-330-6", saigonDo.Id, "Lốc 6 lon", Units.Loc, 6,
            new Money(70000), 1, null, new Money(64000));
        var saigonDoThung24 = saigonDo.AddProductVariant("BIA-SAIGONDO-330-24", saigonDo.Id, "Thùng 24 lon",
            Units.Thung, 24, new Money(275000), 2, null, new Money(251000));
        productVariants.AddRange(new[] { saigonDoLoc6, saigonDoThung24 });

        // Add a default variant (quantityBaseUnit = 1) for every product that has no variants yet
        foreach (var product in products.Where(p => !p.Variants.Any()))
        {
            var defaultVariant = product.AddProductVariant(
                $"{product.SkuGeneral.Value}-DEFAULT",
                product.Id,
                "Mặc định",
                product.BaseUnits,
                1,
                new Money(0),
                1,
                null,
                null
            );
            productVariants.Add(defaultVariant);
        }

        context.ProductVariants.AddRange(productVariants);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} product variants.", productVariants.Count);
        return productVariants;
    }


    private static async Task<List<ProductBatch>> SeedProductBatchesAsync(InventoryDbContext context,
        List<Product> products, ILogger logger)
    {
        logger.LogInformation("Seeding product batches...");

        var batches = new List<ProductBatch>();
        var random = new Random(42); // Fixed seed for reproducibility

        foreach (var product in products.Where(p => p.RequiresBatchTracking))
        {
            // Create 2-3 batches per product
            var batchCount = random.Next(2, 4);

            for (var i = 1; i <= batchCount; i++)
            {
                var manufacturingDate = DateTime.UtcNow.AddDays(-random.Next(30, 180));
                var expiryDate = manufacturingDate.AddMonths(random.Next(6, 24));
                var quantity = random.Next(50, 500);
                var costPrice = random.Next(5000, 200000); // arbitrary cost price for seed data

                var batch = new ProductBatch(
                    product.Id,
                    $"BATCH-{product.SkuGeneral.Value}-{i:D3}",
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
        List<ProductVariant> productVariants,
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

                for (var i = 0; i < salesCount && remainingQty > 10; i++)
                {
                    var saleQty = random.Next(5, Math.Min(50, remainingQty / 2));
                    var saleDate = batch.ManufacturingDate.AddDays(random.Next(7, 60));

                    var saleTransaction = new StockTransaction(
                        product.Id,
                        batch.Id,
                        supplier.Id,
                        StockTransactionType.Sale,
                        saleQty,
                        product.Variants.Any() ? product.Variants.First().SalePrice.Value : batch.CostPrice * 1.3m,
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

        // Update product TotalStock based on the seeded transactions
        var productDict = products.ToDictionary(p => p.Id);
        foreach (var transaction in transactions)
        {
            if (!productDict.TryGetValue(transaction.ProductId, out var product))
                continue;
            if (transaction.IsInboundTransaction())
                product.IncreaseStock(transaction.Quantity);
            else if (transaction.IsOutboundTransaction())
                product.DecreaseStock(transaction.Quantity);
        }

        context.StockTransactions.AddRange(transactions);
        await context.SaveChangesAsync();

        // Persist updated TotalStock values back to the Products table
        context.Products.UpdateRange(products.Where(p => p.RequiresBatchTracking));
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} stock transactions.", transactions.Count);
    }
}