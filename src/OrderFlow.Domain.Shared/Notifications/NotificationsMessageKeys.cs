using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Shared.Notifications;

/// <summary>Message keys of the Notifications module (ADR-0021).</summary>
public static class NotificationsMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Notifications, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Notifications, nameof(FieldTooLong));

    /// <summary>The channel is not known. Arguments: the channel.</summary>
    public static readonly MessageKey ChannelInvalid = new(Modules.Notifications, nameof(ChannelInvalid));

    /// <summary>The notification is no longer pending. Arguments: the notification id, its status.</summary>
    public static readonly MessageKey NotificationNotPending = new(Modules.Notifications, nameof(NotificationNotPending));
}
