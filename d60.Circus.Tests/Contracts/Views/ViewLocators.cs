﻿using System;
using d60.Circus.Aggregates;
using d60.Circus.Events;
using d60.Circus.MongoDb.Events;
using d60.Circus.Tests.Contracts.Views.Factories;
using d60.Circus.Tests.MongoDb;
using d60.Circus.Views.Basic;
using d60.Circus.Views.Basic.Locators;
using MongoDB.Driver;
using NUnit.Framework;
using TestContext = d60.Circus.TestHelpers.TestContext;

namespace d60.Circus.Tests.Contracts.Views
{
    [TestFixture(typeof(MongoDbPullViewManagerFactory), Category = TestCategories.MongoDb)]
    [TestFixture(typeof(MsSqlPullViewManagerFactory), Category = TestCategories.MsSql)]
    [TestFixture(typeof(EntityFrameworkPullViewManagerFactory), Category = TestCategories.MsSql)]
    public class ViewLocators<TViewManagerFactory> : FixtureBase where TViewManagerFactory : IPullViewManagerFactory, new()
    {
        MongoDatabase _database;
        MongoDbEventStore _eventStore;

        IViewManager _globalInstanceViewManager;
        TViewManagerFactory _factory;
        IViewManager _instancePerAggregateRootViewManager;
        TestContext _testContext;

        protected override void DoSetUp()
        {
            _database = MongoHelper.InitializeTestDatabase();
            _eventStore = new MongoDbEventStore(_database, "events");

            _factory = new TViewManagerFactory();

            _globalInstanceViewManager = _factory.GetPullViewManager<GlobalInstanceViewInstance>();
            _instancePerAggregateRootViewManager = _factory.GetPullViewManager<InstancePerAggregateRootView>();

            _testContext = new TestContext();
        }


        [Test]
        public void WorksWithInstancePerAggregateRootView()
        {
            _testContext.AddViewManager(_instancePerAggregateRootViewManager);

            var rootId1 = Guid.NewGuid();
            var rootId2 = Guid.NewGuid();

            _testContext.Save(rootId1, new ThisIsJustAnEvent());
            _testContext.Save(rootId1, new ThisIsJustAnEvent());
            _testContext.Save(rootId1, new ThisIsJustAnEvent());

            _testContext.Save(rootId2, new ThisIsJustAnEvent());
            _testContext.Save(rootId2, new ThisIsJustAnEvent());
            _testContext.Save(rootId2, new ThisIsJustAnEvent());

            _testContext.Commit();

            var view = _factory.Load<InstancePerAggregateRootView>(InstancePerAggregateRootLocator.GetViewIdFromGuid(rootId1));
            Assert.That(view.EventCounter, Is.EqualTo(3));
        }

        [Test]
        public void WorksWithGlobalInstanceView()
        {
            _testContext.AddViewManager(_globalInstanceViewManager);

            var rootId1 = Guid.NewGuid();
            var rootId2 = Guid.NewGuid();

            _testContext.Save(rootId1, new ThisIsJustAnEvent());
            _testContext.Save(rootId1, new ThisIsJustAnEvent());
            _testContext.Save(rootId1, new ThisIsJustAnEvent());

            _testContext.Save(rootId2, new ThisIsJustAnEvent());
            _testContext.Save(rootId2, new ThisIsJustAnEvent());
            _testContext.Save(rootId2, new ThisIsJustAnEvent());

            _testContext.Commit();

            var view = _factory.Load<GlobalInstanceViewInstance>(GlobalInstanceLocator.GetViewInstanceId());
            Assert.That(view.EventCounter, Is.EqualTo(6));
        }

        [Test]
        public void DoesNotCallViewLocatorForIrrelevantEvents()
        {
            _testContext.AddViewManager(_globalInstanceViewManager);

            _testContext.Save(Guid.NewGuid(), new JustAnEvent());
            _testContext.Save(Guid.NewGuid(), new AnotherEvent());

            Assert.DoesNotThrow(_testContext.Commit);
        }

        class MyViewInstance : IViewInstance<CustomizedViewLocator>, ISubscribeTo<JustAnEvent>
        {
            public string Id { get; set; }
            public long LastGlobalSequenceNumber { get; set; }

            public void Handle(IViewContext context, JustAnEvent domainEvent)
            {

            }
        }
        class CustomizedViewLocator : ViewLocator
        {
            public override string GetViewId(DomainEvent e)
            {
                if (e is AnEvent) return "yay";

                throw new ApplicationException("oh noes!!!!");
            }
        }
    }

    class JustAnEvent : DomainEvent<Root>
    {
    }
    class AnotherEvent : DomainEvent<Root>
    {
    }
    class Root : AggregateRoot
    {
    }
    class GlobalInstanceViewInstance : IViewInstance<GlobalInstanceLocator>, ISubscribeTo<ThisIsJustAnEvent>
    {
        public int EventCounter { get; set; }
        public void Handle(IViewContext context, ThisIsJustAnEvent domainEvent)
        {
            EventCounter++;
        }

        public string Id { get; set; }
        public long LastGlobalSequenceNumber { get; set; }
    }
    class InstancePerAggregateRootView : IViewInstance<InstancePerAggregateRootLocator>, ISubscribeTo<ThisIsJustAnEvent>
    {
        public int EventCounter { get; set; }
        public void Handle(IViewContext context, ThisIsJustAnEvent domainEvent)
        {
            EventCounter++;
        }

        public string Id { get; set; }
        public long LastGlobalSequenceNumber { get; set; }
    }

    class ThisIsJustAnEvent : DomainEvent<Root>
    {
    }
}