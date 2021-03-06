﻿// ==========================================================================
//  StatusFlowTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Xunit;

namespace Squidex.Domain.Apps.Core.Contents
{
    public class StatusFlowTests
    {
        [Fact]
        public void Should_make_tests()
        {
            Assert.True(StatusFlow.Exists(Status.Draft));
            Assert.True(StatusFlow.Exists(Status.Archived));
            Assert.True(StatusFlow.Exists(Status.Published));

            Assert.True(StatusFlow.CanChange(Status.Draft, Status.Archived));
            Assert.True(StatusFlow.CanChange(Status.Draft, Status.Published));

            Assert.True(StatusFlow.CanChange(Status.Published, Status.Draft));
            Assert.True(StatusFlow.CanChange(Status.Published, Status.Archived));

            Assert.True(StatusFlow.CanChange(Status.Archived, Status.Draft));

            Assert.False(StatusFlow.Exists((Status)int.MaxValue));
            Assert.False(StatusFlow.CanChange(Status.Archived, Status.Published));
        }
    }
}
