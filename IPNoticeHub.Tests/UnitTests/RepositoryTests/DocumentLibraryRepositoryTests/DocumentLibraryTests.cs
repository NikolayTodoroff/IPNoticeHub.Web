using FluentAssertions;
using IPNoticeHub.Domain.Entities.LegalDocuments;
using IPNoticeHub.Infrastructure.Persistence.Repositories.DocumentLibraryRepository;
using IPNoticeHub.Shared.Enums;
using IPNoticeHub.Tests.IntegrationTests.IntegrationTestFactories;
using IPNoticeHub.Tests.UnitTests.UnitTestFactories;
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

            await act.Should().ThrowAsync<ArgumentNullException>().
                WithParameterName("document");
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

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

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

            var recoveredDocument = 
                await repository.GetUserDocumentsAsync(
                "user-1",
                DocumentSourceType.Trademark,
                LetterTemplateType.CeaseAndDesist,
                CancellationToken.None);

            recoveredDocument.Should().HaveCount(1);
            recoveredDocument.Single().DocumentTitle.Should().Be("TM-CND");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public async Task GetUserDocumentsAsync_With_NullOrWhitespaceUserId_ThrowsArgException(string userId)
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            Func<Task> act = async () => await repository.GetUserDocumentsAsync(
                userId,
                DocumentSourceType.Trademark,
                LetterTemplateType.CeaseAndDesist,
                CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>()
                .WithParameterName("userId")
                .WithMessage("*UserId cannot be null or whitespace.*");
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

            var document1 = CreateDocument(
                "user-1", 
                "Old title");

            var document2 = CreateDocument(
                "user-2", 
                "Other users title");

            testDbContext.LegalDocuments.AddRange(
                document1, 
                document2);

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext,testClock);

            await repository.RenameAsync(
                document1.LegalDocumentId, 
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

            await act.Should().ThrowAsync<ArgumentException>().
                WithParameterName("userId").
                WithMessage("*UserId cannot be null or whitespace.*");
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

            await act.Should().ThrowAsync<ArgumentException>().
                WithParameterName("newTitle").
                WithMessage("*New title cannot be null or whitespace.*");
        }

        [Test]
        public async Task SoftDeleteAsync_DeletesCorrectDocument()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            var document1 = CreateDocument(
                "user1",
                "Document1");

            var document2 = CreateDocument(
                "user2",
                "Document2");

            await repository.AddAsync(
                document1,
                CancellationToken.None);

            await repository.AddAsync(
                document2,
                CancellationToken.None);

            await repository.SoftDeleteAsync(
                document1.LegalDocumentId, 
                document1.ApplicationUserId, 
                CancellationToken.None);

            var deletedDocument = 
                await testDbContext.LegalDocuments.
                IgnoreQueryFilters().
                FirstOrDefaultAsync(
                    d => d.LegalDocumentId == document1.LegalDocumentId);

            deletedDocument.Should().NotBeNull();
            deletedDocument!.IsDeleted.Should().BeTrue();

            var document = await repository.GetDocumentByIdAsync(
                document1.LegalDocumentId, 
                document1.ApplicationUserId, 
                CancellationToken.None);
            
            document.Should().BeNull();
        }

        [Test]
        public async Task SoftDeleteAsync_MarksDocumentAsDeleted_AndPreventsRetrievalForOwner()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            var document1 = CreateDocument(
                "user1",
                "Document1");

            var document2 = CreateDocument(
                "user2",
                "Document2");

            await repository.AddAsync(
                document1,
                CancellationToken.None);

            await repository.AddAsync(
                document2,
                CancellationToken.None);

            await repository.SoftDeleteAsync(
                document1.LegalDocumentId, 
                document1.ApplicationUserId, 
                CancellationToken.None);

            var deletedDocument = 
                await testDbContext.LegalDocuments.
                IgnoreQueryFilters().
                FirstOrDefaultAsync(
                    d => d.LegalDocumentId == document1.LegalDocumentId);

            deletedDocument.Should().NotBeNull();
            deletedDocument!.IsDeleted.Should().BeTrue();

            var document = await repository.GetDocumentByIdAsync(
                document1.LegalDocumentId, 
                document1.ApplicationUserId, 
                CancellationToken.None);
            
            document.Should().BeNull();
        }

        [TestCase(null)]
        [TestCase(" ")]
        [TestCase("")]
        public async Task SoftDeleteAsync_WithNullOrWhitespaceUserId_ThrowsArgException(string userId)
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            Func<Task> act = async () => await repository.SoftDeleteAsync(
               100,
               userId,
               CancellationToken.None);

            await act.Should().ThrowAsync<ArgumentException>().
                WithParameterName("userId").
                WithMessage("*UserId cannot be null or whitespace.*");
        }

        [Test]
        public async Task SoftDeleteAsync_WithNullDocument_Returns()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            Func<Task> act = async () => await repository.SoftDeleteAsync(
               22,
               "randomId",
               CancellationToken.None);

            await act.Should().NotThrowAsync();

            testDbContext.LegalDocuments.Should().BeEmpty();
        }

        [Test]
        public async Task SoftDeleteAsync_WithNonExistentDocument_DoesNotAffectOtherDocuments()
        {
            using var testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var testClock = new TestClock();

            var repository =
                new DocumentLibraryRepository(testDbContext, testClock);

            var existingDocument = CreateDocument(
                "user-1", 
                "Existing Doc");

            await repository.AddAsync(
                existingDocument, 
                CancellationToken.None);

            var fakeDocumentId = existingDocument.LegalDocumentId + 999;

            await repository.SoftDeleteAsync(
                fakeDocumentId,
                "user-2",
                CancellationToken.None);

            var unchangedDoc = await testDbContext.LegalDocuments.
                IgnoreQueryFilters().
                FirstOrDefaultAsync(
                d => d.LegalDocumentId == existingDocument.LegalDocumentId);

            unchangedDoc.Should().NotBeNull();
            unchangedDoc!.IsDeleted.Should().BeFalse();
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
