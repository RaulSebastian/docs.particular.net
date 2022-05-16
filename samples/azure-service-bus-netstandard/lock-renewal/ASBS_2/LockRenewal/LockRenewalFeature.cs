﻿using System;
using NServiceBus;
using NServiceBus.Features;

public class LockRenewalFeature : Feature
{
    internal LockRenewalFeature()
    {
        #region LockRenewalFeature

        EnableByDefault();

        Defaults(settings =>
        {
            settings.SetDefault<LockRenewalOptions>(new LockRenewalOptions
            {
                // NServiceBus.Transport.AzureServiceBus sets LockDuration to 5 minutes by default
                LockDuration = TimeSpan.FromMinutes(5),
                RenewalInterval = TimeSpan.FromMinutes(1)
            });
        });

        #endregion
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        var lockRenewalOptions = context.Settings.Get<LockRenewalOptions>();
        var queueName = context.Settings.LocalAddress();

        context.Pipeline.Register(
            stepId: "LockRenewal",
            factoryMethod: builder => new LockRenewalBehavior(lockRenewalOptions.LockDuration, lockRenewalOptions.RenewalInterval, queueName),
            description: "Renew message lock token");
    }
}