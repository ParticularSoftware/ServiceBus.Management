﻿namespace ServiceControl.Recoverability.Groups.Archive
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using Raven.Abstractions.Data;
    using Raven.Client;
    using ServiceControl.Recoverability.Groups.Indexes;
    using ServiceControl.MessageFailures;

    public class ArchiveAllInGroupHandler : IHandleMessages<ArchiveAllInGroup>
    {
        public IDocumentStore Store { get; set; }
        public IBus Bus { get; set; }

        public void Handle(ArchiveAllInGroup message)
        {
            if (String.IsNullOrWhiteSpace(message.GroupId))
            {
                return;
            }

            var indexName = new MessageFailuresByFailureGroupIdIndex().IndexName;

            var operation = Store.DatabaseCommands.UpdateByIndex(indexName, 
                new IndexQuery
                {
                    Query = "FailureGroups_Id:" + message.GroupId
                },
                new[]
                {
                    new PatchRequest
                    {
                        Type = PatchCommandType.Set,
                        Name = "Status",
                        Value = (int) FailedMessageStatus.Archived
                    }
                },
                allowStale: true);

            operation.WaitForCompletionAsync().ContinueWith(result =>
            {
                Bus.Publish(new FailedMessageGroupArchived
                {
                    GroupId = message.GroupId
                });
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}