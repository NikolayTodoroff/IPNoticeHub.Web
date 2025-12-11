using IPNoticeHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace IPNoticeHub.Data.Migrations
{
    [DbContext(typeof(IPNoticeHubDbContext))]
    partial class IPNoticeHubDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.19")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("IPNoticeHub.Data.Entities.CopyrightRegistration.CopyrightEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateOfPublication")
                        .HasColumnType("datetime2");

                    b.Property<string>("NationOfFirstPublication")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<Guid>("PublicId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("RegistrationNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("TypeOfWork")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("YearOfCreation")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationNumber")
                        .IsUnique();

                    b.ToTable("CopyrightRegistrations");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DateOfPublication = new DateTime(2020, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            NationOfFirstPublication = "United States",
                            Owner = "Nikolay Todorov",
                            PublicId = new Guid("076d6d16-235d-40e7-b419-da5465d8ebdf"),
                            RegistrationNumber = "VA0002288838",
                            Title = "Astronaut Music DJ",
                            TypeOfWork = "Visual Material",
                            YearOfCreation = 2020
                        });
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "2b195b12-9690-46b9-ac8e-50118a7102ea",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "73dd29f5-cb9c-4f43-84ae-73597cded863",
                            Email = "testuser1@example.com",
                            EmailConfirmed = true,
                            LockoutEnabled = false,
                            NormalizedEmail = "TESTUSER1@EXAMPLE.COM",
                            NormalizedUserName = "TESTUSER1@EXAMPLE.COM",
                            PasswordHash = "AQAAAAIAAYagAAAAEFakeHash1234567890==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "6fc62e55-dbc8-4022-84b4-cd49bdb663a3",
                            TwoFactorEnabled = false,
                            UserName = "testuser1@example.com"
                        },
                        new
                        {
                            Id = "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "d1d41029-3bbf-4476-890a-d64104220466",
                            Email = "testuser2@example.com",
                            EmailConfirmed = true,
                            LockoutEnabled = false,
                            NormalizedEmail = "TESTUSER2@EXAMPLE.COM",
                            NormalizedUserName = "TESTUSER2@EXAMPLE.COM",
                            PasswordHash = "AQAAAAIAAYagFakeHash0987654321==",
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "abc60a70-c408-4762-be78-c7c0d1ea3b9b",
                            TwoFactorEnabled = false,
                            UserName = "testuser2@example.com"
                        });
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.UserCopyright", b =>
                {
                    b.Property<string>("ApplicationUserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("CopyrightRegistrationId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.HasKey("ApplicationUserId", "CopyrightRegistrationId");

                    b.HasIndex("CopyrightRegistrationId");

                    b.HasIndex("ApplicationUserId", "CopyrightRegistrationId")
                        .IsUnique();

                    b.ToTable("UserCopyrights");

                    b.HasData(
                        new
                        {
                            ApplicationUserId = "2b195b12-9690-46b9-ac8e-50118a7102ea",
                            CopyrightRegistrationId = 1,
                            DateAdded = new DateTime(2025, 12, 7, 23, 3, 56, 409, DateTimeKind.Utc).AddTicks(4279),
                            IsDeleted = false
                        });
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.UserTrademark", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("TrademarkId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.HasKey("UserId", "TrademarkId");

                    b.HasIndex("TrademarkId");

                    b.ToTable("UserTrademarks");

                    b.HasData(
                        new
                        {
                            UserId = "2b195b12-9690-46b9-ac8e-50118a7102ea",
                            TrademarkId = 1,
                            DateAdded = new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            IsDeleted = false
                        },
                        new
                        {
                            UserId = "4d8f7a3e-cb13-42f4-bf61-0a8c301a3f8b",
                            TrademarkId = 2,
                            DateAdded = new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                            IsDeleted = false
                        });
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.UserTrademarkWatchlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AddedOnUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("InitialStatusCodeRaw")
                        .HasColumnType("int");

                    b.Property<DateTime?>("InitialStatusDateUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("InitialStatusText")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<bool>("NotificationsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<int>("TrademarkId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("TrademarkId");

                    b.HasIndex("UserId", "TrademarkId")
                        .IsUnique()
                        .HasFilter("[IsDeleted] = 0");

                    b.ToTable("UserTrademarkWatchlists");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.LegalDocuments.LegalDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AdditionalFacts")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("BodyTemplate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateOfPublication")
                        .HasColumnType("datetime2");

                    b.Property<string>("DocumentTitle")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("GoodFaithStatement")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("InfringingUrl")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("IpTitle")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LetterDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("NationOfFirstPublication")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("RecipientAddress")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("RecipientEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("RecipientName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("RegistrationNumber")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("RelatedPublicId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SenderAddress")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("SenderEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("SenderName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("SourceType")
                        .HasColumnType("int");

                    b.Property<int>("TemplateType")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("YearOfCreation")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("LegalDocuments");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkClassAssignment", b =>
                {
                    b.Property<int>("TrademarkRegistrationId")
                        .HasColumnType("int");

                    b.Property<int>("ClassNumber")
                        .HasColumnType("int");

                    b.HasKey("TrademarkRegistrationId", "ClassNumber");

                    b.ToTable("TrademarkClassAssignment");

                    b.HasData(
                        new
                        {
                            TrademarkRegistrationId = 1,
                            ClassNumber = 25
                        },
                        new
                        {
                            TrademarkRegistrationId = 2,
                            ClassNumber = 25
                        },
                        new
                        {
                            TrademarkRegistrationId = 2,
                            ClassNumber = 28
                        });
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("FilingDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("GoodsAndServices")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("nvarchar(4000)");

                    b.Property<string>("MarkImageUrl")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<string>("Owner")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<Guid>("PublicId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("RegistrationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("RegistrationNumber")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Source")
                        .HasColumnType("int");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("StatusCategory")
                        .HasColumnType("int");

                    b.Property<int?>("StatusCodeRaw")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StatusDateUtc")
                        .HasColumnType("datetime2");

                    b.Property<string>("StatusDetail")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<string>("Wordmark")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.HasKey("Id");

                    b.HasIndex("RegistrationNumber");

                    b.HasIndex("SourceId");

                    b.HasIndex("Source", "SourceId")
                        .IsUnique()
                        .HasDatabaseName("UX_Trademark_Source_SourceId");

                    b.ToTable("TrademarkRegistrations");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            FilingDate = new DateTime(2010, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            GoodsAndServices = "Clothing, footwear, headgear",
                            Owner = "Nike Inc.",
                            PublicId = new Guid("4dd1f011-5362-42ff-9853-33ccbe4aa935"),
                            RegistrationDate = new DateTime(2012, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            RegistrationNumber = "54321",
                            Source = 0,
                            SourceId = "123456",
                            StatusCategory = 1,
                            StatusDetail = "Live/Registered",
                            Wordmark = "Nike"
                        },
                        new
                        {
                            Id = 2,
                            FilingDate = new DateTime(2015, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            GoodsAndServices = "Sports equipment, clothing",
                            Owner = "Adidas AG",
                            PublicId = new Guid("fdb6b78f-6f5c-42ec-8b36-9a958011168b"),
                            RegistrationDate = new DateTime(2017, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            RegistrationNumber = "98765",
                            Source = 0,
                            SourceId = "654321",
                            StatusCategory = 1,
                            StatusDetail = "Live/Registered",
                            Wordmark = "Adidas"
                        });
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<string>("EventTypeRaw")
                        .HasMaxLength(2)
                        .HasColumnType("nvarchar(2)");

                    b.Property<int>("TrademarkId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TrademarkId");

                    b.ToTable("TrademarkEvents");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.UserCopyright", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", "ApplicationUser")
                        .WithMany("UserCopyrights")
                        .HasForeignKey("ApplicationUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("IPNoticeHub.Data.Entities.CopyrightRegistration.CopyrightEntity", "CopyrightRegistration")
                        .WithMany("UserCopyrights")
                        .HasForeignKey("CopyrightRegistrationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ApplicationUser");

                    b.Navigation("CopyrightRegistration");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.UserTrademark", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEntity", "Trademark")
                        .WithMany("UserTrademarks")
                        .HasForeignKey("TrademarkId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", "User")
                        .WithMany("UserTrademarks")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Trademark");

                    b.Navigation("User");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.UserTrademarkWatchlist", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEntity", "Trademark")
                        .WithMany()
                        .HasForeignKey("TrademarkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Trademark");

                    b.Navigation("User");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.LegalDocuments.LegalDocument", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", "User")
                        .WithMany("Documents")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkClassAssignment", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEntity", "TrademarkRegistration")
                        .WithMany("Classes")
                        .HasForeignKey("TrademarkRegistrationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrademarkRegistration");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEvent", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEntity", "TrademarkRegistration")
                        .WithMany("Events")
                        .HasForeignKey("TrademarkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TrademarkRegistration");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("IPNoticeHub.Data.Entities.Identity.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.CopyrightRegistration.CopyrightEntity", b =>
                {
                    b.Navigation("UserCopyrights");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.Identity.ApplicationUser", b =>
                {
                    b.Navigation("Documents");

                    b.Navigation("UserCopyrights");

                    b.Navigation("UserTrademarks");
                });

            modelBuilder.Entity("IPNoticeHub.Data.Entities.TrademarkRegistration.TrademarkEntity", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("Events");

                    b.Navigation("UserTrademarks");
                });
#pragma warning restore 612, 618
        }
    }
}
