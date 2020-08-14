using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ImageOptimizer.Data;

namespace ImageOptimizer.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20160621130443_InitialDatabase")]
    partial class InitialDatabase
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20901")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ImageOptimizer.Models.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("City")
                        .IsRequired();

                    b.Property<string>("CompanyName");

                    b.Property<int>("CountryId");

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<string>("State");

                    b.Property<string>("StreetAddress")
                        .IsRequired();

                    b.Property<string>("ZipCode")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("ImageOptimizer.Models.ApiKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Key")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<string>("Label")
                        .HasAnnotation("MaxLength", 512);

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("ApiKeys");
                });

            modelBuilder.Entity("ImageOptimizer.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("ImageOptimizer.Models.ConvertedImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FileType")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<bool>("IsLossless");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int>("OptimizationLevel");

                    b.Property<TimeSpan>("OptimizationTime");

                    b.Property<long>("OptimizedSize");

                    b.Property<int>("OriginalImageId");

                    b.Property<long>("OriginalSize");

                    b.HasKey("Id");

                    b.HasIndex("OriginalImageId");

                    b.ToTable("ConvertedImages");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Alpha2Code")
                        .HasAnnotation("MaxLength", 2);

                    b.Property<string>("Alpha3Code")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<string>("EnglishName");

                    b.Property<int>("NumericCode");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Image", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("FileType")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<bool>("IsLossless");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int>("OptimizationLevel");

                    b.Property<TimeSpan>("OptimizationTime");

                    b.Property<long>("OptimizedSize");

                    b.Property<long>("OriginalSize");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Invoice", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<DateTime>("Date");

                    b.Property<decimal>("GrossAmount");

                    b.Property<string>("Item");

                    b.Property<decimal>("NetAmount");

                    b.Property<string>("TransactionId");

                    b.Property<decimal>("VatAmount");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("Invoices");
                });

            modelBuilder.Entity("ImageOptimizer.Models.RolePermission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AllowedFileExtensions");

                    b.Property<string>("AllowedFileTypes");

                    b.Property<int>("AllowedImageSize");

                    b.Property<bool>("CanConvertToApng");

                    b.Property<bool>("CanConvertToJpeg2000");

                    b.Property<bool>("CanConvertToJxr");

                    b.Property<bool>("CanConvertToWebp");

                    b.Property<bool>("CanOptimizeLossy");

                    b.Property<bool>("CanUseMultisite");

                    b.Property<int>("ImageLimitPerMonth");

                    b.Property<int>("OptimizationLevel");

                    b.Property<string>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RolePermissions");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("PaymentGatewaySubscriptionId");

                    b.Property<string>("Status");

                    b.Property<int>("SubscriptionPlanId");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.HasIndex("SubscriptionPlanId");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("ImageOptimizer.Models.SubscriptionPlan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("PaymentGatewaySubscriptionPlanId");

                    b.Property<decimal>("Price");

                    b.Property<string>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("SubscriptionPlans");
                });

            modelBuilder.Entity("ImageOptimizer.Models.UserMonthlyOptimization", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<DateTime>("Date");

                    b.Property<int>("MonthlyOptimizedImages");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.ToTable("UserMonthlyOptimizations");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Address", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("ImageOptimizer.Models.ApiKey", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("ImageOptimizer.Models.ConvertedImage", b =>
                {
                    b.HasOne("ImageOptimizer.Models.Image")
                        .WithMany()
                        .HasForeignKey("OriginalImageId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ImageOptimizer.Models.Image", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Invoice", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("ImageOptimizer.Models.RolePermission", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("ImageOptimizer.Models.Subscription", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");

                    b.HasOne("ImageOptimizer.Models.SubscriptionPlan")
                        .WithMany()
                        .HasForeignKey("SubscriptionPlanId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ImageOptimizer.Models.SubscriptionPlan", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("ImageOptimizer.Models.UserMonthlyOptimization", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ImageOptimizer.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
