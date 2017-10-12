﻿// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppDomainObject : DomainObjectBase
    {
        private App app;

        public string Name
        {
            get { return app.Name; }
        }

        public string PlanId
        {
            get { return app.PlanId; }
        }

        public int ContributorCount
        {
            get { return app.Contributors.Count; }
        }

        public AppDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(AppCreated @event)
        {
            app = App.Create(Id, @event.Name);
        }

        protected void On(AppContributorAssigned @event)
        {
            app = app.UpdateContributors(c => c.Assign(@event.ContributorId, @event.Permission));
        }

        protected void On(AppContributorRemoved @event)
        {
            app = app.UpdateContributors(c => c.Remove(@event.ContributorId));
        }

        protected void On(AppClientAttached @event)
        {
            app = app.UpdateClients(c => c.Add(@event.Id, @event.Secret));
        }

        protected void On(AppClientUpdated @event)
        {
            app = app.UpdateClients(c => c.Update(@event.Id, @event.Permission));
        }

        protected void On(AppClientRenamed @event)
        {
            app = app.UpdateClients(c => c.Rename(@event.Id, @event.Name));
        }

        protected void On(AppClientRevoked @event)
        {
            app = app.UpdateClients(c => c.Revoke(@event.Id));
        }

        protected void On(AppLanguageAdded @event)
        {
            app = app.UpdateLanguages(c => c.Add(@event.Language));
        }

        protected void On(AppLanguageRemoved @event)
        {
            app = app.UpdateLanguages(c => c.Remove(@event.Language));
        }

        protected void On(AppLanguageUpdated @event)
        {
            app = app.UpdateLanguages(c => c.Update(@event.Language, @event.IsOptional, @event.IsMaster, @event.Fallback));
        }

        protected void On(AppPlanChanged @event)
        {
            app = app.ChangePlan(@event.PlanId, @event.Actor);
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Create(CreateApp command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create app");

            ThrowIfCreated();

            var appId = new NamedId<Guid>(command.AppId, command.Name);

            RaiseEvent(SimpleMapper.Map(command, new AppCreated { AppId = appId }));

            RaiseEvent(SimpleMapper.Map(command, CreateInitialOwner(appId, command)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialLanguage(appId)));

            return this;
        }

        public AppDomainObject UpdateClient(UpdateClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update client");

            ThrowIfNotCreated();

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientRenamed()));
            }

            if (command.Permission.HasValue)
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientUpdated { Permission = command.Permission.Value }));
            }

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot assign contributor");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject AttachClient(AttachClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot attach client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached()));

            return this;
        }

        public AppDomainObject RevokeClient(RevokeClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot revoke client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));

            return this;
        }

        public AppDomainObject AddLanguage(AddLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot add language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));

            return this;
        }

        public AppDomainObject RemoveLanguage(RemoveLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));

            return this;
        }

        public AppDomainObject UpdateLanguage(UpdateLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageUpdated()));

            return this;
        }

        public AppDomainObject ChangePlan(ChangePlan command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot change plan");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPlanChanged()));

            return this;
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = new NamedId<Guid>(Id, Name);
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private static AppLanguageAdded CreateInitialLanguage(NamedId<Guid> id)
        {
            return new AppLanguageAdded { AppId = id, Language = Language.EN };
        }

        private static AppContributorAssigned CreateInitialOwner(NamedId<Guid> id, SquidexCommand command)
        {
            return new AppContributorAssigned { AppId = id, ContributorId = command.Actor.Identifier, Permission = AppContributorPermission.Owner };
        }

        private void ThrowIfNotCreated()
        {
            if (app == null)
            {
                throw new DomainException("App has not been created.");
            }
        }

        private void ThrowIfCreated()
        {
            if (app != null)
            {
                throw new DomainException("App has already been created.");
            }
        }
    }
}
