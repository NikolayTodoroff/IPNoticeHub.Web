using FluentAssertions;
using IPNoticeHub.Common.EnumConstants;
using IPNoticeHub.Data;
using IPNoticeHub.Data.Entities.LegalDocuments;
using IPNoticeHub.Data.Repositories.DocumentLibrary.Implementations;
using IPNoticeHub.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace IPNoticeHub.Tests.UnitTests.RepositoryTests.DocumentLibraryRepositoryTests
{
    [TestFixture]
    public class DocumentLibraryTests
    {
        [Test]
        public async Task AddAsync_PersistsDocument_AndReturnsGeneratedId()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            var repository = 
                new DocumentLibraryRepository(testDbContext);

            var document = CreateDocument("user-1", "My first document");

            var id = await repository.AddAsync(document, CancellationToken.None);

            id.Should().BeGreaterThan(0);
            document.Id.Should().Be(id);

            var recoveredDocument = await testDbContext.LegalDocuments.SingleAsync();
            recoveredDocument.Id.Should().Be(id);
            recoveredDocument.DocumentTitle.Should().Be("My first document");
            recoveredDocument.UserId.Should().Be("user-1");
        }

        [Test]
        public async Task GetNonDeletedUserDocumentsAsync_ReturnsOnlyUsersDocuments()
        {
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            testDbContext.LegalDocuments.AddRange(
                CreateDocument("user-1", "u1-doc-1"),
                CreateDocument("user-1", "u1-deleted", isDeleted: true),
                CreateDocument("user-2", "u2-doc-1"));

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext);

            var recoveredDocument = await repository.GetUserDocumentsAsync(
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
            using IPNoticeHubDbContext? testDbContext = 
                InMemoryDbContextFactory.CreateTestDbContext();

            testDbContext.LegalDocuments.AddRange(
                CreateDocument("user-1", "TM-CND",
                    source: DocumentSourceType.Trademark,
                    template: LetterTemplateType.CeaseAndDesist),
                CreateDocument("user-1", "CP-DMCA",
                    source: DocumentSourceType.Copyright,
                    template: LetterTemplateType.Dmca),
                CreateDocument("user-1", "TM-DMCA",
                    source: DocumentSourceType.Trademark,
                    template: LetterTemplateType.Dmca));

            await testDbContext.SaveChangesAsync();

            var repository = 
                new DocumentLibraryRepository(testDbContext);

            var recoveredDocument = await repository.GetUserDocumentsAsync(
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
            using IPNoticeHubDbContext? testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var doc1 = CreateDocument("user-1", "Owned");
            var doc2 = CreateDocument("user-1", "Deleted", isDeleted: true);
            var doc3 = CreateDocument("user-2", "Other user");

            testDbContext.LegalDocuments.AddRange(doc1, doc2, doc3);
            await testDbContext.SaveChangesAsync();

            var repository = new DocumentLibraryRepository(testDbContext);

            var recoveredDocument = await repository.GetDocumentByIdAsync(
                doc1.Id, 
                "user-1", 
                CancellationToken.None);
            
            recoveredDocument.Should().NotBeNull();
            recoveredDocument!.DocumentTitle.Should().Be("Owned");

            var deletedDocument = await repository.GetDocumentByIdAsync(
                doc2.Id, 
                "user-1", 
                CancellationToken.None);

            deletedDocument.Should().BeNull();

            var foreignDocument = await repository.GetDocumentByIdAsync(
                doc3.Id, 
                "user-1", 
                CancellationToken.None);

            foreignDocument.Should().BeNull();
        }

        [Test]
        public async Task RenameAsync_UpdatesTitle_OnlyForMatchingUser()
        {
            using IPNoticeHubDbContext? testDbContext =
                InMemoryDbContextFactory.CreateTestDbContext();

            var doc1 = CreateDocument("user-1", "Old title");
            var doc2 = CreateDocument("user-2", "Other users title");

            testDbContext.LegalDocuments.AddRange(doc1, doc2);
            await testDbContext.SaveChangesAsync();

            var repository = new DocumentLibraryRepository(testDbContext);

            await repository.RenameAsync(doc1.Id, "user-1", "New title", CancellationToken.None);

            var recoveredDocuments = await testDbContext.LegalDocuments.
                OrderBy(d => d.Id).
                ToListAsync();

            recoveredDocuments[0].DocumentTitle.Should().Be("New title");
            recoveredDocuments[1].DocumentTitle.Should().Be("Other users title");
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
                UserId = userId,
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
