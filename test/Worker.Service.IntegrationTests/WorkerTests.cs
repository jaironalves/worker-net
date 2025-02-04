﻿using Worker.Service.IntegrationTests.Fixtures;

namespace Worker.Service.IntegrationTests
{
    public class WorkerTests : IClassFixture<WorkerFixture>
    {
        private readonly WorkerFixture fixture;

        public WorkerTests(WorkerFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task TestMessagesProcessing()
        {
            var messages = new List<string>
            {
                "Message 1",
                "Message 2" 
            };

            var postedIds = new List<string>();
            foreach (var message in messages)
            {
                var postedId = await fixture.PostMessageAsync(message, nameof(TestMessagesProcessing));
                postedIds.Add(postedId);
            }

            await fixture.WaitConsumeAsync(postedIds, 60);

            // Verifique se as mensagens foram processadas
            // Adicione suas asserções aqui
            Assert.True(fixture.Worker is null);
        }

        [Fact]
        public async Task TestMessagesProcessing2()
        {
            var messages = new List<string>
            {
                "Message 3",
                "Message 4"
            };

            var postedIds = new List<string>();
            foreach (var message in messages)
            {
                var postedId = await fixture.PostMessageAsync(message, nameof(TestMessagesProcessing2));
                postedIds.Add(postedId);
            }

            await fixture.WaitConsumeAsync(postedIds, 60);

            // Verifique se as mensagens foram processadas
            // Adicione suas asserções aqui
            Assert.True(fixture.Worker is null);
        }
    }
}
