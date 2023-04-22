﻿namespace VolumeShot.Models
{
    internal enum Method
    {
        CancelAllOrdersAsync,
        ReDistanceAsync,
        StopLossAsync,
        AccountUpdate,
        OrderUpdate,
        OpenOrder,
        SetDistances,
        ClosePositionsAsync,
        GetPositionInformationAsync,
        OpenOrderMarketAsync,
        OpenOrderTakeProfitAsync,
        OpenOrderLimitAsync,
        ReDistanceChengeVolumeAsync
    }
}
