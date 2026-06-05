using Hogs.RPG.Core.Entities.TradeObjects;
using Hogs.RPG.Core.Enums;
using System;
using System.Collections.Generic;

namespace Hogs.RPG.Services.TradeServices
{
    public class TradeService
    {
        private readonly Dictionary<ulong, TradeSession> _activeTrades = new();

        public bool HasActiveTrade(ulong userId)
        {
            if (!_activeTrades.TryGetValue(userId, out var trade))
                return false;

            return trade.State == TradeState.Pending || trade.State == TradeState.Active || trade.State == TradeState.Confirming;
        }

        public TradeSession GetTrade(ulong userId)
            => _activeTrades.TryGetValue(userId, out var trade) ? trade : null;

        public TradeSession CreateTrade(ulong player1, ulong player2)
        {
            var session = new TradeSession
            {
                Player1Id = player1,
                Player2Id = player2,
                LastUpdatedAt = DateTime.UtcNow,
                WarningSent = false
            };

            _activeTrades[player1] = session;
            _activeTrades[player2] = session;

            return session;
        }

        public List<TradeSession> GetAllTrades()
        {
            return _activeTrades.Values.Distinct().ToList();
        }

        public List<TradeSession> GetExpiredTrades(TimeSpan timeout)
        {
            var now = DateTime.UtcNow;

            return _activeTrades.Values
                .Distinct()
                .Where(t => now - t.LastUpdatedAt > timeout)
                .ToList();
        }

        public void CleanupExpiredTrades(TimeSpan timeout)
        {
            var expired = GetExpiredTrades(timeout);

            foreach (var trade in expired)
            {
                RemoveTrade(trade);
            }
        }

        public void RemoveTrade(TradeSession session)
        {
            _activeTrades.Remove(session.Player1Id);
            _activeTrades.Remove(session.Player2Id);
        }

        //HELPER

        public bool HasPendingTrade(ulong userId)
        {
            return _activeTrades.TryGetValue(userId, out var trade)
                && trade.State == TradeState.Pending;
        }
    }
}