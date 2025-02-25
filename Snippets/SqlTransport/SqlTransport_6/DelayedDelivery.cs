﻿using System;
using NServiceBus;

class DelayedDelivery
{
    void Configure(EndpointConfiguration endpointConfiguration)
    {        
        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        var delayedDeliverySettings = transport.NativeDelayedDelivery();

        #region DelayedDeliveryTableSuffix

        delayedDeliverySettings.TableSuffix("Delayed");

        #endregion

        #region DelayedDeliveryProcessingInterval

        delayedDeliverySettings.ProcessingInterval(TimeSpan.FromSeconds(5));

        #endregion

        #region DelayedDeliveryBatchSize

        delayedDeliverySettings.BatchSize(100);

        #endregion

        #region DelayedDeliveryEnableTM

        delayedDeliverySettings.EnableTimeoutManagerCompatibility();

        #endregion
    }
}
