using BubbleShop.Application.Common.Interfaces;
using BubbleShop.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BubbleShop.Application.AppServices
{
    public static class ChannelFactoryExtensions
    {

        public static async Task SendMessageAsync(
            this IChannelFactory factory,
            ChannelType channelType,
            string userId,
            string message,
            CancellationToken cancellationToken = default)
        {
            var adapter = factory.GetChannelAdapter(channelType);
            await adapter.SendMessageAsync(userId, message, cancellationToken);
        }


        public static async Task SendTypingIndicatorAsync(
            this IChannelFactory factory,
            ChannelType channelType,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var adapter = factory.GetChannelAdapter(channelType);
            await adapter.SendTypingIndicatorAsync(userId, cancellationToken);
        }

      
        public static async Task BroadcastToAllChannelsAsync(
            this IChannelFactory factory,
            string userId,
            string message,
            CancellationToken cancellationToken = default)
        {
            var tasks = factory.GetAllChannelAdapters()
                .Where(a => a.ChannelType != ChannelType.Unknown)
                .Select(a => a.SendMessageAsync(userId, message, cancellationToken));

            await Task.WhenAll(tasks);
        }

  
        public static List<ChannelType> GetAvailableChannelTypes(this IChannelFactory factory)
        {
            return factory.GetAllChannelAdapters()
                .Select(a => a.ChannelType)
                .Where(c => c != ChannelType.Unknown)
                .ToList();
        }


        public static bool HasAnyAvailableChannel(this IChannelFactory factory)
        {
            return factory.GetAllChannelAdapters().Any();
        }
    }
}
