using FluentAssertions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Infrastructure.Persistence.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.UnitTests.TestFactories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.DocumentLibraryRepositoryTests
{
    [TestFixture]
    public class DocumentLibraryTests
    {
        [Test]
        public async Task AddAsync_PersistsDocument_AndReturnsGeneratedId()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository = 
                new DocumentLibraryRepository(testDbContext,testClock);

            var document = CreateDocument(
                "user-1", 
                "My first document");

            var id = await repository.AddAsync(
                document, 
                CancellationToken.None);

            id.Should().BeGreaterThan(0);
            document.LegalDocumentId.Should().Be(id);

            var recoveredDocument = 
                await testDbContext.LegalDocuments.SingleAsync();

            recoveredDocument.LegalDocumentId.Should().Be(id);
            recoveredDocument.DocumentTitle.Should().Be("My first document");
            recoveredDocument.ApplicationUserId.Should().Be("user-1");
        }

        [Test]
        public async Task AddAsync_WithNullDocument_ThrowsNullArgumentException()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext,testClock);

            Func<Task> act = async () => await repository.AddAsync(
                null!,
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("document");
        }

        [Test]
        public async Task AddAsync_SetsCreatedOnToCurrentUtcTime_WhenCreatingNewDocument()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();
            
            var repository =
                new DocumentLibraryRepository(testDbContext,testClock);

            var document = CreateDocument(
                "user-1",
                "My first document");

            document.CreatedOn = default;

            await repository.AddAsync(document,CancellationToken.None);

            document.CreatedOn.Should().Be(testClock.UtcNow);
        }

        [Test]
        public async Task AddAsync_DoesNotOverwriteCreatedOn_WhenAlreadySet()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository = 
                new DocumentLibraryRepository(testDbContext, testClock);

            var defaultTime = 
                new DateTime(2020, 05, 06,
                07, 08, 09, DateTimeKind.Utc);

            var document = CreateDocument(
                userId: "user-1", 
                title: "My first document");

            document.CreatedOn = defaultTime;

            await repository.AddAsync(document, CancellationToken.None);

            document.CreatedOn.Should().Be(defaultTime);
        }

        [Test]
        public async Task GetNonDeletedUserDocumentsAsync_ReturnsOnlyUsersDocuments()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            testDbContext.LegalDocuments.AddRange(
                CreateDocument(
                    "user-1", 
                    "u1-doc-1"),
                CreateDocument(
                    "user-1", 
                    "u1-deleted", 
                    isDeleted: true),
                CreateDocument(
                    "user-2", 
                    "u2-doc-1"));

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext,testClock);

            var recoveredDocument = 
                await repository.GetUserDocumentsAsync(
                "user-1",
                null, 
                null, 
                CancellationToken.None);

            recoveredDocument.Should().HaveCount(1);
            recoveredDocument.Single().DocumentTitle.Should().Be("u1-doc-1");
        }

        [Test]
        public async Task GetUserDocumentsAsync_AppliesSourceAndTemplateFilters()
        {
            using var testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            testDbContext.LegalDocuments.AddRange(
                CreateDocument(
                    "user-1", 
                    "TM-CND",
                    source: DocumentSourceType.Trademark,
                    template: LetterTemplateType.CeaseAndDesist),

                CreateDocument(
                    "user-1", 
                    "CP-DMCA",
                    source: DocumentSourceType.Copyright,
                    template: LetterTemplateType.Dmca),

                CreateDocument(
                    "user-1", 
                    "TM-DMCA",
                    source: DocumentSourceType.Trademark,
                    template: LetterTemplateType.Dmca));

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext,testClock);

            var recoveredDocument = 
                await repository.GetUserDocumentsAsync(
                "user-1",
                DocumentSourceType.Trademark,
                LetterTemplateType.CeaseAndDesist,
                CancellationToken.None);

            recoveredDocument.Should().HaveCount(1);
            recoveredDocument.Single().DocumentTitle.Should().Be("TM-CND");
        }

        [Test]
        public async Task GetNotDeletedDocumentByIdAsync_ReturnsDocument_ForOwner()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var document1 = CreateDocument(
                "user-1", 
                "Owned");

            var document2 = CreateDocument(
                "user-1", 
                "Deleted", 
                isDeleted: true);

            var document3 = CreateDocument(
                "user-2", 
                "Other user");

            testDbContext.LegalDocuments.AddRange(
                document1, 
                document2, 
                document3);

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext,testClock);

            var recoveredDocument = await 
                repository.GetDocumentByIdAsync(
                document1.LegalDocumentId, 
                "user-1", 
                CancellationToken.None);
            
            recoveredDocument.Should().NotBeNull();
            recoveredDocument!.DocumentTitle.Should().Be("Owned");

            var deletedDocument = 
                await repository.GetDocumentByIdAsync(
                document2.LegalDocumentId, 
                "user-1", 
                CancellationToken.None);

            deletedDocument.Should().BeNull();

            var foreignDocument = 
                await repository.GetDocumentByIdAsync(
                document3.LegalDocumentId, 
                "user-1", 
                CancellationToken.None);

            foreignDocument.Should().BeNull();
        }

        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("")]
        public async Task GetDocumentByIdAsync_WithNullOrWhitespace_UserId_ThrowsArgException(string userId)
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            await testDbContext.SaveChangesAsync();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            Func<Task> act = async () => await repository.GetDocumentByIdAsync(
                12,
                userId,
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("userId")
                .WithMessage("*UserId cannot be null or whitespace.*");
        }

        [Test]
        public async Task RenameAsync_UpdatesTitle_OnlyForMatchingUser()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var doc1 = CreateDocument(
                "user-1", 
                "Old title");

            var doc2 = CreateDocument(
                "user-2", 
                "Other users title");

            testDbContext.LegalDocuments.AddRange(
                doc1, 
                doc2);

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext,testClock);

            await repository.RenameAsync(
                doc1.LegalDocumentId, 
                "user-1", 
                "New title", 
                CancellationToken.None);

            var recoveredDocuments = await testDbContext.LegalDocuments.
                OrderBy(d => d.LegalDocumentId).
                ToListAsync();

            recoveredDocuments[0].DocumentTitle.Should().Be("New title");
            recoveredDocuments[1].DocumentTitle.Should().Be("Other users title");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task RenameAsync_With_NullOrWhiteSpaceUserId_ThrowsArgException(string userId)
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
               new DocumentLibraryRepository(testDbContext, testClock);

            Func<Task> act = async () => await repository.RenameAsync(
                100,
                userId,
                "New title",
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("userId")
                .WithMessage("*UserId cannot be null or whitespace.*");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task RenameAsync_With_NullOrWhiteSpaceNewName_ThrowsArgException(string newTitle)
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
               new DocumentLibraryRepository(testDbContext, testClock);

            Func<Task> act = async () => await repository.RenameAsync(
                100,
                "testUserId",
                newTitle,
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("newTitle")
                .WithMessage("*New title cannot be null or whitespace.*");
        }

        private static LegalDocument CreateDocument(
            string userId,
            string title,
            bool isDeleted = false,
            DocumentSourceType source = DocumentSourceType.Trademark,
            LetterTemplateType template = LetterTemplateType.CeaseAndDesist)
        {
            return new LegalDocument
            {
                ApplicationUserId = userId,
                RelatedPublicId = Guid.NewGuid(),
                SourceType = source,
                TemplateType = template,
                DocumentTitle = title,
                BodyTemplate = "Body",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = isDeleted,

                SenderName = "TestSender",
                SenderAddress = "TestSenderAddress",
                RecipientName = "TestRecipient",
                RecipientAddress = "TestRecipientAddress"
            };
        }
    }
}
