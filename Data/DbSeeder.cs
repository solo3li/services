using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServicesApp.Models.Entities;

namespace ServicesApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        // Roles
        string[] roles = ["Admin", "Student", "Executor"];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // Assign ALL permissions to Admin role
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (adminRole != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(adminRole);
            foreach (var permission in Models.AppPermissions.All)
            {
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(adminRole, new System.Security.Claims.Claim("Permission", permission));
                }
            }
        }

        // Admin user
        var adminEmail = "admin@services.io";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Platform Admin",
                EmailConfirmed = true,
                IsExecutor = true,
                ExecutorStatus = ExecutorStatus.Approved,
                IsActive = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRolesAsync(admin, ["Admin", "Student", "Executor"]);
        }

        // Demo executor
        var execEmail = "john@executor.com";
        ApplicationUser? executor = null;
        if (await userManager.FindByEmailAsync(execEmail) == null)
        {
            executor = new ApplicationUser
            {
                UserName = execEmail,
                Email = execEmail,
                FullName = "John Designer",
                EmailConfirmed = true,
                IsExecutor = true,
                ExecutorStatus = ExecutorStatus.Approved,
                Bio = "Full-stack developer & UI/UX Designer with 5+ years of experience.",
                IsActive = true
            };
            await userManager.CreateAsync(executor, "Executor@123");
            await userManager.AddToRolesAsync(executor, ["Student", "Executor"]);
        }
        else
        {
            executor = await userManager.FindByEmailAsync(execEmail);
        }

        // Demo student
        var studentEmail = "sara@client.com";
        if (await userManager.FindByEmailAsync(studentEmail) == null)
        {
            var student = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FullName = "Sara Johnson",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(student, "Student@123");
            await userManager.AddToRoleAsync(student, "Student");
        }

        // Categories
        if (!db.Categories.Any())
        {
            var categories = new List<Category>
            {
                new() { Name = "Web Development", Slug = "web-development", Icon = "🌐", Description = "Websites, web apps, APIs and more", SortOrder = 1 },
                new() { Name = "Mobile Apps", Slug = "mobile-apps", Icon = "📱", Description = "iOS and Android development", SortOrder = 2 },
                new() { Name = "UI/UX Design", Slug = "ui-ux-design", Icon = "🎨", Description = "User interfaces and experience design", SortOrder = 3 },
                new() { Name = "Graphic Design", Slug = "graphic-design", Icon = "✏️", Description = "Logos, branding, illustrations", SortOrder = 4 },
                new() { Name = "Digital Marketing", Slug = "digital-marketing", Icon = "📢", Description = "SEO, social media, ads", SortOrder = 5 },
                new() { Name = "Content Writing", Slug = "content-writing", Icon = "✍️", Description = "Copywriting, blog posts, translations", SortOrder = 6 },
                new() { Name = "Video & Animation", Slug = "video-animation", Icon = "🎬", Description = "Video editing, motion graphics", SortOrder = 7 },
                new() { Name = "Data Science", Slug = "data-science", Icon = "📊", Description = "ML, data analysis, dashboards", SortOrder = 8 },
            };
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
        }

        // Services
        if (!db.Services.Any() && executor != null)
        {
            var cats = await db.Categories.ToListAsync();
            var catDict = cats.ToDictionary(c => c.Slug, c => c.Id);

            var services = new List<Service>
            {
                new() { Title = "Professional React Web App", Description = "I'll build a modern, responsive React application with Redux, TypeScript, and a clean UI. Includes authentication, API integration, and deployment.", Price = 299, DeliveryDays = 7, CategoryId = catDict["web-development"], ExecutorId = executor.Id, Tags = "react,typescript,web,frontend", Rating = 4.9, ReviewCount = 47, OrderCount = 52 },
                new() { Title = "Full-Stack ASP.NET Core API", Description = "Complete RESTful API with ASP.NET Core, EF Core, SQL Server/SQLite, authentication, and Swagger docs.", Price = 399, DeliveryDays = 10, CategoryId = catDict["web-development"], ExecutorId = executor.Id, Tags = "aspnet,api,backend,dotnet", Rating = 4.8, ReviewCount = 33, OrderCount = 38 },
                new() { Title = "iOS & Android App (React Native)", Description = "Cross-platform mobile app with React Native. Real device testing, push notifications, and App Store/Play Store guidance.", Price = 599, DeliveryDays = 14, CategoryId = catDict["mobile-apps"], ExecutorId = executor.Id, Tags = "mobile,react-native,ios,android", Rating = 4.7, ReviewCount = 28, OrderCount = 31 },
                new() { Title = "UI/UX Design System in Figma", Description = "Complete design system with components, tokens, and interactive prototypes in Figma. Includes dark mode.", Price = 249, DeliveryDays = 5, CategoryId = catDict["ui-ux-design"], ExecutorId = executor.Id, Tags = "figma,design,ui,ux", Rating = 5.0, ReviewCount = 62, OrderCount = 70 },
                new() { Title = "Logo & Brand Identity", Description = "Professional logo design with full brand identity pack: colors, typography, business card, social media kit.", Price = 149, DeliveryDays = 4, CategoryId = catDict["graphic-design"], ExecutorId = executor.Id, Tags = "logo,branding,design,identity", Rating = 4.9, ReviewCount = 91, OrderCount = 105 },
                new() { Title = "SEO & Content Strategy", Description = "Full SEO audit, keyword research, on-page optimization, and 3-month content calendar. Includes monthly reporting.", Price = 199, DeliveryDays = 7, CategoryId = catDict["digital-marketing"], ExecutorId = executor.Id, Tags = "seo,marketing,content,strategy", Rating = 4.6, ReviewCount = 44, OrderCount = 49 },
                new() { Title = "Professional Blog Writing (10 Articles)", Description = "10 SEO-optimized, engaging blog articles up to 1500 words each. Research included. Any niche.", Price = 120, DeliveryDays = 7, CategoryId = catDict["content-writing"], ExecutorId = executor.Id, Tags = "writing,blog,content,seo", Rating = 4.8, ReviewCount = 77, OrderCount = 88 },
                new() { Title = "Explainer Video Animation", Description = "2-3 minute professional explainer video with custom illustrations, animation, and voiceover.", Price = 349, DeliveryDays = 10, CategoryId = catDict["video-animation"], ExecutorId = executor.Id, Tags = "video,animation,explainer,motion", Rating = 4.7, ReviewCount = 36, OrderCount = 40 },
                new() { Title = "Data Dashboard in Python", Description = "Interactive data dashboard with Plotly/Dash or Streamlit. Data cleaning, visualization, and deployment.", Price = 279, DeliveryDays = 7, CategoryId = catDict["data-science"], ExecutorId = executor.Id, Tags = "python,data,dashboard,visualization", Rating = 4.9, ReviewCount = 23, OrderCount = 26 },
                new() { Title = "E-commerce Website (Shopify/WooCommerce)", Description = "Full e-commerce setup with product listings, payment gateway, shipping config, and custom theme.", Price = 449, DeliveryDays = 12, CategoryId = catDict["web-development"], ExecutorId = executor.Id, Tags = "ecommerce,shopify,woocommerce,store", Rating = 4.8, ReviewCount = 55, OrderCount = 61 },
            };
            db.Services.AddRange(services);
            await db.SaveChangesAsync();
        }

        // Email Templates
        if (!db.EmailTemplates.Any())
        {
            var templates = new List<EmailTemplate>
            {
                new() 
                { 
                    Name = "Welcome Email", 
                    Slug = "welcome-email", 
                    Subject = "Welcome to {{AppName}}!", 
                    Body = "<h1>Hello {{UserName}},</h1><p>Welcome to our platform! We're excited to have you with us.</p><p>Click here to start exploring services: <a href='{{Link}}'>Explore Now</a></p>" 
                },
                new() 
                { 
                    Name = "Order Confirmation", 
                    Slug = "order-confirmation", 
                    Subject = "New Order Placed #{{OrderNumber}}", 
                    Body = "<h3>Order Confirmed</h3><p>Hi {{UserName}},</p><p>Your order #{{OrderNumber}} has been successfully placed and the executor has been notified.</p><p><a href='{{Link}}'>Track your order here</a></p>" 
                },
                new() 
                { 
                    Name = "Support Ticket Reply", 
                    Slug = "ticket-reply", 
                    Subject = "New Reply on Ticket #{{TicketId}}", 
                    Body = "<p>Hi {{UserName}},</p><p>A staff member has replied to your support ticket. Please log in to view the response.</p><p><a href='{{Link}}'>View Ticket</a></p>" 
                }
            };
            db.EmailTemplates.AddRange(templates);
            await db.SaveChangesAsync();
        }
    }
}
